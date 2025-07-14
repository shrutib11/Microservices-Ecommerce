using CategoryService.API.GrpcServices;
using CategoryService.Application.Interfaces;
using CategoryService.Application.Mappings;
using CategoryService.Application.Validators;
using CategoryService.Domain.Interfaces;
using CategoryService.Infrastructure;
using CategoryService.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microservices.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var conn = builder.Configuration.GetConnectionString("CategoryServiceDbConnection");
builder.Services.AddDbContext<CategoryServiceDbContext>(options => options.UseNpgsql(conn));

builder.Services.AddAutoMapper(typeof(CategoryProfile));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService.Application.Services.CategoryService>();

builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<CategoryDtoValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseMiddleware<ErrorHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<CategoryGrpcService>();

app.Run();
