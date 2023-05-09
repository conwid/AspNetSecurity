using AspNetSecurityDemos.Demos;
using AspNetSecurityDemos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace AspNetSecurityDemos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly TicketDatabase ticketDatabase;

        public HomeController(ApplicationDbContext context,
                              UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              TicketDatabase ticketDatabase)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.ticketDatabase = ticketDatabase ?? throw new ArgumentNullException(nameof(ticketDatabase));
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
            await userManager.CreateAsync(new ApplicationUser
            {
                Email = "test@test.hu",
                UserName = "test@test.hu",
                EmailConfirmed = true,
                DateOfBirth = new DateTime(1980, 1, 1),
                Address = "Some sensitive address information"
            }, "a");
            await userManager.CreateAsync(new ApplicationUser
            {
                Email = "test2@test.hu",
                UserName = "test2@test.hu",
                EmailConfirmed = true,
                DateOfBirth = new DateTime(2020, 1, 1),
                Address = "Some other sensitive address information"
            }, "a");
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [MinimumAge(18)]
        public IActionResult DrinkBeer()
        {
            return View("Index");
        }

        [Authorize]
        public async Task<IActionResult> Logins()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(ticketDatabase.ticketStore.ToList());
        }

        [Authorize]
        public async Task<IActionResult> DeleteLogin(Guid loginId)
        {
            var ticket = ticketDatabase.ticketStore.SingleOrDefault(t => t.Id == loginId);
            if (ticket != null)
            {
                ticketDatabase.ticketStore.Remove(ticket);
            }
            return RedirectToAction(nameof(Logins));
        }
    }
}