using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace AspNetSecurityDemos.Demos;

public class CustomAuthenticationEvents : CookieAuthenticationEvents
{
    public override async Task SigningIn(CookieSigningInContext context)
    {
       await base.SigningIn(context);
    }

    public async override Task SigningOut(CookieSigningOutContext context)
    {
       await base.SigningOut(context);
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        //context.RejectPrincipal();
        //context.ReplacePrincipal();
        await base.ValidatePrincipal(context);
    }
}