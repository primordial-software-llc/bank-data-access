using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;

namespace FinanceApi.Routes.Authenticated
{
    public class PostPurchase : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/purchase";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var model = JsonConvert.DeserializeObject<PurchaseModel>(request.Body);
            if (!model.AgreedToBillingTerms)
            {
                throw new Exception("Must agree to billing terms");
            }
            Subscription subscription;
            try
            {
                subscription = Purchase(
                    user.Email,
                    model.CardCvc,
                    model.CardNumber,
                    model.CardExpirationMonth,
                    model.CardExpirationYear);
            }
            catch (StripeException stripeException)
            {
                response.Body = new JObject
                {
                    {"status", stripeException.Message}
                }.ToString();
                response.StatusCode = 400;
                return;
            }
            response.Body = JsonConvert.SerializeObject(subscription);
            var jsonPatch = new JObject
            {
                ["billingAgreement"] = JObject.FromObject(new BillingAgreement
                {
                    AgreedToBillingTerms = model.AgreedToBillingTerms,
                    Date = DateTime.UtcNow.ToString("O"),
                    IpAddress = IpLookup.GetIp(request)
                })
            };
            new UserService().UpdateUser(user.Email, jsonPatch);
        }

        public Subscription Purchase(string email, string cardCvc, string cardNumber, long cardExpirationMonth, long cardExpirationYear)
        {
            StripeConfiguration.ApiKey = Configuration.STRIPE_API_SECRET_KEY;
            var customerSvc = new CustomerService();
            var customer = GetOrCreateCustomer(customerSvc, email);
            var paymentMethodService = new PaymentMethodService();
            var paymentMethod = paymentMethodService.Create(
                new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardCreateOptions
                    {
                        Cvc = cardCvc,
                        Number = cardNumber,
                        ExpMonth = cardExpirationMonth,
                        ExpYear = cardExpirationYear
                    },
                });
            paymentMethod = paymentMethodService.Attach(
                paymentMethod.Id,
                new PaymentMethodAttachOptions
                {
                    Customer = customer.Id
                });
            var subscriptionSvc = new SubscriptionService();
            var subscription = GetOrCreateSubscription(subscriptionSvc, customer, Configuration.STRIPE_INCOME_CALCULATOR_PRODUCT_PLAN_ID, paymentMethod.Id);
            return subscription;
        }

        private Subscription GetOrCreateSubscription(
            SubscriptionService subscriptionService,
            Customer customer,
            string plan,
            string paymentMethodId)
        {
            var subscription = subscriptionService.List(
                new SubscriptionListOptions
                {
                    Customer = customer.Id,
                    Plan = plan
                }).FirstOrDefault();
            if (subscription != default(Subscription))
            {
                return subscription;
            }
            subscription = subscriptionService.Create(new SubscriptionCreateOptions
            {
                Customer = customer.Id,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Plan = plan,
                        Quantity = 1
                    }
                },
                DefaultPaymentMethod = paymentMethodId
            });
            return subscription;
        }

        private Customer GetOrCreateCustomer(CustomerService customerService, string email)
        {
            var customer = customerService.List(new CustomerListOptions
            {
                Email = email
            }).FirstOrDefault();

            if (customer != default(Customer))
            {
                return customer;
            }

            customer = customerService.Create(new CustomerCreateOptions
            {
                Email = email
            });

            return customer;
        }
    }
}
