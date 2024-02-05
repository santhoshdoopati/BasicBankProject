Feature: Basic Bank Tests

A short summary of the feature

@TC1 @Account
Scenario: User can create account with valid data
    Given minimum deposit is 100000
    And account name is "Santhosh"
    And City is "Hyderabad"
    And Account Type is "Current"
    When Account creation endpoint is triggered with above details
    Then Verify response code is 200
    And Verify Success message is "Account created successfully"
    And Verify Account details are returned
@TC2 @Account
 Scenario: User can delete existing account
    Given User has existing account with account number 919191
    And User account status is Normal
    And User signature matching with Delete Account Terms
    When  POST delete account endpoint is trigeered
    Then Verify response code is "200"
    And Verify Success message is "Account deleted successfully"
    And Verify Deleted Account details
@TC3 @Account
Scenario: User can depoiste into account
    Given User has existing account with account number 919191
    And User name "Santhosh" matched with account's registered name
    And IFSC code "BANK0098" matches with account's IFSC code
    And User deposit ammount is "200000"
    When POST deposit amount end point is trigeered
    Then Verify response code is 200
    And Verify Account balance is updated
@TC4 @Account
Scenario:  User can withdraw from account
    Given User has existing account with account number 919191
    And Entered PIN "1212" matches with account's PIN
    And User enters withdrawl amount as "50000"
    And Account balance is greater than withdrawl amount
    When POST withdraw amount end point is trigeered
    Then Verify response code is 200
    And Verify Account balance is updated after withdrawl


