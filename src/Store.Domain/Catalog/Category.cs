using Store.Domain.Common;

namespace Store.Domain.Catalog;

/// <summary>
/// Group of products shown in the catalog and used to aggregate sales in the dashboard.
/// </summary>
public sealed class Category : Entity
{
    public const int NameMaxLength = 100;

    private Category(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    public string Name { get; private set; }

    /// <summary>URL-friendly identifier derived from the name, unique across categories.</summary>
    public string Slug { get; private set; }

    public static Category Create(string name)
    {
        var validName = Guard.NotEmpty(name, nameof(name), NameMaxLength);
        return new Category(validName, BuildSlug(validName));
    }

    public void Rename(string name)
    {
        var validName = Guard.NotEmpty(name, nameof(name), NameMaxLength);
        Slug = BuildSlug(validName);
        Name = validName;
    }

    private static string BuildSlug(string name)
    {
        var slug = SlugGenerator.From(name);
        return slug.Length > 0
            ? slug
            : throw new DomainException("Category name must contain at least one letter or digit.");
    }
}
