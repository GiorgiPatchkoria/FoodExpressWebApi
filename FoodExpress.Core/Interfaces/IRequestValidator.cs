namespace FoodExpress.Core.Interfaces
{
    public interface IRequestValidator
    {
        void Validate(object? request);
        void PositiveId(int id);
    }
}