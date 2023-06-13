using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Thesis.Data;
using Thesis.Helpers;
using Thesis.Models;
using Thesis.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid.Helpers.Mail;
using SendGrid;
using Thesis.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Identity;

namespace Thesis.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly PasswordHasher<Users> _passwordHasher;


        public RegistrationController(ApplicationDbContext dbContext, IEmailSender emailSender)
        {
            _db = dbContext;
            _emailSender = emailSender;
            _passwordHasher = new PasswordHasher<Users>();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("Index");
        }

        public async Task<IActionResult> Register(RegistrationViewModel user)
        {
            if (ModelState.IsValid)
            {
                var email = user.Email;
                var password = user.Password;
                var name = user.Name;
                var type = "user";

                var existingUser = _db.Users.FirstOrDefault(u => u.Email == email);

                if (existingUser == null)
                {
                    var hashedPassword = _passwordHasher.HashPassword(null, password);

                    var newUser = new Users
                    {
                        Email = email,
                        Password = hashedPassword,
                        Name = name,
                        Type = type,
                        IsVerified = false // Set IsVerified to false initially
                    };

                    _db.Users.Add(newUser);
                    await _db.SaveChangesAsync();

                    var request = HttpContext.Request;

                    // Compose the verification email message
                    var verificationLink = Url.Action("VerifyEmail", "Registration", new { email }, request.Scheme, request.Host.Value);
                    var message = $"Здравейте, {name},<br><br>Благодаря Ви, че се регистрирахте в нашия сайт!<br><br>Моля, натиснете следния линк, за да валидирате имейл адреса си:<br><br><a href='{verificationLink}'>{verificationLink}</a><br><br>Поздрави,<br>Хайро";
                    await _emailSender.SendEmailAsync(email, "Валидация на имейл", message);

                    // Display a message to check email for verification link
                    return View("CheckEmail");
                }
                else
                {
                    ModelState.AddModelError("Email", "Този имейл е регистриран.");
                }
            }

            return View("Index", user);
        }

        public async Task<IActionResult> VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                // Invalid email
                return RedirectToAction("EmailVerificationError", "Registration");
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || user.IsVerified)
            {
                // User not found or already verified
                return RedirectToAction("EmailVerificationError", "Registration");
            }

            // Verify the user's email by setting IsVerified to true
            user.IsVerified = true;
            await _db.SaveChangesAsync();

            return View("EmailVerificationSuccess");
        }

    }
}
