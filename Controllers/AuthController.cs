using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using Google.Api;
using RacerBooks.Interfaces;
using RacerBooks.Models;


namespace RacerBooks.Controllers
{
    public class AuthController : Controller
    {

        RacerbooksContext _context = new RacerbooksContext();
        private readonly IUnsuccessfulLoginLogger _logger;
        FirebaseAuthProvider auth;


        public AuthController(RacerbooksContext context, IUnsuccessfulLoginLogger logger)
        {
            _context = context;
            _logger = logger;
            auth = new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("FirebaseEBBooks")));
        }


        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(LoginModel login)
        {
            try
            {
                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                string currentUserId = fbAuthLink.User.LocalId;

                if (currentUserId != null)
                {
                    HttpContext.Session.SetString("LoggedInUser", currentUserId);
                    TempData["EmailAsTempData"] = login.Email;


                    var uid = HttpContext.Session.GetString("LoggedInUser");

                    // Query the database for the user's role
                    var loggedInUser = _context.Users.FirstOrDefault(u => u.FirebaseUuid == uid);
                    if (loggedInUser == null || loggedInUser.UserRole != "Admin")
                    {
                        // If the user does not exist or is not an admin, redirect to an error page or show an error message
                        HttpContext.Session.Clear();
                        return RedirectToAction("NotAdmin", "Auth");
                    }


                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Log the unsuccessful login attempt
                    await _logger.LogUnsuccessfulLoginAttemptAsync(login.Email, "Incorrect credentials.");
                    ViewBag.Error = "Incorrect credentials, please login with your administrator credentials";
                    return View();

                }
            }
            catch (FirebaseAuthException ex)
            {
                // Handle Firebase authentication exceptions
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                ModelState.AddModelError(string.Empty, firebaseEx.error.message);
                return View(login);
            }
        }



        [HttpGet]
        public IActionResult AdminRegister()
        {
            // Check if the user is logged in and their role is "Admin"
            var uid = HttpContext.Session.GetString("LoggedInUser");

            if (string.IsNullOrEmpty(uid))
            {
                return RedirectToAction("AdminLogin", "Auth");
            }

            // Query the database for the user's role
            var loggedInUser = _context.Users.FirstOrDefault(u => u.FirebaseUuid == uid);
            if (loggedInUser == null || loggedInUser.UserRole != "Admin")
            {
                // If the user does not exist or is not an admin, redirect to an error page or show an error message
                return RedirectToAction("NotAdmin", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminRegister(LoginModel login, Models.User user)
        {
            try
            {

                // Validate email format
                if (!login.Email.Contains("@"))
                {
                    ModelState.AddModelError("Email", "Invalid email entered");
                    return View(login);
                }

                // Validate password length
                if (login.Password.Length < 8)
                {
                    ModelState.AddModelError("Password", "Password must be at least 8 characters.");
                    return View(login);
                }

                // Validate password complexity (at least 1 special character and 1 number)
                if (!HasSpecialCharacter(login.Password) || !HasNumber(login.Password))
                {
                    ModelState.AddModelError("Password", "Password must contain at least 1 special character (!@#$%^&*) and 1 number.");
                    return View(login);
                }

                // Check if the email already exists
                if (UserExists(login.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(login);
                }

                await auth.CreateUserWithEmailAndPasswordAsync(login.Email, login.Password);

                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                string currentUserId = fbAuthLink.User.LocalId;

                user.FirebaseUuid = currentUserId;
                user.Email = login.Email;
                user.UserRole = "Admin";

                _context.Add(user);

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");

            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                ModelState.AddModelError(string.Empty, firebaseEx.error.message);
                return View(login);
            }
        }







        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Email == id);
        }

        //Checks if the password contains special characters
        private bool HasSpecialCharacter(string input)
        {
            var specialCharacters = new char[] { '!', '@', '#', '$', '%', '^', '&', '*' };
            return input.Any(c => specialCharacters.Contains(c));
        }

        //Checks if the password contains a number
        private bool HasNumber(string input)
        {
            return input.Any(char.IsDigit);
        }

    }
}