using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RESTful_API.Models;
using RESTful_API.Service;
using Stripe;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Stripe Payment
var stripeKey = builder.Configuration["Stripe:SecretKey"]; // Add this to appsettings.json
if (string.IsNullOrEmpty(stripeKey))
{
    throw new ArgumentNullException("Stripe:SecretKey is missing in appsettings.json");
}

StripeConfiguration.ApiKey = stripeKey;


// Swagger OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Swagger OpenAPI Documentation
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ISIWebAPI",
        Version = "v1",
        Description = "ISI JWT Web API - Ecommerce Store com documentacao OpenAPI JWT",
        Contact = new OpenApiContact
        {
            Name = "Integracao de Sistemas de Informacao 2024/25",
            Email = "a23502@alunos.ipca.pt",
            Url = new Uri("https://www.ipca.pt"),
        },
    });

    // Enabling JWT Realted documentation

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer'[space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
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
            new string[]{}
        }
    });

    var filePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "RESTful-API.xml");
    c.IncludeXmlComments(filePath);
});


// JWT Token Related

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new ArgumentNullException(nameof(jwtKey), "Jwt:Key is not configured in appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UserService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISIWebAPI"));
}

app.UseHttpsRedirection();

app.UseAuthentication();    // JWT

app.UseAuthorization();

app.MapControllers();

app.Run();
