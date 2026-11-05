using System.Security.Claims;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.Identity;

namespace FoodExpress.Middleware
{
    public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
    {
        public int UserId
        {
            get
            {
                return int.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
            }
        }
        public bool IsAdmin
        {
            get
            {
                return accessor.HttpContext?.User.IsInRole(Roles.Admin) == true;
            }
        }
    }
}
