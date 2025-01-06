using Sheleni_API.Data;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Generate the secret key
string secretKey = configuration["Jwt:Key"];
if (string.IsNullOrEmpty(secretKey))
{
    secretKey = GenerateRandomString();

    // Read the contents of the appsettings.json file
    var appSettingsJson = File.ReadAllText("appsettings.json");

    // Update the secret key value
    var jsonObject = JObject.Parse(appSettingsJson);
    jsonObject["Jwt"]["Key"] = secretKey;

    // Write the updated contents back to the appsettings.json file
    File.WriteAllText("appsettings.json", jsonObject.ToString());

    // Rebuild the configuration with the updated values
    configuration = new ConfigurationBuilder()
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
}

// Get the issuer and audience from configuration
string issuer = configuration["Jwt:Issuer"];
string audience = configuration["Jwt:Audience"];

// Configure the database connection
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)); // UseNpgsql for PostgreSQL

// Configure authentication
//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = issuer,
//            ValidAudience = audience,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
//        };
//    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Helper method to generate a random string
string GenerateRandomString()
{
    using (var rng = RandomNumberGenerator.Create())
    {
        var randomBytes = new byte[32]; // Adjust the byte length based on your requirements
        rng.GetBytes(randomBytes);
        var secretKey = Convert.ToBase64String(randomBytes);
        Console.WriteLine($"Generated Secret Key: {secretKey}");
        Console.WriteLine($"Secret Key Length: {secretKey.Length}");
        return secretKey;
    }
}
