namespace BridgeTask.Api.Common.Pagination;

public enum SortDirection
{
    Asc,
    Desc
}

/// Common paging and sorting parameters bound from the query string.
public abstract record PagedQuery
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    /// 1-based page number.
    public int PageNumber { get; init; } = 1;

    /// Items per page (1-100).
    public int PageSize { get; init; } = DefaultPageSize;

    /// Field to sort by; allowed values depend on the resource.
    public string? SortBy { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
