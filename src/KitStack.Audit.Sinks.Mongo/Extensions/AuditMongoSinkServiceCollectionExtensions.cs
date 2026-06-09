using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Sinks.Mongo.Persistence;
using KitStack.Audit.Sinks.Mongo.Sinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace KitStack.Audit.Sinks.Mongo.Extensions;

/// <summary>
/// Registers the MongoDB audit sink using the MongoDB.EntityFrameworkCore provider. The same sink
/// instance backs both <see cref="IAuditSink"/> and <see cref="IActivitySink"/>.
/// Trails go to the "AuditTrails" collection; activities go to "ActivityEvents".
/// </summary>
public static class AuditMongoSinkServiceCollectionExtensions
{
    public static IServiceCollection AddKitStackAuditMongoSink(
        this IServiceCollection services,
        string connectionString,
        string databaseName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        BsonSerializer.TryRegisterSerializer(new GuidSerializer(BsonType.String));

        services.AddDbContext<MongoAuditDbContext>(o => o.UseMongoDB(connectionString, databaseName));

        services.TryAddScoped<MongoAuditSink>();
        services.TryAddScoped<IAuditSink>(sp => sp.GetRequiredService<MongoAuditSink>());
        services.TryAddScoped<IActivitySink>(sp => sp.GetRequiredService<MongoAuditSink>());

        return services;
    }
}
