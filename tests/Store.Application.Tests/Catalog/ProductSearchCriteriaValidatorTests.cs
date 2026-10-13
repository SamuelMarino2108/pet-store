using Store.Application.Catalog;

namespace Store.Application.Tests.Catalog;

public class ProductSearchCriteriaValidatorTests
{
    private readonly ProductSearchCriteriaValidator _validator = new();

    [Fact]
    public void Default_criteria_are_valid()
    {
        Assert.True(_validator.Validate(new ProductSearchCriteria()).IsValid);
    }

    [Fact]
    public void Full_criteria_are_valid()
    {
        var criteria = new ProductSearchCriteria
        {
            Category = "juguetes",
            Search = "pelota",
            MinPrice = 10_000,
            MaxPrice = 10_000,
            InStockOnly = true,
            Sort = ProductSort.PriceDesc,
            Page = 3,
            PageSize = ProductSearchCriteria.MaxPageSize,
        };

        Assert.True(_validator.Validate(criteria).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(ProductSearchCriteria.MaxPage + 1)]
    public void Page_out_of_range_is_invalid(int page)
    {
        AssertInvalid(new ProductSearchCriteria { Page = page }, nameof(ProductSearchCriteria.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ProductSearchCriteria.MaxPageSize + 1)]
    public void Page_size_out_of_range_is_invalid(int pageSize)
    {
        AssertInvalid(new ProductSearchCriteria { PageSize = pageSize }, nameof(ProductSearchCriteria.PageSize));
    }

    [Fact]
    public void Search_longer_than_max_length_is_invalid()
    {
        var criteria = new ProductSearchCriteria { Search = new string('a', ProductSearchCriteria.SearchMaxLength + 1) };

        AssertInvalid(criteria, nameof(ProductSearchCriteria.Search));
    }

    [Fact]
    public void Negative_min_price_is_invalid()
    {
        AssertInvalid(new ProductSearchCriteria { MinPrice = -1 }, nameof(ProductSearchCriteria.MinPrice));
    }

    [Fact]
    public void Max_price_below_min_price_is_invalid()
    {
        var criteria = new ProductSearchCriteria { MinPrice = 50_000, MaxPrice = 20_000 };

        AssertInvalid(criteria, nameof(ProductSearchCriteria.MaxPrice));
    }

    [Fact]
    public void Undefined_sort_is_invalid()
    {
        AssertInvalid(new ProductSearchCriteria { Sort = (ProductSort)99 }, nameof(ProductSearchCriteria.Sort));
    }

    private void AssertInvalid(ProductSearchCriteria criteria, string propertyName)
    {
        var result = _validator.Validate(criteria);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == propertyName);
    }
}
