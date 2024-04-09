using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using EBBooks.Services;
using Newtonsoft.Json;
using Firebase.Auth;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using static Google.Rpc.Context.AttributeContext.Types;
using RacerBooks.Interfaces;
using RacerBooks.Models;

namespace RacerBooks.Controllers
{
    public class LoginController : Controller
    {
        RacerbooksContext _context = new RacerbooksContext();
        private readonly IUnsuccessfulLoginLogger _logger;
        FirebaseAuthProvider auth;

        public LoginController(RacerbooksContext context, IUnsuccessfulLoginLogger logger)
        {
            _context = context;
            _logger = logger;
            auth = new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("FirebaseEBBooks")));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            try
            {
                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                string currentUserId = fbAuthLink.User.LocalId;

                if (currentUserId != null)
                {
                    HttpContext.Session.SetString("LoggedInUser", currentUserId);
                    TempData["EmailAsTempData"] = login.Email;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Log the unsuccessful login attempt
                    await _logger.LogUnsuccessfulLoginAttemptAsync(login.Email, "Incorrect credentials.");
                    ViewBag.Error = "Incorrect credentials, please register a user";
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
        public IActionResult Logout()
        {
            //Logout and clears the session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

    }
}