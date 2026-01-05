namespace InventoryManager.Application.DTOs.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalItems, int Page, int PageSize)
{
    public PagedMeta Meta { get; } = new(TotalItems, Page, PageSize);
}