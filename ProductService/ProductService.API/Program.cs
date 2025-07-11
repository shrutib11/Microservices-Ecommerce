using Microservices.Shared;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure;
using ProductService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("ProductDb");
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(conn!));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<ICategoryServiceProxy, CategoryServiceProxy>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7000"); // API Gateway URL
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
