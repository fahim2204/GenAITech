using GenAITech.Data;
using GenAITech.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GenAITech.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _logger = logger;
            _signInManager = signInManager;
        }


        public async Task<IActionResult> Index()
        {
            if (_context.GenAISites != null)
            {
                // Sort the data by SomeProperty and then convert it to a list
                var sortedData = await _context.GenAISites.OrderByDescending(item => item.Like).ToListAsync();
                return View(sortedData);
            }
            else
            {
                return Problem("Entity set 'ApplicationDbContext.GenAISites' is null.");
            }
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult GenAI()
        {
            return View();
        }
        public IActionResult GenAISites()
        {
            return View();
        }
        public IActionResult Jobs()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            Console.WriteLine("hello");
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            // Sign out the user
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}