using FoodExpress.Services.Services.Implementations;
using System.ComponentModel.DataAnnotations;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Validators
{
    public class RequestValidator : IRequestValidator
    {
        public void Validate(object? request)
        {
            if (request is null)
            {
                throw new ServiceException(ServiceError.Validation, "A request body is required.");
            }
            var errors = new List<ValidationResult>();
            Validator.TryValidateObject(request, new ValidationContext(request), errors, true);
            var price = request switch {
                CreateMenuItemDTO create => create.Price,
                UpdateMenuItemDTO update => update.Price,
                _ => null
            };
            if (price.HasValue && (price.Value < 0 || price.Value > 999999.99m || decimal.Round(price.Value, 2) != price.Value)) {
                errors.Add(new ValidationResult("Price must have at most two decimal places and be between 0 and 999999.99.", ["Price"]));
            }
            if (errors.Count > 0)
            {
                throw new ServiceException(ServiceError.Validation, "Validation failed.",
                    errors.SelectMany(error => error.MemberNames.DefaultIfEmpty("Request")
                        .Select(member => (Member: member, Message: error.ErrorMessage ?? "Invalid value.")))
                        .GroupBy(x => x.Member).ToDictionary(x => x.Key, x => x.Select(e => e.Message).Distinct().ToArray()));
            }
        }

        public void PositiveId(int id)
        {
            if (id <= 0)
            {
                throw new ServiceException(ServiceError.Validation, "The ID must be positive.");
            }
        }
    }
}