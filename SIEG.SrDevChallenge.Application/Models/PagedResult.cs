using System;

namespace SIEG.SrDevChallenge.Application.Models;

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public long TotalCount { get; init; }

    public int TotalPages =>
        (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PagedResult(
        IReadOnlyCollection<T> items,
        int page,
        int pageSize,
        long totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}