using Microsoft.EntityFrameworkCore;
using Marathon.Data;
using Microsoft.Extensions.Azure;
using Marathon.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// 2. Configure Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Prevents Swagger from crashing if you have duplicate routes
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// 3. Register Custom Services
builder.Services.AddScoped<JwtServives>();

// 4. Database Context
builder.Services.AddDbContext<ApplicationDbcontext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 5. Azure Blob Storage & Services (MOVED BEFORE Build())
var storageConnectionString = builder.Configuration
    .GetConnectionString("StorageConnectionString")
    ?? throw new Exception("Storage connection missing");

builder.Services.AddSingleton(new BlobServiceClient(storageConnectionString));

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnectionString:blobServiceUri"]!).WithName("StorageConnectionString");
    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConnectionString:queueServiceUri"]!).WithName("StorageConnectionString");
    clientBuilder.AddTableServiceClient(builder.Configuration["StorageConnectionString:tableServiceUri"]!).WithName("StorageConnectionString");
});

// ==========================================
// BUILD THE APP
// ==========================================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CRITICAL: You must have UseAuthentication() BEFORE UseAuthorization()
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();