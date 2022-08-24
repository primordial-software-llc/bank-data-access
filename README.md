# Primordial Software LLC

## Public Features
 - Free weekly and monthly budget builder
 - Paid banking integration for financial assets and credit card expenses
     - Build budget expenses automatically from credit card expenses

## Private Features
This site also hosts a private area to manage weekly and monthly property rentals integrating with QuickBooks Online and Clover
- Automatic weekly and monthly invoices using scheduled AWS Lambda functions for tracking expected and collected income
    - Invoices are configured per active customer
- Automatic monthly sales receipt creation in QuickBooks Online from Clover Point of Sale system for cash and card sales
- Web based Point of Sale system with integration to QuickBooks Online
    - Can be used in a browser on a phone or on a computer
    - Accept credit card payments when recording a sale paying about 1% more in credit card fees, but saving you the cost and convenience of having to carry a clover device
    - Receipts create/update invoices and/or payments in QuickBooks online. Payment application is robust handling many complex scenarios for clean reports in QuickBooks Online.
    - Receipts feature a print view for writing to a thermal printer
    - Receipts create an immutable record in AWS Dynamo DB for validating all QuickBooks Online transactions
- Custom double entry cash basis income report comparing immutable AWS Dynamo DB records against QuickBooks Online records
    - Custom date ranges: daily, weekly, monthly, etc.
    - Provides a summary of expected cash and checks to be desposited
    - Provides a summary of credit and debit card expected to be deposited into merchant account. Credit and debit cards are tracked backed to the original sale to provide a third check in addition to Dynamo DB and QuickBooks Online.
    - Report highlights discrepancies in any of the three systems: AWS Dynamo DB, QuickBooks Online and Clover
- Property map currently supporting about 300 sub-plots of land on a private property
- *All private features are fully open source. A private application is contained here, so I can share the knowledge without spending the time to automatically build out the infrastructure with a new account creation nor does the account creation support organizations only individuals.*

## About This Project

Currently powered by plaid https://plaid.com/
