using Store.Domain.Common;

namespace Store.Domain.Catalog;

/// <summary>
/// Identifies a product in a supplier's catalog. The domain only knows the supplier's code
/// and the external id; each supplier integration interprets them.
/// </summary>
public sealed record SupplierReference
{
    public const int SupplierCodeMaxLength = 50;
    public const int SupplierProductIdMaxLength = 100;

    private SupplierReference(string supplierCode, string supplierProductId)
    {
        SupplierCode = supplierCode;
        SupplierProductId = supplierProductId;
    }

    public string SupplierCode { get; }

    public string SupplierProductId { get; }

    public static SupplierReference Create(string supplierCode, string supplierProductId)
    {
        var code = Guard.NotEmpty(supplierCode, nameof(supplierCode), SupplierCodeMaxLength).ToLowerInvariant();
        var productId = Guard.NotEmpty(supplierProductId, nameof(supplierProductId), SupplierProductIdMaxLength);

        return new SupplierReference(code, productId);
    }
}
