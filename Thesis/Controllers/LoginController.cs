using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Thesis.Data;
using Thesis.ViewModels;

namespace Thesis.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<Users> _passwordHasher;
        private readonly IEmailSender _emailSender;

        public LoginController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _passwordHasher = new PasswordHasher<Users>();
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync();

                Response.Cookies.Delete(".AspNetCore.Cookies");
                HttpContext.Session.Clear(); // delete all previous sessions

                return RedirectToAction("Index", "Login");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);

            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Грешно име или парола.");
                return View("Index", model);
            }

            if (user.IsVerified == false)
            {
                ModelState.AddModelError(string.Empty, "Моля, валидирайте имейл адреса.");
                return View("Index", model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Грешно име или парола.");
                return View("Index", model);
            }

            HttpContext.Session.SetString("IsLoggedIn", "true");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Type)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Response.Cookies.Delete(".AspNetCore.Cookies");

            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PassReset(string email)
        {
            var request = HttpContext.Request;

            var verificationLink = Url.Action("NewPass", "Login", new { email }, request.Scheme, request.Host.Value);
            var message = $"Натиснете следния линк, за да рестартирате паролата си:<br><br><a href='{verificationLink}'>{verificationLink}</a><br><br>Поздрави,<br>Хайро";
            await _emailSender.SendEmailAsync(email, "Нова парола", message); // pass reset mail with link

            return View("CheckEmailPass");
        }

        public async Task<IActionResult> PassResetVerification(string email, string pass, string confpass)
        {
            if (email == null)
            {
                throw new Exception("Invalid token or token has expired.");
            }

            if (string.IsNullOrEmpty(pass))
            {
                ModelState.Remove("pass");
                ModelState.Remove("confpass");
                ModelState.AddModelError("pass", "Моля, въведете парола.");
                ViewData["Email"] = email;
                return View("NewPass");
            }

            if (string.IsNullOrEmpty(confpass))
            {
                ModelState.Remove("confpass");
                ModelState.AddModelError("pass", "Моля, повторете паролата.");
                ViewData["Email"] = email;
                return View("NewPass");
            }

            if (pass != confpass)
            {
                ModelState.Remove("pass");
                ModelState.Remove("confpass");
                ModelState.AddModelError("pass", "Паролите не са еднакви.");
                ViewData["Email"] = email;
                return View("NewPass");
            }

            if (pass.Length < 6)
            {
                ModelState.Remove("pass");
                ModelState.Remove("confpass");
                ModelState.AddModelError("pass", "Моля, напишете по-дълга парола.");
                ViewData["Email"] = email;
                return View("NewPass");
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == email);

            var hashedPassword = _passwordHasher.HashPassword(null, pass); // hash the password before saving

            user.Password = hashedPassword;

            await _db.SaveChangesAsync();

            return View("PassResetSuccess");
        }

        public ActionResult ResetView()
        {
            return View();
        }

        public ActionResult NewPass(string email)
        {
            if (email == null)
            {
                throw new Exception("Invalid token or token has expired.");
            }

            ViewBag.Email = email; // store the email for reset pass

            return View();
        }
    }
}
