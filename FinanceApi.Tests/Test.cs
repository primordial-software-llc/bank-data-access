﻿using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public void Refreshes_On_First_Attempt()
        {
            var userBankAccount = new FinanceUserBankAccount();
            if (!DateTime.TryParseExact(userBankAccount.Updated, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updated))
            {
                updated = DateTime.UtcNow.AddYears(-1);
            }
            var refreshPoint = DateTime.UtcNow.AddHours(-1);
            if (refreshPoint <= updated)
            {
                Assert.True(false);
            }
            else
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void Refreshes_When_Stale()
        {
            var userBankAccount = new FinanceUserBankAccount
            {
                Updated = DateTime.UtcNow.AddYears(-1).ToString("O")
            };
            if (!DateTime.TryParseExact(userBankAccount.Updated, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updated))
            {
                updated = DateTime.UtcNow;
            }
            var refreshPoint = DateTime.UtcNow.AddHours(-1);
            if (refreshPoint <= updated)
            {
                Assert.True(false);
            }
            else
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void Uses_Cache_When_Not_Stale()
        {
            var userBankAccount = new FinanceUserBankAccount
            {
                Updated = DateTime.UtcNow.AddYears(1).ToString("O")
            };
            if (!DateTime.TryParseExact(userBankAccount.Updated, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updated))
            {
                updated = DateTime.UtcNow;
            }
            var refreshPoint = DateTime.UtcNow.AddHours(-1);
            if (refreshPoint <= updated)
            {
                Assert.True(true);
            }
            else
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void EmptyTokenIsNotValid()
        {
            var isValid = AwsCognitoJwtTokenValidator.IsValid(string.Empty, string.Empty, string.Empty);
            output.WriteLine(isValid.ToString());
        }

    }
}
