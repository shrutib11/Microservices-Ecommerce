using FluentValidation;
using FluentValidation.AspNetCore;
using Microservices.Shared;
using Microservices.Shared.Protos;
using Microsoft.EntityFrameworkCore;
using ProductService.API.GrpcServices;
using ProductService.Application.Interfaces;
using ProductService.Application.Validators;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure;
using ProductService.Infrastructure.Repositories;


AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("ProductDb");
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(conn!));

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<ProductDtoValidator>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc();
builder.Services.AddSwaggerGen();


builder.Services.AddGrpcClient<Category.CategoryClient>(o =>
{
    o.Address = new Uri("http://localhost:5003"); 
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
app.MapGrpcService<ProductGrpcService>();

app.Run();
