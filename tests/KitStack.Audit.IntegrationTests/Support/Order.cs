namespace KitStack.Audit.IntegrationTests.Support;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Customer { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
