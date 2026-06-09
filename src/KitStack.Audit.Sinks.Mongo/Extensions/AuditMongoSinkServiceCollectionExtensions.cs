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
/// Registers the MongoDB audit sink using the MongoDB.EntityFrameworkCore provider.
///
/// <code>
/// services.AddKitStackAuditMongoSink(
///     connectionString: configuration["Audit:Database:ConnectionString"]!,
///     databaseName:     configuration["Audit:Database:DatabaseName"]!);
/// </code>
///
/// Trails are written to the "AuditTrails" collection.
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

        // Store Guids as strings (matches the original ERP behavior). Safe to call repeatedly.
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(BsonType.String));

        services.AddDbContext<MongoAuditDbContext>(o => o.UseMongoDB(connectionString, databaseName));
        services.TryAddScoped<IAuditSink, MongoAuditSink>();

        return services;
    }
}
