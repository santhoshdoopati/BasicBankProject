using BasicBankProject.Modal;
using System;
using TechTalk.SpecFlow;
using RestSharp;
using NUnit.Framework;
using System.Configuration;

namespace BasicBankProject.StepDefinitions
{
    [Binding]
    public class BasicBankTestsStepDefinitions : BaseTest
    {
        #region variables

        RestHelpers apiHelper = new RestHelpers();
        RestResponse response = new RestResponse();
        AccountDetails accountDetails = new AccountDetails();
        private int minimumDeposit;
        private int accountNumber;
        private int depositedAmount;
        private int withdrawlAmount;
        private int balanceBeforeDeposit;
        private int balanceBeforeWithdrawl;
        private string accountHolderName = "";
        private string branchCity = "";
        private string accountType = "";

        #endregion

        [Given(@"minimum deposit is (.*)")]
        public void GivenMinimumDepositIs(int depositAmount)
        {
            minimumDeposit = depositAmount;
            Assert.IsTrue(minimumDeposit >= 0, "Minimum Deposit should be greater than or equal to zero");
        }

        [Given(@"account name is ""([^""]*)""")]
        public void GivenAccountNameIs(string name)
        {
            accountHolderName = name;
            Assert.IsTrue(accountHolderName.Length >= 4, "Name must contain at least 4 charactors");
        }

        [Given(@"City is ""([^""]*)""")]
        public void GivenCityIs(string city)
        {
            branchCity = city;
        }

        [Given(@"Account Type is ""([^""]*)""")]
        public void GivenAccountTypeIs(string type)
        {
            accountType = type;
        }

        [When(@"Account creation appUrl is triggered with above details")]
        public void WhenAccountCreationappUrlIsTriggeredWithAboveDetails()
        {
            //Body for create request
            var body = new {name =  accountHolderName, minBalance = minimumDeposit, branch = branchCity, account = accountType};
            //Building request and executing
            response = apiHelper.MakeAPICall(appUrl, createEndpoint, Method.Post, body);
        }

        [Then(@"Verify response code is (.*)")]
        public void ThenVerifyResponseCodeIs(string expectedResponseCode)
        {
            //Verifying Status code with expected status code
            Assert.AreEqual(response.StatusCode, expectedResponseCode);
        }

        [Then(@"Verify Success message is (.*)")]
        public void ThenVerifySuccessMessage(string expectedSuccessMessage)
        {
            AccountDetails accountDetails = apiHelper.DeserializeResponse<AccountDetails>(response);
            //Verifying message
            Assert.AreEqual(accountDetails.message, expectedSuccessMessage);
        }

        [Then(@"Verify Account details are returned")]
        public void ThenVerifyAccountDetailsAreReturned()
        {
            //Verifying Account details are returned
            Assert.IsNotNull(accountDetails.num, "Account number is retuned as null");
            Assert.IsNotNull(accountDetails.accountHolderName, "Account holder name is retuned as null");
            Assert.IsNotNull(accountDetails.bankCode, "IFSC code is retuned as null");
        }

        [Given(@"User has existing account with account number (.*)")]
        public void GivenUserHasExistingAccountWithAccountNumber(int accountNum)
        {
            
            //Getting Account details
            var getAccountDetailsBody = new { acNum = accountNum };
            response = apiHelper.MakeAPICall(appUrl, accountDetailsEndPoint, Method.Get,getAccountDetailsBody, "token");
            //Verifying Account is returned
            if(!response.IsSuccessStatusCode)
            {
                Assert.Fail($"Account with account number {accountNum} does not exist");
            }
        }

        [Given(@"User account status is Normal")]
        public void GivenUserAccountStatusIsNormal()
        {
            //Getting Account details
            var getAccountDetailsBody = new { acNum = accountNumber };
            response = apiHelper.MakeAPICall(appUrl, accountDetailsEndPoint, Method.Get, getAccountDetailsBody, "token");
            accountDetails = apiHelper.DeserializeResponse<AccountDetails>(response);
            //Verifying Account Status
            if(accountDetails.status != "Normal")
            {
                Assert.Fail("Account must be in Normal state to delete the account");
            }
        }

        [Given(@"User signature matching with Delete Account Terms")]
        public void GivenUserSignatureMatchingWithDeleteAccountTerms()
        {
            //Verifying Signature
            var signatureResponse = apiHelper.MakeAPICall(appUrl, "/account/verifySign", Method.Get, "token");
            if (!signatureResponse.IsSuccessStatusCode)
            {
                Assert.Fail("Signature on Delete Terms is matching with Account Holder's signature");
            }
        }

        [When(@"POST delete account appUrl is trigeered")]
        public void WhenPOSTDeleteAccountappUrlIsTrigeered()
        {
            //Delete request body
            var deleteRequestBody = new { acNum = accountNumber };
            //Building request and executing
            response = apiHelper.MakeAPICall(appUrl,deleteEndpoint, Method.Delete, deleteRequestBody);
        }

        [Then(@"Verify Deleted Account details")]
        public void ThenVerifyDeletedAccountDetails()
        {
            var getAccountDetailsBody = new { acNum = accountNumber };
            response = apiHelper.MakeAPICall(appUrl, accountDetailsEndPoint, Method.Get, getAccountDetailsBody, "token");

            //Verying Account is NOT returned after delete
            if (response.IsSuccessful)
            {
                Assert.Fail("Account is not deleted");
            }
        }

        [Given(@"User name ""([^""]*)"" matched with account's registered name")]
        public void GivenUserNameMatchedWithAccountsRegisteredName(string name)
        {
            //Getting Account details
            var getAccountDetailsBody = new { acNum = accountNumber };
            response = apiHelper.MakeAPICall(appUrl, accountDetailsEndPoint, Method.Get, getAccountDetailsBody, "token");
            accountDetails = apiHelper.DeserializeResponse<AccountDetails>(response);

            //Verifying given name and existing name on records
            Assert.AreEqual(accountDetails.accountHolderName, name, $"Expected Name: {name}, Existing Name: {accountDetails.accountHolderName}");
        }

        [Given(@"IFSC code ""([^""]*)"" matches with account's IFSC code")]
        public void GivenIFSCCodeMatchesWithAccountsIFSCCode(string givenIFSC)
        {
            var getAccountDetailsBody = new { acNum = accountNumber };
            response = apiHelper.MakeAPICall(appUrl, accountDetailsEndPoint, Method.Get, getAccountDetailsBody, "token");
            accountDetails = apiHelper.DeserializeResponse<AccountDetails>(response);

            //Verifying given IFSC code matches with Account's IFSC code
            Assert.AreEqual(accountDetails.bankCode, givenIFSC, $"Expected IFSC: {givenIFSC}, Existing IFSC:{accountDetails.bankCode}");

            //Storing current balance
            balanceBeforeDeposit = Convert.ToInt32(accountDetails.balance);
        }

        [Given(@"User deposit ammount is ""([^""]*)""")]
        public void GivenUserDepositAmmountIs(string depositAmount)
        {
            //Making sure deposite amount is greater than zero
            if (Convert.ToInt32(depositAmount) > 0)
            {
                depositedAmount = Convert.ToInt32(depositAmount);
            }
            else { Assert.Fail("Deposit amount must be greater than Zero"); }
        }

        [When(@"POST deposit amount end point is trigeered")]
        public void WhenPOSTDepositAmountappUrlIsTrigeered()
        {
            //Deposit request body
            var depositRequestBody = new { acNum = accountNumber, depAmount = depositedAmount };
            response = apiHelper.MakeAPICall(appUrl, depositEndpoint, Method.Post, depositRequestBody, "token");
        }

        [Then(@"Verify Account balance is updated")]
        public void ThenVerifyAccountBalanceIsUpdated()
        {
            //Getting Account details
            var getAccountDetailsBody = new { acNum = accountNumber };
            response = apiHelper.MakeAPICall(appUrl, accountDetailsEndPoint, Method.Get, getAccountDetailsBody, "token");
            accountDetails = apiHelper.DeserializeResponse<AccountDetails>(response);

            //Verifying Balance is updated after deposit
            int balanceAfterDeposit = Convert.ToInt32(accountDetails.balance);
            Assert.IsTrue(balanceAfterDeposit == balanceBeforeDeposit + depositedAmount, "Balance is not updated with deposited amount");
        }

        [Given(@"Entered PIN ""([^""]*)"" matches with account's PIN")]
        public void GivenEnteredPINMatchesWithAccountsPIN(string enteredPin)
        {
            //Verifying PIN
            var verifyPinbody = new { acNum = accountNumber, pin = enteredPin };
            var verifyPinResponse = apiHelper.MakeAPICall(appUrl, "/services/verifyPin", Method.Get, verifyPinbody);
            Assert.IsTrue(verifyPinResponse.IsSuccessStatusCode, "Entered Pin is Incorrect. Account may get blocked for successive incorrect pin attempts");
        }

        [Given(@"User enters withdrawl amount as ""([^""]*)""")]
        public void GivenUserEntersWithdrawlAmountAs(string desiredAmount)
        {
            withdrawlAmount = Convert.ToInt32(desiredAmount);
        }

        [Given(@"Account balance is greater than withdrawl amount")]
        public void GivenAccountBalanceIsGreaterThanWithdrawlAmount()
        {
            var getAccountDetailsBody = new { acNum = accountNumber };
            response = apiHelper.MakeAPICall(appUrl, accountDetailsEndPoint, Method.Get, getAccountDetailsBody, "token");
            accountDetails = apiHelper.DeserializeResponse<AccountDetails>(response);

            //Verifying whether current balance is greater than requested withdrwl amount
            Assert.IsTrue(Convert.ToInt32(accountDetails.balance) >= withdrawlAmount, $"Balance is not sufficient for withdrawl. Current Balance: {accountDetails.balance}");
            //Storing Balance before the withdrawl
            balanceBeforeWithdrawl = Convert.ToInt32(accountDetails.balance);
        }

        [When(@"POST withdraw amount end point is trigeered")]
        public void WhenPOSTWithdrawAmountappUrlIsTrigeered()
        {
            var withdrawRequestBody = new { acNum = accountNumber, desAmount = withdrawlAmount };
            response = apiHelper.MakeAPICall(appUrl, withdrawEndpoint, Method.Post, withdrawRequestBody, "token");
        }

        [Then(@"Verify Account balance is updated after withdrawl")]
        public void ThenVerifyAccountBalanceIsUpdatedAfterWithdrawl()
        {
            var getAccountDetailsBody = new { acNum = accountNumber };
            response = apiHelper.MakeAPICall(appUrl, accountDetailsEndPoint, Method.Get, getAccountDetailsBody, "token");
            accountDetails = apiHelper.DeserializeResponse<AccountDetails>(response);
            int balanceAfterWithdrawl = Convert.ToInt32(accountDetails.balance);
            //Verifying withdrawn amount is deducted from balance.
            Assert.IsTrue(balanceAfterWithdrawl == balanceBeforeWithdrawl - withdrawlAmount, "Balance is not updated after Withdrawl");
        }
    }
}
