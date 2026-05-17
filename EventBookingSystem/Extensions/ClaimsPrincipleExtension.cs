using System.Security.Claims;

namespace EventBookingSystem.Extensions
{
    public static class ClaimsPrincipleExtension
    {
        public static int GetCurrentUserId(this ClaimsPrincipal principal)
        {
            return int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
