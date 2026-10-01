using BridgeTask.Api.Data.Configurations;
using BridgeTask.Api.Dtos.Countries;
using FluentValidation;

namespace BridgeTask.Api.Validators;

internal static class CountryRules
{
    public static IRuleBuilderOptions<T, string> ValidCountryName<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(CountryConfiguration.NameMaxLength);

    public static IRuleBuilderOptions<T, string> ValidCountryCode<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Matches("^[A-Za-z]{2}$").WithMessage("'Code' must be a 2-letter ISO 3166-1 alpha-2 code.");
}

public sealed class CreateCountryRequestValidator : AbstractValidator<CreateCountryRequest>
{
    public CreateCountryRequestValidator()
    {
        RuleFor(x => x.Name).ValidCountryName();
        RuleFor(x => x.Code).ValidCountryCode();
    }
}

public sealed class UpdateCountryRequestValidator : AbstractValidator<UpdateCountryRequest>
{
    public UpdateCountryRequestValidator()
    {
        RuleFor(x => x.Name).ValidCountryName();
        RuleFor(x => x.Code).ValidCountryCode();
    }
}

public sealed class CountryQueryValidator : PagedQueryValidator<CountryQuery>
{
    public CountryQueryValidator() : base(CountryQuery.SortableFields)
    {
        RuleFor(q => q.Search).MaximumLength(CountryConfiguration.NameMaxLength);
        RuleFor(q => q.Code!).ValidCountryCode().When(q => q.Code is not null);
    }
}
