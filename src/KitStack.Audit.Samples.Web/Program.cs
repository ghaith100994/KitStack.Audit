using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.EntityFrameworkCore.Interceptors;
using KitStack.Audit.Samples.Web.Auditing;
using KitStack.Audit.Samples.Web.Domain;
using KitStack.Audit.Samples.Web.Persistence;
using KitStack.Audit.Sinks.EntityFrameworkCore.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string businessConnection = "Data Source=sample-business.db";
var auditConnection = builder.Configuration["Audit:Database:ConnectionString"]
                      ?? "Data Source=sample-audit.db";

// 1) Who is acting (application-specific).
builder.Services.AddScoped<IAuditContextAccessor, SampleAuditContextAccessor>();

// 2) Capture (restricted to aggregate roots) + the sink chosen by Audit:Sink in appsettings.
//    For the efcore sink we supply the relational provider here.
builder.Services.AddKitStackAudit<IAggregateRoot>(
    builder.Configuration,
    efSinkProvider: o => o.UseSqlite(auditConnection));

// 3) Business context with the interceptor attached.
builder.Services.AddDbContext<SampleDbContext>((sp, o) =>
{
    o.UseSqlite(businessConnection);
    o.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

var app = builder.Build();

// Create the demo schemas (use migrations in a real app).
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<SampleDbContext>().Database.EnsureCreated();
    scope.ServiceProvider.GetRequiredService<AuditDbContext>().Database.EnsureCreated();
}

app.MapPost("/products", async (Product product, SampleDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id:guid}", async (Guid id, Product input, SampleDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    product.Name = input.Name;
    product.Price = input.Price;
    product.IsActive = input.IsActive;
    await db.SaveChangesAsync();
    return Results.Ok(product);
});

app.MapDelete("/products/{id:guid}", async (Guid id, SampleDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Read back the captured trails from the audit store.
app.MapGet("/audit", async (AuditDbContext audit) =>
    await audit.AuditTrails
        .OrderByDescending(t => t.DateTime)
        .Take(100)
        .ToListAsync());

app.Run();
