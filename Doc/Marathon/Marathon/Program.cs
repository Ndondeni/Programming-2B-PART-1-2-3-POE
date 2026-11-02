using Microsoft.EntityFrameworkCore;
using Marathon.Data;
using Microsoft.Extensions.Azure;
using Marathon.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity; // Added this to keep things clean

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

// 2. Configure Swagger and Controllers
builder.Services.AddControllers();
builder.Services.AddScoped<IPasswordHasher<Marathon.Models.UserAccount>, PasswordHasher<Marathon.Models.UserAccount>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // <--- Removed ResolveConflictingActions

// 3. Register Custom Services
builder.Services.AddScoped<JwtServives>();

// 4. Database Context
builder.Services.AddDbContext<ApplicationDbcontext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Azure Blob Storage & Services 
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();