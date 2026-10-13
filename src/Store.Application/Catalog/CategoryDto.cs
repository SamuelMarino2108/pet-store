namespace Store.Application.Catalog;

/// <summary>A catalog category and how many products it has.</summary>
public sealed record CategoryDto(Guid Id, string Slug, string Name, int ProductCount);
