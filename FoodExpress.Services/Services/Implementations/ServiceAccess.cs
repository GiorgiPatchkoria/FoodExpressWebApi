using FoodExpress.Core.Interfaces;

namespace FoodExpress.Services.Services.Implementations
{
    public static class ServiceAccess
    {
        public static int RequireUser(ICurrentUser current)
        {
            if (current.UserId <= 0)
            {
                throw new ServiceException(ServiceError.Unauthorized, "Authentication is required.");
            }

            return current.UserId;
        }

        public static void RequireAdmin(ICurrentUser current)
        {
            RequireUser(current);
            if (!current.IsAdmin)
            {
                throw new ServiceException(ServiceError.Forbidden, "Admin access is required.");
            }
        }

        public static void RequireOwner(ICurrentUser current, int userId)
        {
            if (RequireUser(current) != userId)
            {
                throw new ServiceException(ServiceError.NotFound, "Resource not found.");
            }
        }
    }
}