namespace InventoryManager.Application.DTOs.Common;

public sealed record PagedMeta(int TotalItems, int CurrentPage, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}