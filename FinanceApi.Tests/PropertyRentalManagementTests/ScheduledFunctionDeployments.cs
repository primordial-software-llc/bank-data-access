using System;
using System.Collections.Generic;
using Amazon;
using Amazon.Lambda;
using AwsLambdaDeploy;
using FinanceApi.Tests.InfrastructureAsCode;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.PropertyRentalManagementTests
{
    public class ScheduledFunctionDeployments
    {
        private ITestOutputHelper Output { get; }

        public ScheduledFunctionDeployments(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void DeployCreateWeeklyInvoices()
        {
            var environmentVariables = new Dictionary<string, string>();
            var scheduleExpression = "cron(0 0 ? * 1 *)";
            new LambdaDeploy().Deploy(
                Factory.CreateCredentialsFromProfile(),
                new List<RegionEndpoint> { RegionEndpoint.USEast1 },
                environmentVariables,
                scheduleExpression,
                "lakeland-mi-pueblo-create-weekly-invoices",
                @"C:\Users\peon\Desktop\projects\bank-data-access\PropertyRentalManagement.CreateWeeklyInvoices\PropertyRentalManagement.CreateWeeklyInvoices.csproj",
                new LambdaEntrypointDefinition
                {
                    AssemblyName = "PropertyRentalManagement.CreateWeeklyInvoices",
                    Namespace = "PropertyRentalManagement.CreateWeeklyInvoices",
                    ClassName = "Function",
                    FunctionName = "FunctionHandler"
                },
                roleArn: "arn:aws:iam::283733643774:role/lambda_exec_finance_api",
                runtime: Runtime.Dotnetcore31,
                256,
                1,
                TimeSpan.FromMinutes(5));
        }

        [Fact]
        public void DeployCreateMonthlyInvoices()
        {
            var environmentVariables = new Dictionary<string, string>();
            var scheduleExpression = $"cron(0 0 1 * ? *)";
            new LambdaDeploy().Deploy(
                Factory.CreateCredentialsFromProfile(),
                new List<RegionEndpoint> { RegionEndpoint.USEast1 },
                environmentVariables,
                scheduleExpression,
                "lakeland-mi-pueblo-create-monthly-invoices",
                @"C:\Users\peon\Desktop\projects\bank-data-access\PropertyRentalManagement.CreateMonthlyInvoices\PropertyRentalManagement.CreateMonthlyInvoices.csproj",
                new LambdaEntrypointDefinition
                {
                    AssemblyName = "PropertyRentalManagement.CreateMonthlyInvoices",
                    Namespace = "PropertyRentalManagement.CreateMonthlyInvoices",
                    ClassName = "Function",
                    FunctionName = "FunctionHandler"
                },
                roleArn: "arn:aws:iam::283733643774:role/lambda_exec_finance_api",
                runtime: Runtime.Dotnetcore31,
                256,
                1,
                TimeSpan.FromMinutes(5));
        }
    }
}
