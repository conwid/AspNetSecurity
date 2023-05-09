using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace AspNetSecurityDemos.Demos;

public class AuthenticationEvent
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTimeOffset? LastActivity { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public byte[] Value { get; set; }
}


public class TicketDatabase
{
    public List<AuthenticationEvent> ticketStore = new List<AuthenticationEvent>();
}

public class CustomTicketStore : ITicketStore
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CustomTicketStore(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task RemoveAsync(string key)
    {
        var ticketDatabase = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<TicketDatabase>();
        if (Guid.TryParse(key, out var id))
        {
            var ticket = ticketDatabase.ticketStore.SingleOrDefault(x => x.Id == id);
            if (ticket != null)
            {
                ticketDatabase.ticketStore.Remove(ticket);
            }
        }
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var ticketDatabase = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<TicketDatabase>();
        if (Guid.TryParse(key, out var id))
        {
            var storedTicket = ticketDatabase.ticketStore.SingleOrDefault(t => t.Id == id);
            if (storedTicket != null)
            {
                storedTicket.LastActivity = DateTimeOffset.UtcNow;
                storedTicket.Expires = ticket.Properties.ExpiresUtc;
            }
        }
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var ticketDatabase = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<TicketDatabase>();
        if (Guid.TryParse(key, out var id))
        {
            var ticket = ticketDatabase.ticketStore.SingleOrDefault(t => t.Id == id);
            if (ticket != null)
            {
                ticket.LastActivity = DateTimeOffset.UtcNow;
                return TicketSerializer.Default.Deserialize(ticket.Value);
            }
        }
        return null;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var ticketDatabase = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<TicketDatabase>();
        var userId = ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var authTicket = new AuthenticationEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LastActivity = DateTimeOffset.UtcNow,
            Value = TicketSerializer.Default.Serialize(ticket)
        };
        var expires = ticket.Properties.ExpiresUtc;
        if (expires.HasValue)
            authTicket.Expires = expires.Value;

        ticketDatabase.ticketStore.Add(authTicket);
        return authTicket.Id.ToString();
    }
}
