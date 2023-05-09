using AspNetSecurityDemos.Demos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=IdentityDemo;Integrated Security=true"));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts => opts.Stores.ProtectPersonalData = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<CustomClaimFactory>()
                .AddPersonalDataProtection<CustomLookupProtector, CustomKeyRing>();

//builder.Services.AddScoped<IPersonalDataProtector, MyPersonalDataProtector>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, MinimumAgePolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 1;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TicketDatabase>(); // can be scoped if it's an actual db connection
builder.Services.AddSingleton<ITicketStore, CustomTicketStore>(); // must be singleton, because it is resolved for the authorization middleware

builder.Services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
                .Configure<ITicketStore>((options, store) => options.SessionStore = store);

var app = builder.Build();


app.UseHttpsRedirection().UseStaticFiles().UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
