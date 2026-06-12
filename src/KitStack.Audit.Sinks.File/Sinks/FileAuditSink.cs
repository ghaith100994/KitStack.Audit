using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Sinks.File.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KitStack.Audit.Sinks.File.Sinks;

/// <summary>
/// Appends trails and activities as JSON Lines (one JSON document per line) to rolling files.
/// Useful for lightweight deployments, shipping into log pipelines, and local diagnostics.
/// Errors are logged and swallowed — the originating business transaction has already committed.
/// </summary>
public sealed class FileAuditSink : IAuditSink, IActivitySink
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly FileAuditSinkOptions _options;
    private readonly ILogger<FileAuditSink>? _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileAuditSink(IOptions<FileAuditSinkOptions> options, ILogger<FileAuditSink>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _logger = logger;
    }

    public Task WriteAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default)
        => AppendAsync(_options.TrailFilePrefix, trails, cancellationToken);

    public Task WriteActivitiesAsync(IReadOnlyList<ActivityEvent> activities, CancellationToken cancellationToken = default)
        => AppendAsync(_options.ActivityFilePrefix, activities, cancellationToken);

    /// <summary>The file a record written right now would land in (mainly for diagnostics/tests).</summary>
    public string GetCurrentTrailFilePath() => BuildFilePath(_options.TrailFilePrefix, DateTime.UtcNow);

    /// <summary>The file an activity written right now would land in (mainly for diagnostics/tests).</summary>
    public string GetCurrentActivityFilePath() => BuildFilePath(_options.ActivityFilePrefix, DateTime.UtcNow);

    private async Task AppendAsync<T>(string prefix, IReadOnlyList<T> records, CancellationToken ct)
    {
        if (records is null || records.Count == 0)
            return;

        try
        {
            var builder = new StringBuilder();
            foreach (var record in records)
                builder.AppendLine(JsonSerializer.Serialize(record, SerializerOptions));

            var path = BuildFilePath(prefix, DateTime.UtcNow);

            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await System.IO.File.AppendAllTextAsync(path, builder.ToString(), ct).ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while appending {Count} audit record(s) to file.", records.Count);
        }
    }

    private string BuildFilePath(string prefix, DateTime utcNow)
    {
        var suffix = _options.RollInterval switch
        {
            FileRollInterval.Day => $"-{utcNow:yyyyMMdd}",
            FileRollInterval.Month => $"-{utcNow:yyyyMM}",
            _ => string.Empty,
        };

        return Path.GetFullPath(Path.Combine(_options.Directory, $"{prefix}{suffix}.jsonl"));
    }
}
