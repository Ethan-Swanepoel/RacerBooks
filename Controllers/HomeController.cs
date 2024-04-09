using EBBooks.Models;
using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static Google.Rpc.Context.AttributeContext.Types;

namespace RacerBooks.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        FirebaseAuthProvider auth;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            auth = new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("FirebaseEBBooks")));
        }

        public IActionResult Index()
        {
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
    }
}
