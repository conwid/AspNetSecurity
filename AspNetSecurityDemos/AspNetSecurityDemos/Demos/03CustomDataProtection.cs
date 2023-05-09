using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace AspNetSecurityDemos.Demos;

public class CustomLookupProtector : ILookupProtector
{
  
    [return: NotNullIfNotNull("data")]
    public string? Protect(string keyId, string? data)
    {
        return data == null ? null : new string(data.Reverse().ToArray());
    }
    [return: NotNullIfNotNull("data")]
    public string? Unprotect(string keyId, string? data)
    {
        return data == null ? null : new string(data.Reverse().ToArray());
    }
}

public class CustomPersonalDataProtector : DefaultPersonalDataProtector
{
    public CustomPersonalDataProtector(ILookupProtectorKeyRing keyRing, ILookupProtector protector) : base(keyRing, protector)
    {
    }

    public override string? Protect(string? data)
    {
        var x = base.Protect(data);
        return x;
    }

    public override string? Unprotect(string? data)
    {
        var x = base.Unprotect(data);
        return x;
    }
}

public class CustomKeyRing : ILookupProtectorKeyRing
{

    private string currentKeyId = "1";
    public string this[string keyId]
    {
        get
        {
            if (keyId == "1")
                return "A";
            return "B";
        }
    }

    public string CurrentKeyId
    {
        get
        {
            return currentKeyId;
        }
    }

    public IEnumerable<string> GetAllKeyIds()
    {
        return new List<string> { "1", "2" };
    }
}