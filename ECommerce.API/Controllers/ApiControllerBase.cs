using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected int? CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }

    protected bool IsAdmin => User.IsInRole("Admin");

    protected bool IsSeller => User.IsInRole("Seller");

    protected bool CanAccessUser(int userId)
    {
        return IsAdmin || CurrentUserId == userId;
    }

    protected ActionResult OwnershipForbidden()
    {
        return User.Identity?.IsAuthenticated == true ? Forbid() : Unauthorized();
    }

    protected static string CreateGuestAccessToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    protected static bool FixedTimeEquals(string expected, string? actual)
    {
        if (string.IsNullOrWhiteSpace(actual))
        {
            return false;
        }

        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expected);
        var actualBytes = System.Text.Encoding.UTF8.GetBytes(actual);
        return expectedBytes.Length == actualBytes.Length && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
