using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using MiniWebApplication.ViewModels;
using System.Globalization;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.EntityFrameworkCore;
using MiniWebApplication.Helpers;


namespace MiniWebApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly RazorViewToStringRenderer _razorViewToStringRenderer;

        public AccountController(ApplicationDbContext context, IConfiguration configuration, RazorViewToStringRenderer razorViewToStringRenderer)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
            _razorViewToStringRenderer = razorViewToStringRenderer;
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                return View(model);
            }

            // Remove non-numeric characters from phone number
            var phoneDigits = new string(model.PhoneNumber.Where(char.IsDigit).ToArray());

            if (phoneDigits.Length != 10)
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must be exactly 10 digits.");
                return View(model);
            }

            // Try to parse the Date of Birth
            if (!DateTime.TryParseExact(model.DateOfBirth, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateOfBirth))
            {
                ModelState.AddModelError("DateOfBirth", "Invalid date format. Please use mm/dd/yyyy.");
                return View(model);
            }

            var emailToken = GenerateEmailConfirmationToken();

            // Map the ViewModel to the User model
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = phoneDigits,
                DateOfBirth = parsedDateOfBirth,
                EmailConfirmed = false,
                EmailConfirmationToken = emailToken,
                EmailConfirmationTokenExpires = DateTime.UtcNow.AddHours(24)
            };

            // Hash the password and store it
            user.Password = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            try
            {
                // Send confirmation email
                var apiKey = _configuration["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);

                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { token = emailToken, email = user.Email }, Request.Scheme);

                var from = new EmailAddress("xingalan1992@gmail.com", "MSSA Web Application");
                var to = new EmailAddress(user.Email, $"{user.FirstName} {user.LastName}");
                var subject = "Please confirm your email";

                // Prepare the email model
                var emailModel = new EmailViewModel
                {
                    FirstName = user.FirstName,
                    ConfirmationLink = confirmationLink
                };

                // Render the email templates
                var plainTextContent = await _razorViewToStringRenderer.RenderViewToStringAsync("~/Views/Shared/ConfirmEmailTxt.cshtml", emailModel);
                var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync("~/Views/Shared/ConfirmEmailHtml.cshtml", emailModel);


                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                var response = await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while sending email:");
                Console.WriteLine(ex.ToString());

                ModelState.AddModelError(string.Empty, "An error occurred while sending the confirmation email. Please try again later.");
                return View(model);
            }

            // Show a view that tells the user to check their email for confirmation
            return RedirectToAction("RegistrationSuccess");
        }

        public async Task<IActionResult> TestEmailTemplate()
        {
            var emailModel = new EmailViewModel
            {
                FirstName = "TestUser",
                ConfirmationLink = "https://example.com/confirm"
            };

            try
            {
                var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync("~/Views/Shared/ConfirmEmailHtml.cshtml", emailModel);
                return Content(htmlContent, "text/html");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                // Invalid token or email
                return View("Error"); // Create an Error view to display an error message
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.EmailConfirmationToken == token);

            if (user == null)
            {
                // User not found or token invalid
                return View("Error"); // Display an error message
            }

            if (user.EmailConfirmationTokenExpires < DateTime.UtcNow)
            {
                // Token expired
                return View("Error"); // Display an error message
            }

            // Confirm the email
            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null; // Clear the token
            user.EmailConfirmationTokenExpires = DateTime.MinValue; // Reset the expiration

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Display a confirmation view
            return View("ConfirmEmailSuccess"); // Create a ConfirmEmailSuccess view to display a success message
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Retrieve the user based on the entered username
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError("Username", "This username does not exist.");
                return View(model);
            }

            // Check if the user's email is confirmed
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "You need to confirm your email before you can log in.");
                return View(model);
            }


            // Verify the hashed password
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result == PasswordVerificationResult.Success)
            {
                // Create the list of claims, including the NameIdentifier claim
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Include the NameIdentifier claim
        };

                // Create the identity and principal
                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                // Sign in the user
                await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

                // Redirect to the Home page after successful login
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("Password", "Wrong password.");
                return View(model);
            }
        }

        // GET: Account/RegistrationSuccess
        public IActionResult RegistrationSuccess()
        {
            return View();
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // Helper method to generate a unique email confirmation token
        private string GenerateEmailConfirmationToken()
        {
            return Guid.NewGuid().ToString(); // Simple token generation; consider using more secure methods
        }
    }
}
