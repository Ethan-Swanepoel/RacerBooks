using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using static Google.Rpc.Context.AttributeContext.Types;
using Firebase.Auth;
using Newtonsoft.Json;
using RacerBooks.Models;

namespace RacerBooks.Controllers
{
    public class UsersController : Controller
    {
        private readonly RacerbooksContext _context;
        FirebaseAuthProvider auth;
        public UsersController(RacerbooksContext context)
        {
            _context = context;
            auth = new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("FirebaseEBBooks")));
        }


        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Email == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Register
        [HttpGet]
        public IActionResult RegisterView()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterView(LoginModel login, Models.User user)
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
                user.UserRole = "Customer";

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

        //Extra methods---------------------------------------------------------------------------------

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