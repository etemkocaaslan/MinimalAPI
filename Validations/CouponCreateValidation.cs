using FluentValidation;
using MinimalAPI.Models.DTO;

namespace MinimalAPI.Validations
{
    public class CouponCreateValidation : AbstractValidator<CouponCreateDTO>
    {
        public CouponCreateValidation()
        {
            RuleFor(model => model.Name).NotEmpty();
            RuleFor(model => model.Percent).InclusiveBetween(1, 100);
        }
    }
}
