using System.Security.Claims;

namespace eqshopping.Repositories
{
    public interface IClaimsHelper
    {
        string GetUserRole(ClaimsPrincipal user);
        string GetUserId(ClaimsPrincipal user);
    }
    public class ClaimsHelper: IClaimsHelper
    {
        public string GetUserRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        public string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
