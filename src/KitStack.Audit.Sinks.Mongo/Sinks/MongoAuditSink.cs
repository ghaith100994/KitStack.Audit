using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Sinks.Mongo.Documents;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace KitStack.Audit.Sinks.Mongo.Sinks;

/// <summary>
/// Persists captured trails to a MongoDB collection as native documents.
/// Errors are logged and swallowed — the originating business transaction has already committed.
/// </summary>
public sealed class MongoAuditSink : IAuditSink
{
    private readonly IMongoCollection<AuditTrailDocument> _collection;
    private readonly ILogger<MongoAuditSink>? _logger;

    public MongoAuditSink(IMongoCollection<AuditTrailDocument> collection, ILogger<MongoAuditSink>? logger = null)
    {
        _collection = collection;
        _logger = logger;
    }

    public async Task WriteAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default)
    {
        if (trails is null || trails.Count == 0)
            return;

        try
        {
            var documents = new List<AuditTrailDocument>(trails.Count);
            foreach (var dto in trails)
                documents.Add(AuditTrailDocument.From(dto));

            await _collection.InsertManyAsync(documents, options: null, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while saving {Count} audit trails to MongoDB.", trails.Count);
        }
    }
}
