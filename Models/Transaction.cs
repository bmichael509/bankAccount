using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BankAccount.Models
{
    public class Transaction
    {
        [Key] // the below prop is the primary key, [Key] is not needed if named with pattern: ModelNameId
        public int TransactionId { get; set; }
        [Required(ErrorMessage = "is required")]
        [Display(Name = "Amount:")]
        [DataType(DataType.Currency)]
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User AccountOwner { get; set; }
    }
}