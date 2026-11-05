namespace FoodExpress.Core.Models.Identity
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }

    public static class Roles
    {
        public const int CustomerId = 1;
        public const int AdminId = 2;
        public const string Customer = "Customer";
        public const string Admin = "Admin";
    }
}