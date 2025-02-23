using FluentValidation;
using Main.Requests;

namespace Main.Validators;

public class SearchReqValidator : AbstractValidator<SearchReq>
{
    public SearchReqValidator()
    {
        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.");

        RuleFor(x => x.FromDate)
            .NotEmpty().WithMessage("From Date is required.");

        RuleFor(x => x.ToDate)
            .NotEmpty().WithMessage("To Date is required.");
    }
}
