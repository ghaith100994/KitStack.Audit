using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Narration.Contracts;
using KitStack.Audit.Narration.Extensions;
using KitStack.Audit.Narration.Options;
using KitStack.Audit.Narration.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KitStack.Audit.Tests.Narration;

public class NarrationRegistrationTests
{
    [Fact]
    public void Toolkit_registers_services_and_merges_configuration()
    {
        var services = new ServiceCollection()
            .AddKitStackAuditNarrationToolkit(o => o.SystemFields.Add("RowVersion"))
            .AddKitStackAuditNarrationToolkit(o => o.ActivationProperty = "Enabled");

        using var provider = services.BuildServiceProvider();
        var narration = provider.GetRequiredService<INarrationServices>();

        Assert.Single(services, d => d.ServiceType == typeof(NarrationOptions));
        Assert.Contains("RowVersion", narration.Options.SystemFields);
        Assert.Equal("Enabled", narration.Options.ActivationProperty);
        Assert.IsType<DefaultNarrationPhrases>(narration.Phrases);
        Assert.IsType<DefaultEnumLabelProvider>(narration.EnumLabels);
    }

    [Fact]
    public void Enum_label_provider_can_be_replaced_in_either_order()
    {
        var before = new ServiceCollection().AddNarrationEnumLabelProvider<ArabicStatusLabels>().AddKitStackAuditNarrationToolkit();
        var after = new ServiceCollection().AddKitStackAuditNarrationToolkit().AddNarrationEnumLabelProvider<ArabicStatusLabels>();

        using var p1 = before.BuildServiceProvider();
        using var p2 = after.BuildServiceProvider();
        Assert.IsType<ArabicStatusLabels>(p1.GetRequiredService<INarrationServices>().EnumLabels);
        Assert.IsType<ArabicStatusLabels>(p2.GetRequiredService<INarrationServices>().EnumLabels);
    }

    [Fact]
    public void Narrators_are_discovered_once_and_resolved()
    {
        var services = new ServiceCollection()
            .AddKitStackAuditNarrationToolkit()
            .AddAuditNarratorsFromAssembly(typeof(ShopOrderNarrator).Assembly)
            .AddAuditNarratorsFromAssembly(typeof(ShopOrderNarrator).Assembly);

        using var provider = services.BuildServiceProvider();
        var narrators = provider.GetServices<IAuditNarrator>().Select(n => n.GetType()).ToList();

        Assert.Single(narrators, t => t == typeof(ShopOrderNarrator));
        Assert.Single(narrators, t => t == typeof(ProbeNarrator));
    }

    [Fact]
    public void Base_type_filter_accepts_open_generics_and_closed_types()
    {
        var openGeneric = new ServiceCollection()
            .AddAuditNarratorsFromAssembly(typeof(ShopOrderNarrator).Assembly, typeof(NarratorBase<>));
        var closed = new ServiceCollection()
            .AddAuditNarratorsFromAssembly(typeof(ShopOrderNarrator).Assembly, typeof(ShopOrderNarrator));

        Assert.Equal(2, openGeneric.Count(d => d.ServiceType == typeof(IAuditNarrator)));
        Assert.Equal(typeof(ShopOrderNarrator), Assert.Single(closed).ImplementationType);
    }
}
