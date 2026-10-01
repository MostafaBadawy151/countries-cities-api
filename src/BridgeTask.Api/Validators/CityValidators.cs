using BridgeTask.Api.Data.Configurations;
using BridgeTask.Api.Dtos.Cities;
using FluentValidation;

namespace BridgeTask.Api.Validators;

internal static class CityRules
{
    public static IRuleBuilderOptions<T, string> ValidCityName<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(CityConfiguration.NameMaxLength);
}

public sealed class CreateCityRequestValidator : AbstractValidator<CreateCityRequest>
{
    public CreateCityRequestValidator()
    {
        RuleFor(x => x.Name).ValidCityName();
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.Population).GreaterThanOrEqualTo(0).When(x => x.Population.HasValue);
    }
}

public sealed class UpdateCityRequestValidator : AbstractValidator<UpdateCityRequest>
{
    public UpdateCityRequestValidator()
    {
        RuleFor(x => x.Name).ValidCityName();
        RuleFor(x => x.CountryId).GreaterThan(0);
        RuleFor(x => x.Population).GreaterThanOrEqualTo(0).When(x => x.Population.HasValue);
    }
}

public sealed class CityQueryValidator : PagedQueryValidator<CityQuery>
{
    public CityQueryValidator() : base(CityQuery.SortableFields)
    {
        RuleFor(q => q.Search).MaximumLength(CityConfiguration.NameMaxLength);
        RuleFor(q => q.CountryId).GreaterThan(0).When(q => q.CountryId.HasValue);
        RuleFor(q => q.MinPopulation).GreaterThanOrEqualTo(0).When(q => q.MinPopulation.HasValue);
        RuleFor(q => q.MaxPopulation).GreaterThanOrEqualTo(0).When(q => q.MaxPopulation.HasValue);
        RuleFor(q => q)
            .Must(q => q.MinPopulation <= q.MaxPopulation)
            .When(q => q.MinPopulation.HasValue && q.MaxPopulation.HasValue)
            .WithName("MinPopulation")
            .WithMessage("'Min Population' must be less than or equal to 'Max Population'.");
    }
}

public sealed class CountryCitiesQueryValidator : PagedQueryValidator<CountryCitiesQuery>
{
    public CountryCitiesQueryValidator() : base(CityQuery.SortableFields)
    {
        RuleFor(q => q.Search).MaximumLength(CityConfiguration.NameMaxLength);
    }
}
