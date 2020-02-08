using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;

namespace FinanceApi.Routes.Authenticated
{
    public class PostPurchase : IRoute
    {
        public APIGatewayProxyResponse Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user = null)
        {
            var body = JObject.Parse(request.Body);
            if (body["agreedToBillingTerms"] == null || !body["agreedToBillingTerms"].Value<bool>())
            {
                throw new Exception("Must agree to billing terms");
            }
            var subscription = Purchase(
                user.Email,
                body["cardCvc"].Value<string>(),
                body["cardNumber"].Value<string>(),
                body["cardExpirationMonth"].Value<long>(),
                body["cardExpirationYear"].Value<long>());
            response.Body = JsonConvert.SerializeObject(subscription);
            var jsonPatch = new JObject
            {
                ["billingAgreement"] = JObject.FromObject(new BillingAgreement
                {
                    AgreedToLicense = body["agreedToBillingTerms"].Value<bool>(),
                    Date = DateTime.UtcNow.ToString("O"),
                    IpAddress = request.RequestContext.Identity.SourceIp
                })
            };
            new UserService().UpdateUser(user.Email, jsonPatch, true);
            return response;
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
