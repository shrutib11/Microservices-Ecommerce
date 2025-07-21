using CartService.Application.Interfaces;
using CartService.Application.Mappings;
using CartService.Application.Validators;
using CartService.Domain.Interfaces;
using CartService.Infrastructure;
using CartService.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microservices.Shared;
using Microservices.Shared.Protos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var conn = builder.Configuration.GetConnectionString("CheckoutDb");
builder.Services.AddDbContext<CartServiceDbContext>(options =>
    options.UseNpgsql(conn!));

builder.Services.AddAutoMapper(typeof(CartProfile));
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<ICartService, CartService.Application.Services.CartService>();
builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<CartDtoValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpcClient<User.UserClient>(o =>
{
    o.Address = new Uri("http://localhost:5006"); 
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
