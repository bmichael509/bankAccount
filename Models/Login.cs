using System;
using System.ComponentModel.DataAnnotations;
namespace BankAccount.Models
{
    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}