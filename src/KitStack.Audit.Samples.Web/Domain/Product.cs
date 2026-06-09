using KitStack.Audit.Abstractions.Attributes;

namespace KitStack.Audit.Samples.Web.Domain;

[AuditDefinition(module: "Catalog", subModule: "Products", resource: "Product",
                 displayEn: "Product", displayAr: "منتج")]
public class Product : IAggregateRoot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
