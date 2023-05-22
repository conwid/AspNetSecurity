using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AspNetSecurityDemos.Demos;

public class CustomClaimFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    public CustomClaimFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var claims = await base.GenerateClaimsAsync(user);
        claims.AddClaim(new Claim("http://schemas.techeddemo.com/2023/05/identity/dateofbirth", user.DateOfBirth.ToShortDateString()));

        return claims;
    }
}

public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public MinimumAgeRequirement(int minimumAge)
    {
        MinimumAge = minimumAge;
    }

    public int MinimumAge { get; }
}

public class MinimumAgeHandler : IAuthorizationHandler
{
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var item in context.PendingRequirements)
        {
            switch (item)
            {
                case MinimumAgeRequirement r:
                    await HandleMinimumAgeRequirement(context, r);
                    break;
            };
        }
    }

    protected async Task HandleMinimumAgeRequirement(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
    {
        var ageClaim = context.User.Claims.SingleOrDefault(c => c.Type == "http://schemas.techeddemo.com/2023/05/identity/dateofbirth");
        if (ageClaim!=null && DateTime.Now.Subtract(DateTime.Parse(ageClaim.Value)) > TimeSpan.FromDays(requirement.MinimumAge * 365))
            context.Succeed(requirement);
    }
}

public sealed class MinimumAgeAttribute : AuthorizeAttribute
{
    private static readonly string policyPrefix = "minimumAge:";
    public MinimumAgeAttribute(int minimumAge)
    {
        MinimumAge = minimumAge;
    }
    public int MinimumAge
    {
        get => int.Parse(Policy.Substring(policyPrefix.Length));
        set => Policy = $"{policyPrefix}{value}";
    }
}

public class MinimumAgePolicyProvider : DefaultAuthorizationPolicyProvider
{
    private static readonly string policyPrefix = "minimumAge:";
    public MinimumAgePolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }
    public async override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(policyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var minimumAge = int.Parse(policyName.Substring(policyPrefix.Length));
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new MinimumAgeRequirement(minimumAge));
            return policy.Build();
        }
        return await base.GetPolicyAsync(policyName);
    }
}