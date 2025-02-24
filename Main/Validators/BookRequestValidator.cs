using FluentValidation;
using Main.Requests;

namespace Main.Validators;

public class BookRequestValidator : AbstractValidator<BookRequest>
{
    public BookRequestValidator()
    {
        RuleFor(x => x.OptionCode)
            .NotEmpty().WithMessage("OptionCode is required");

        RuleFor(x => x.SearchRequest)
            .NotNull().WithMessage("SearchReq is required");

        RuleFor(x => x.SearchRequest)
            .ChildRules(searchReq =>
            {
                searchReq.RuleFor(s => s.Destination)
                    .NotEmpty().WithMessage("Destination is required");

                searchReq.RuleFor(s => s.FromDate)
                    .NotNull().WithMessage("FromDate is required")
                    .GreaterThanOrEqualTo(DateTime.Today).WithMessage("FromDate cannot be in the past")
                    .LessThanOrEqualTo(s => s.ToDate).WithMessage("FromDate must be before or equal to ToDate");


                searchReq.RuleFor(s => s.ToDate)
                    .NotNull().WithMessage("ToDate is required");
            });
    }
}
