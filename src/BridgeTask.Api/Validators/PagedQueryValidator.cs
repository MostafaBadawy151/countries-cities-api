using BridgeTask.Api.Common.Pagination;
using FluentValidation;

namespace BridgeTask.Api.Validators;

public abstract class PagedQueryValidator<T> : AbstractValidator<T> where T : PagedQuery
{
    protected PagedQueryValidator(IReadOnlyCollection<string> sortableFields)
    {
        RuleFor(q => q.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, PagedQuery.MaxPageSize);

        RuleFor(q => q.SortBy)
            .Must(s => sortableFields.Contains(s!, StringComparer.OrdinalIgnoreCase))
            .When(q => !string.IsNullOrWhiteSpace(q.SortBy))
            .WithMessage($"'Sort By' must be one of: {string.Join(", ", sortableFields)}.");

        RuleFor(q => q.SortDirection)
            .IsInEnum();
    }
}
