using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using System.Diagnostics;
using System.Security.Claims;
using Thesis.Data;
using Thesis.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Thesis.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
       public HomeController(ApplicationDbContext db, ILogger<HomeController> logger, IEmailSender emailSender)
        {
            _db = db;
            _logger = logger;
            _emailSender = emailSender;
        }

        public IActionResult Index(string returnUrl = null)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            ViewBag.Message = $"Здравейте, {userName}!";
            var diplom = _db.Diplom.ToList();

            ViewBag.Diplom = diplom;
            ViewBag.ReturnUrl = returnUrl; // Pass the returnUrl to the view

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult CheckWord(string word)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            ViewBag.Message = $"Здравейте, {userName}!";
            // query the database to check if the word exists
            var wordRecord = _db.Diplom.FirstOrDefault(w => w.Word == word.ToLower());

            List<string> imagePaths = new List<string>();

            bool notFound = false;

            if (word != null)
            {
                // set the image path in the ViewBag variable
                if (wordRecord == null)
                {
                    string[] tokens = word.ToLower().Split(new[] { ',', ' ', '.', '!', '?', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var token in tokens)
                    {
                        var record = _db.Diplom.FirstOrDefault(w => w.Word == token);

                        if (record == null || record.Image == null)
                        {
                            ViewBag.WordNotFound = "Изразът не е намерен!";
                            notFound = true;
                            break;
                        }
                    }

                    if (!notFound) 
                    {
                        

                        foreach (var token in tokens)
                        {
                            var record = _db.Diplom.FirstOrDefault(w => w.Word == token);

                            imagePaths.Add(record.Image);
                        }
                    }

                    ViewBag.ImagePath = imagePaths;
                }

                else
                {
                    imagePaths.Add(wordRecord.Image);
                    ViewBag.ImagePath = imagePaths;
                }
                
            }
            else
            {
                // set the error message in the ViewBag variable
                ViewBag.WordNotFound = "Изразът не е намерен!";
            }
           
            return View("Index");
        }

        public async Task<IActionResult> UploadImage(IFormFile imageFile, string imageName)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Моля, изберете снимка, която да качите.");
            }

            // Save the image file to the wwwroot/img folder
            var fileName = Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Save the image name to the database
            var imagePath = $"/img/{fileName}";
            _db.Diplom.Add(new Diplom { Image = fileName, Word = imageName }) ;
            _db.SaveChanges();

            return RedirectToAction("Dictionary");
        }

        public IActionResult Details(int id)
        {
            var image = _db.Diplom.FirstOrDefault(i => i.Id == id);
            var imagePath = Path.Combine("/img", image.Image);
            var imageName = Path.Combine(image.Word);
            ViewData["ImagePath"] = imagePath;
            ViewData["ImageName"] = imageName;
            return View(image);
        }

        public IActionResult Delete(int id)
        {
            var image = _db.Diplom.FirstOrDefault(i => i.Id == id);

            _db.Diplom.Remove(image);
            _db.SaveChanges();

            return RedirectToAction("Dictionary", "Home");
        }

        public IActionResult Dictionary()
        {
            var diplom = _db.Diplom.ToList();

            // Add Diplom to the ViewBag
            ViewBag.Diplom = diplom;
            return View("~/Views/Admin/Dictionary.cshtml");
        }

        public async Task<IActionResult> SendQuestion(string message)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            ViewBag.Message = $"Здравейте, {userName}!";

            if (string.IsNullOrEmpty(message) || message.Length < 20)
            {
                ModelState.AddModelError("message", "Съобщението трябва да е минимум 20 символа!");
                return View("Index"); // Replace with the appropriate view name
            }
            var mail = "hairosamaz@abv.bg";
            var subject = "Ново съобщение!";
            await _emailSender.SendEmailAsync(mail, subject, message);

            var senderMail = User.FindFirstValue(ClaimTypes.Email);
            var senderSumber = "Съобщението е изпратено!";
            var senderMessage = "Благодаря Ви за съобщението! То е получено и ще бъде анализирано от екипа ни!";
            await _emailSender.SendEmailAsync(senderMail, senderSumber, senderMessage);

            ViewData["SuccessMessage"] = "Съобщението е изпратено!";

            return View("Index");
        }

        public IActionResult UserDatabase()
        {
            var userDB = _db.Users.ToList();

            // Add Diplom to the ViewBag
            ViewBag.Users = userDB;
            return View("~/Views/Admin/UserDatabase.cshtml");
        }

        public IActionResult DeleteUser(int id)
        {
            var user = _db.Users.FirstOrDefault(i => i.UserId == id);

            _db.Users.Remove(user);
            _db.SaveChanges();

            return RedirectToAction("UserDatabase", "Home");
        }
    }
}