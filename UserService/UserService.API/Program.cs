using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microservices.Shared;
using Microservices.Shared.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserService.API.GrpcServices;
using UserService.API.Validators;
using UserService.Application.Interfaces;
using UserService.Application.Mappings;
using UserService.Domain.Interfaces;
using UserService.Infrastructure;
using UserService.Infrastructure.Repositories;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var conn = builder.Configuration.GetConnectionString("SupportDeskConnection");
builder.Services.AddDbContext<UserServiceDbContext>(options => options.UseNpgsql(conn));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService.Application.Services.UserService>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(UserProfile).Assembly);

builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "User MicroService", Version = "v1" });
    });

builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();
builder.Services.AddGrpc();
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 4000;
});
var app = builder.Build();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User MicroService");
    });

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapGrpcService<UserGrpcService>();

app.Run();