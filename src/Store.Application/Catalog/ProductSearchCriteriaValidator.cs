using FluentValidation;
using Store.Domain.Catalog;

namespace Store.Application.Catalog;

internal sealed class ProductSearchCriteriaValidator : AbstractValidator<ProductSearchCriteria>
{
    public ProductSearchCriteriaValidator()
    {
        RuleFor(c => c.Page).InclusiveBetween(1, ProductSearchCriteria.MaxPage);
        RuleFor(c => c.PageSize).InclusiveBetween(1, ProductSearchCriteria.MaxPageSize);
        RuleFor(c => c.Search).MaximumLength(ProductSearchCriteria.SearchMaxLength);
        RuleFor(c => c.Category).MaximumLength(Category.NameMaxLength);
        RuleFor(c => c.Sort).IsInEnum();
        RuleFor(c => c.MinPrice).GreaterThanOrEqualTo(0);
        RuleFor(c => c.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .GreaterThanOrEqualTo(c => c.MinPrice!.Value)
            .When(c => c.MinPrice.HasValue, ApplyConditionTo.CurrentValidator);
    }
}
