using Microsoft.EntityFrameworkCore;
using ProductServices.Data;
using Shared.Repositories.Interfaces;
using Shared.Repositories;
using NLog.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using ProductServices.Validator;
using RabbitMQ.Client;
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseNLog();
    // Add services to the container.

    builder.Services.AddControllers();
    //Add rabbit conection
    builder.Services.AddSingleton(new ConnectionFactory
    {
        HostName = "localhost",
        DispatchConsumersAsync = true
    });
    //reposit and validation injection
    builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
    builder.Services.AddHostedService<ProductStockValidatorConsumer>();

    //Bd Conection
    builder.Services.AddDbContext<ProductDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConecction")));

 
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    // Autorizacion token swagger
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

    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingresa tu token JWT en este formato: Bearer {tu_token}"
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



    // Autorizacion token swagger

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    var logger = NLog.LogManager.GetCurrentClassLogger();
    logger.Error(ex, "Error starting product service");

}
finally
{
    NLog.LogManager.Shutdown();
}
