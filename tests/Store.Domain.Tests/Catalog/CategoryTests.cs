using Store.Domain.Catalog;
using Store.Domain.Common;

namespace Store.Domain.Tests.Catalog;

public class CategoryTests
{
    [Theory]
    [InlineData("Juguetes para Gatos", "juguetes-para-gatos")]
    [InlineData("  Camas & Cojines ", "camas-cojines")]
    [InlineData("Alimentación Húmeda", "alimentacion-humeda")]
    [InlineData("Accesorios de paseo (perros)", "accesorios-de-paseo-perros")]
    public void Create_builds_slug_from_name(string name, string expectedSlug)
    {
        var category = Category.Create(name);

        Assert.Equal(name.Trim(), category.Name);
        Assert.Equal(expectedSlug, category.Slug);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("---")]
    public void Create_rejects_names_without_letters_or_digits(string name)
    {
        Assert.Throws<DomainException>(() => Category.Create(name));
    }

    [Fact]
    public void Rename_updates_name_and_slug()
    {
        var category = Category.Create("Collares");

        category.Rename("Collares y Correas");

        Assert.Equal("Collares y Correas", category.Name);
        Assert.Equal("collares-y-correas", category.Slug);
    }

    [Fact]
    public void Each_category_gets_a_unique_id()
    {
        Assert.NotEqual(Category.Create("Camas").Id, Category.Create("Camas").Id);
    }
}
