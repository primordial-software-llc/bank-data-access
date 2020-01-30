using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
{
    public class Test
    {
        private readonly ITestOutputHelper output;

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Stripe_Purchase()
        {
            StripeConfiguration.ApiKey = Configuration.STRIPE_API_SECRET_KEY;
            const string PLAN = "plan_GdPUjMfpSganRW";
            var customerSvc = new CustomerService();
            var customer = GetOrCreateCustomer(customerSvc, "timg456789@yahoo.com");
            var paymentMethodService = new PaymentMethodService();
            var paymentMethod = paymentMethodService.Create(
                new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardCreateOptions
                    {
                        Cvc = "123",
                        Number = "4000000000000077",
                        ExpMonth = 01,
                        ExpYear = 2025
                    },
                });
            paymentMethod = paymentMethodService.Attach(
                paymentMethod.Id,
                new PaymentMethodAttachOptions
                {
                    Customer = customer.Id
                });
            var subscriptionSvc = new SubscriptionService();
            var subscription = GetOrCreateSubscription(subscriptionSvc, customer, PLAN, paymentMethod.Id);
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

        [Fact]
        public void EmptyTokenIsNotValid()
        {
            var isValid = AwsCognitoJwtTokenValidator.IsValid(string.Empty, string.Empty);
            output.WriteLine(isValid.ToString());
        }

        [Fact]
        public void GetInstitution()
        {
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());

            var ins = client.GetInstitution("ins_9")["institution"];
            output.WriteLine(ins.ToString(Formatting.Indented));


            var itemJson = new JArray();
            var institutionsJson = new JArray();
            var institutions = new HashSet<string>();
            var itemRecord = client.GetItem("")["item"];
            institutions.Add(itemRecord["institution_id"].Value<string>());
            itemJson.Add(itemRecord);
            foreach (var institution in institutions)
            {
                institutionsJson.Add(client.GetInstitution(institution)["institution"]);
            }
            foreach (var item in itemJson)
            {
                var institutionId = item["institution_id"].Value<string>();
                item["institution"] = institutionsJson.First(x =>
                    string.Equals(x["institution_id"].Value<string>(), institutionId, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void GetItem()
        {
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());
            var balance = client.GetItem("");
            output.WriteLine(balance.ToString(Formatting.Indented));
        }

        [Fact]
        public void RemoveItem()
        {
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());
            var balance = client.RemoveItem("");
            output.WriteLine(balance.ToString(Formatting.Indented));
        }

    }
}
