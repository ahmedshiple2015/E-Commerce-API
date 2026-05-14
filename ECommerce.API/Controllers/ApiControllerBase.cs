using System.Security.Claims;
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
}
