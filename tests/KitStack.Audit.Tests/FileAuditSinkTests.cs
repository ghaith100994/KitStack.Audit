using System.Text.Json;
using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Sinks.File.Options;
using KitStack.Audit.Sinks.File.Sinks;
using Microsoft.Extensions.Options;
using Xunit;

namespace KitStack.Audit.Tests;

public sealed class FileAuditSinkTests : IDisposable
{
    private readonly string _directory =
        Path.Combine(Path.GetTempPath(), "kitstack-audit-tests", Guid.NewGuid().ToString("N"));

    private FileAuditSink CreateSink(FileRollInterval interval = FileRollInterval.Day)
        => new(Microsoft.Extensions.Options.Options.Create(new FileAuditSinkOptions
        {
            Directory = _directory,
            RollInterval = interval,
        }));

    [Fact]
    public async Task Writes_one_json_line_per_trail()
    {
        var sink = CreateSink();

        var trail = new TrailDto { TableName = "Order", Type = TrailType.Create };
        trail.NewValues["Status"] = "Draft";

        await sink.WriteAsync(new[] { trail, new TrailDto { TableName = "Invoice", Type = TrailType.Delete } });

        var lines = await File.ReadAllLinesAsync(sink.GetCurrentTrailFilePath());
        Assert.Equal(2, lines.Length);

        var first = JsonSerializer.Deserialize<JsonElement>(lines[0]);
        Assert.Equal("Order", first.GetProperty("TableName").GetString());
        Assert.Equal("Draft", first.GetProperty("NewValues").GetProperty("Status").GetString());
    }

    [Fact]
    public async Task Appends_across_writes()
    {
        var sink = CreateSink();

        await sink.WriteAsync(new[] { new TrailDto { TableName = "A", Type = TrailType.Create } });
        await sink.WriteAsync(new[] { new TrailDto { TableName = "B", Type = TrailType.Update } });

        var lines = await File.ReadAllLinesAsync(sink.GetCurrentTrailFilePath());
        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public async Task Activities_go_to_their_own_file()
    {
        var sink = CreateSink();

        await sink.WriteActivitiesAsync(new[]
        {
            new ActivityEvent { EntityType = "Order", Action = TrailType.Create, TitleEn = "Order created" },
        });

        Assert.False(File.Exists(sink.GetCurrentTrailFilePath()));
        var lines = await File.ReadAllLinesAsync(sink.GetCurrentActivityFilePath());
        var activity = JsonSerializer.Deserialize<JsonElement>(Assert.Single(lines));
        Assert.Equal("Order created", activity.GetProperty("TitleEn").GetString());
    }

    [Fact]
    public async Task Roll_interval_changes_file_name()
    {
        var daily = CreateSink(FileRollInterval.Day);
        var single = CreateSink(FileRollInterval.None);

        Assert.EndsWith($"audit-{DateTime.UtcNow:yyyyMMdd}.jsonl", daily.GetCurrentTrailFilePath());
        Assert.EndsWith(Path.Combine(_directory, "audit.jsonl"), single.GetCurrentTrailFilePath());

        await single.WriteAsync(new[] { new TrailDto { TableName = "A", Type = TrailType.Create } });
        Assert.True(File.Exists(Path.Combine(_directory, "audit.jsonl")));
    }

    [Fact]
    public async Task Empty_batches_write_nothing()
    {
        var sink = CreateSink();
        await sink.WriteAsync(Array.Empty<TrailDto>());
        Assert.False(File.Exists(sink.GetCurrentTrailFilePath()));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
