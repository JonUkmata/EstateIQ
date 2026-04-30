using System.ComponentModel.DataAnnotations;

namespace EstateIQ.DTOs;

/// <summary>
/// Represents query string filters for the properties list endpoint.
/// </summary>
public class PropertyQueryParameters
{
    public string? City { get; set; }

    public int? PropertyTypeId { get; set; }

    public int? PropertyStatusId { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
