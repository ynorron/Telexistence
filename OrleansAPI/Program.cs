using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Telexistence.OrleansAPI.Hubs;
using Telexistence.OrleansAPI.Interfaces;
using Telexistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddSingleton<ICommandRepository, MongoCommandRepository>();
builder.Services.AddScoped<ICommandService, CommandService>();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            RoleClaimType = ClaimTypes.Role,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options => {
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Telexistence API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.UseMongoDBClient(builder.Configuration["Mongo:ConnectionString"]); 
    siloBuilder.AddMongoDBGrainStorage("Mongo", options =>
    {
        options.DatabaseName = "telexistence";
        // options.CreateShardKeyForCosmos = false; // optional, for CosmosDB
    });
    siloBuilder.AddMemoryGrainStorage("RobotStream");
    siloBuilder.AddMemoryStreams("RobotStream");
});

builder.Services.AddSignalR();

var app = builder.Build();
app.MapHealthChecks("/healthz");
app.MapHub<RobotControlHub>("/robotControlHub");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            error = "An unexpected error occurred.",
            detail = exception?.Message
        }));
    });
});

app.Run();
