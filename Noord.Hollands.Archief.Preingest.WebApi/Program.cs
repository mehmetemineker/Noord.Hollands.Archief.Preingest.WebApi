using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Noord.Hollands.Archief.Preingest.WebApi.Entities;
using Noord.Hollands.Archief.Preingest.WebApi.EventHub;
using Noord.Hollands.Archief.Preingest.WebApi.Handlers;
using Noord.Hollands.Archief.Preingest.WebApi.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddCors();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});


builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);

var settings = appSettingsSection.Get<AppSettings>();
builder.Services.AddDbContext<PreIngestStatusContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

builder.Services.AddSingleton<PreingestEventHub>();
builder.Services.AddSingleton<CollectionHandler>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "events.html", "collection.html", "collections.html" }
});

app.UseStaticFiles();

var origins = settings.WithOrigins.Split(";");

app.UseCors(x => x.AllowAnyOrigin().AllowCredentials().AllowAnyMethod().AllowAnyHeader().WithOrigins(origins));

app.UseStatusCodePages();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHub<PreingestEventHub>("/preingestEventHub");

CreateDbIfNotExists(app);

app.Run();


static void CreateDbIfNotExists(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<PreIngestStatusContext>();
            context.Database.EnsureCreated();
            // DbInitializer.Initialize(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB.");
        }
    }
}