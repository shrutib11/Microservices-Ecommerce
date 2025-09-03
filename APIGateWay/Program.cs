using System.Security.Claims;
using System.Text;
using Microservices.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

//ADD CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") 
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();
// var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
// builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
// builder.Services.AddAuthentication("Bearer")
//     .AddJwtBearer("Bearer", options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidIssuer = jwtSettings!.Issuer,
//             ValidateAudience = true,
//             ValidAudience = jwtSettings.Audience,
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(jwtSettings.Key)
//             ),
//             ValidateLifetime = true,
//         };
//     });

var jwtSection = builder.Configuration.GetSection("Jwt");
var authority = jwtSection["Authority"];
var requireHttps = bool.Parse(jwtSection["RequireHttpsMetadata"] ?? "false");
var audiences = jwtSection.GetSection("Audiences").Get<string[]>();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = requireHttps;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,

            ValidateAudience = true,
            ValidAudiences = audiences,

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "preferred_username"
        };
    });

builder.Services.AddAuthorization();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7001; 
});

var app = builder.Build();

app.UseHttpsRedirection();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Ocelot
app.UseRouting();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.UseOcelot();

app.Run();
