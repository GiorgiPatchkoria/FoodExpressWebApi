namespace FoodExpress.Core.Interfaces
{
    public interface ICurrentUser
    {
        int UserId { get; }
        bool IsAdmin { get; }
    }
}