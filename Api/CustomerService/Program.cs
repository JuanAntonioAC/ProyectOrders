using CustomerService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using Shared.Repositories.Interfaces;
using NLog.Web;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using CustomerService.Validator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

try
{
    var builder = WebApplication.CreateBuilder(args);
    //Nlog 
    builder.Host.UseNLog();
    // Add services to the container.

    builder.Services.AddControllers();
    // Build Data Base 
    builder.Services.AddDbContext<CustomerDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConecction")));
    //Rabbitmq Connection
    builder.Services.AddSingleton(new ConnectionFactory
    {
        HostName = "localhost",
        DispatchConsumersAsync = true
    });
    builder.Services.AddHostedService<CustomerValidationConsumer>();
    builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    //Check Authentication token
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
    builder.Services.AddSwaggerGen();

    //Add Authorization Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = " 'Bearer '",
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
    logger.Error(ex, "Error starting customer service");

}
finally
{
    NLog.LogManager.Shutdown();
}

