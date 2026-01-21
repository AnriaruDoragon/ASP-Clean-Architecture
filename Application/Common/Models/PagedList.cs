namespace Application.Common.Models;

/// <summary>
/// Represents a paginated list of items.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PagedList<T>(IReadOnlyList<T> items, int total, int page, int size)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int Page { get; } = page;
    public int Size { get; } = size;
    public int Total { get; } = total;
    public int Pages => (int)Math.Ceiling(Total / (double)Size);
    public int? Previous => Page > 1 ? (Page - 1 > Pages) ? Pages : null : null;
    public int? Next => Page < Pages ? Page + 1 : null;

    public static PagedList<T> Empty(int pageNumber = 1, int pageSize = 10) =>
        new([], 0, pageNumber, pageSize);
}
