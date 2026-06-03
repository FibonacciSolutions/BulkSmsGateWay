
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OmniRoute.Api.Middleware;
using OmniRoute.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. REGISTER CORE ENGINE SERVICES (Dependency Injection Container)
// =========================================================================

// Allow Controllers to handle incoming endpoints
builder.Services.AddControllers();

// 🚀 THE CRITICAL FIX: Registers IHttpClientFactory to allow WhatsApp Node communications
builder.Services.AddHttpClient();

// Register your Entity Framework Core DB Context pointing to SQL Server connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=OmniRouteDb;Trusted_Connection=True;"));

// Configure Swagger/OpenAPI with Support for Custom API Key Authentication headers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OmniRoute Platform API Gateway", Version = "v1" });

    // Add Security Definition for X-API-KEY header
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication needed to access routing pipelines. Example: 'X-API-KEY Your_Secret_Key'",
        In = ParameterLocation.Header,
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Configure CORS policy to allow your local React frontend (Port 5173) to fetch data safely
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDashboard", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// =========================================================================
// 2. CONFIGURE THE HTTP REQUEST PIPELINE (Middleware Stack)
// =========================================================================

// Enable Developer Exception Page if running locally to trace database/endpoint errors easily
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OmniRoute.Api v1"));
}

// Enable CORS Policy across all route endpoints
app.UseCors("AllowReactDashboard");

// Enforce your custom API Key/Tenant Isolation validation middleware block
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseRouting();
app.UseAuthorization();

// Map your controller routes automatically (e.g., api/v1/Message/send)
app.MapControllers();

// Launch the system engine!
app.Run();