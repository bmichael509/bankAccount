using System.Collections.Generic;

namespace BankAccount.Models
{
    public class AccountUserWrapper
    {
        public User LoggedInUser { get; set; }
        public List<Transaction> ListOfTransactions { get; set; }
        public Transaction TransactionForm { get; set; }
    }
}