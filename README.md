# Account Balance Exercise

To practice Event Sourcing and Message part of Reactive Domain, we are going to go through and implement the account balance problem, working individually using a Test-Driven Development approach, but in a manner suitable for production use – for example in the Trading project.

All of the resources available during normal development can be used – collaboration with others, the internet, examples from the past couple of weeks and so forth.

The use cases for the Account are:

1.	An **account** can be created with an ID and a name of the account holder

2.	An **overdraft limit** can be set per account.

3.	A **daily wire transfer limit** can be set per account.

4.	Cheques can be **deposited** into an account. When a cheque is deposited, the funds are available on the next business day (defined as Monday to Friday, 9am-5pm).

5.	Cash can be **deposited** into an account. When cash is deposited, the funds are available immediately.

6.	Cash can be **withdrawn** from an account. Funds are removed immediately.

7.	A **wire transfer** can cause funds to be withdrawn from an account. Funds are removed immediately. If a wire transfer is attempted which is higher than the daily wire limit, the account should be placed into a **blocked** state.

8.	If a withdrawal would cause the account balance to be negative by more than overdraft limit for the account, the withdrawal should not succeed, and the account should be placed into a **blocked** state.

9.	A deposit made to an account in the blocked state should **unblock** the account when the funds become available for use.

Write tests using xunit.net and the ReactiveDomain testing tools to drive the specifications for this problem – there is no need to build a console user interface.

Check code into Git as if it were being submitted for review to be put back into the project at the stages you feel are appropriate, paying attention to commit messages and the constructs. 
Please refer to:
https://github.com/linedata/trader-sharp-sdk/blob/master/doc/standards/commit-messages.md.
Work in the central repository with your name as a branch name.
https://github.com/linedata/CQRS-Training
