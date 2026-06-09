using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Sinks.Mongo.Documents;
using KitStack.Audit.Sinks.Mongo.Sinks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace KitStack.Audit.Sinks.Mongo.Extensions;

/// <summary>
/// Registers the MongoDB audit sink.
///
/// <code>
/// services.AddKitStackAuditMongoSink(
///     connectionString: configuration["Audit:Database:ConnectionString"]!,
///     databaseName:     configuration["Audit:Database:DatabaseName"]!,
///     collectionName:   "AuditTrails");
/// </code>
/// </summary>
public static class AuditMongoSinkServiceCollectionExtensions
{
    public static IServiceCollection AddKitStackAuditMongoSink(
        this IServiceCollection services,
        string connectionString,
        string databaseName,
        string collectionName = "AuditTrails")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        // Store Guids as strings (matches the original ERP behavior). Safe to call repeatedly.
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(BsonType.String));

        services.TryAddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.TryAddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(databaseName);
            return database.GetCollection<AuditTrailDocument>(collectionName);
        });

        services.TryAddScoped<IAuditSink, MongoAuditSink>();

        return services;
    }
}
