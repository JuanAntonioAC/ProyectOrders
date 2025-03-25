using Microsoft.EntityFrameworkCore;
using OrderServices.Data;
using Shared.Repositories;
using Shared.Repositories.Interfaces;
using NLog.Web;
using OrderServices.Mappers;
using Microsoft.OpenApi.Models;
using OrderServices.Stocks.Interfaces;
using OrderServices.Stocks;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


try
{ 
var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseNLog();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConecction")));
//Add rabbit conection
builder.Services.AddSingleton(new ConnectionFactory
    {
        HostName = "localhost", 
        DispatchConsumersAsync = true 
    });
//Repositories and validators
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped(typeof(ICustomerValidatorSender), typeof(CustomerValidatorSender));
builder.Services.AddScoped(typeof(IProductStockValidatorSender), typeof(ProductStockValidatorSender));
//Check Authentication token
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(op =>
    {
        op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


    }).AddJwtBearer(op =>
    {
        op.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

    });
//
builder.Services.AddAutoMapper(typeof(OrderMapper));
// Autorizacion token swagger
builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Ingrese el token JWT con el prefijo 'Bearer '",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    });
var app = builder.Build();


    

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();



    app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    var logger = NLog.LogManager.GetCurrentClassLogger();
    logger.Error(ex, "Error starting customer order");

}
finally
{
    NLog.LogManager.Shutdown();
}