using InventoryManager.Application.DTOs.Common;
using System.Text.Json;

namespace InventoryManager.API.Extensions;

public static class HttpHeaderExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void AddPaginationHeader(this HttpResponse response, PagedMeta meta)
    {
        string json = JsonSerializer.Serialize(meta, JsonOptions);

        response.Headers.Append("X-Pagination", json);
        response.Headers.Append("Access-Control-Expose-Headers", "X-Pagination");
    }
}