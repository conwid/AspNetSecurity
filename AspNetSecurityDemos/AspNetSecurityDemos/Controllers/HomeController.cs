using AspNetSecurityDemos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AspNetSecurityDemos.Controllers
{
    public class HomeController : Controller
    {
        private readonly IdentityDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public HomeController(IdentityDbContext context, 
                              UserManager<IdentityUser> userManager, 
                              SignInManager<IdentityUser> signInManager)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }


        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);

            if (!result.Succeeded)
                throw new Exception("Errr");

            return View("Index");
        }

        public async Task<IActionResult> Init()
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            await userManager.CreateAsync(new() { Email = "test@test.hu", UserName = "test@test.hu", EmailConfirmed = true }, "a");
            await userManager.CreateAsync(new() { Email = "test2@test.hu", UserName = "test2@test.hu", EmailConfirmed = true }, "a");
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}