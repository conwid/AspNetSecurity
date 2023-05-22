using Microsoft.AspNetCore.Antiforgery;

namespace AspNetSecurityDemos.Demos;

public class Item
{
    public int Data { get; set; }
}

public class ExpiringAntiforgeryAddtionalDataProvider : IAntiforgeryAdditionalDataProvider
{
    public string GetAdditionalData(HttpContext context)
    {
        return DateTime.UtcNow.AddMinutes(5).ToString();
    }

    public bool ValidateAdditionalData(HttpContext context, string additionalData)
    {
        var isDate = DateTime.TryParse(additionalData, out var expirationDate);
        return isDate && DateTime.UtcNow < expirationDate;
    }
}