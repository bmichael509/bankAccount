using System;
using System.Linq;
using System.Collections.Generic;
using BankAccount.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;


namespace BankAccount.Controllers     //be sure to use your own project's namespace!
{
    public class HomeController : Controller
    {
        private MyContext _context;

        public HomeController(MyContext context)
        {
            _context = context;
        }

        [HttpGet("")]     //Http Method and the route
        public IActionResult Registration() //When in doubt, use IActionResult
        {
            return View("Registration");//or whatever you want to return
        }
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View("Login");
        }


        [HttpPost("/RegisterNewUser")]
        public IActionResult RegisterNewUser(User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");

                    // You may consider returning to the View at this point
                    return RedirectToAction("Login");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                _context.Add(user);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserId", user.UserId);
                return RedirectToAction("Success", new { id = user.UserId });
            }
            return View("Registration");
        }
        [HttpPost("LoginUser")]
        public IActionResult LoginUser(Login login)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == login.Email);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<Login>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(login, userInDb.Password, login.Password);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("Password", "Invalid Password");
                    return View("Login");
                }
                else
                {
                    HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                    return RedirectToAction("Success", new { id = userInDb.UserId });
                }
            }
            return View("Login");
        }
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet("success/{id}")]
        public IActionResult Success(int id)
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (UserId == null)
            {
                return RedirectToAction("Registration");
            }
            AccountUserWrapper LoggedInThing = new AccountUserWrapper();
            LoggedInThing.LoggedInUser = _context.Users.FirstOrDefault(LoggedInUser => LoggedInUser.UserId == (int)UserId);
            LoggedInThing.ListOfTransactions = _context.Transactions
                .Include(t => t.AccountOwner)
                .Where(t => t.UserId == id)
                .ToList();
            return View("Success", LoggedInThing);
        }

        [HttpPost("success/{id}")]
        public IActionResult CreateTransaction(int id, AccountUserWrapper AmountFromForm)
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (UserId == null)
            {
                return RedirectToAction("Registration");
            }
            if (ModelState.IsValid)
            {
                List<Transaction> AccountTransactions = _context.Transactions
                    .Include(t => t.AccountOwner)
                    .Where(t => t.UserId == id)
                    .ToList();
                double AccountBalance = 0;
                foreach (Transaction thisOne in AccountTransactions)
                {
                    AccountBalance += thisOne.Amount;
                }

                // If you don't have enough money.
                if (AmountFromForm.TransactionForm.Amount < 0 && AmountFromForm.TransactionForm.Amount * -1 > AccountBalance)
                {
                    ModelState.AddModelError("TransactionForm.Amount", "You do not have enough money.");
                    return Success(id);
                }
                AmountFromForm.TransactionForm.UserId = (int)UserId;
                _context.Transactions.Add(AmountFromForm.TransactionForm);
                _context.SaveChanges();

            }
            return RedirectToAction("Success");
        }
    }
}