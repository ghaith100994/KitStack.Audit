namespace KitStack.Audit.Tests.Support;

public class Widget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
