using Microsoft.EntityFrameworkCore;
using InvestNaijaAuth.Data;
using Serilog;
using Serilog.Exceptions;
using InvestNaijaAuth.Mappings;
using InvestNaijaAuth.MiddleWare;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InvestNaijaAuth.Servicies;
using InvestNaijaAuth.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithEnvironmentName()   
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .WriteTo.File("InvestNaijaLogs.txt", rollingInterval: RollingInterval.Infinite, shared: true)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [TraceId: {TraceId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add SPA services
    builder.Services.AddSpaStaticFiles(configuration =>
    {
        configuration.RootPath = "InvestNaija/out";
    });

    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddSerilog(dispose: true);
    });

    builder.Services.AddDbContext<InvestNaijaDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.Configure<AzureBlobSettings>(
        builder.Configuration.GetSection("AzureBlobSettings"));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration.GetValue<String>("JwtSettings:AccessToken")!)
                ),
                ValidateIssuerSigningKey = true
            };
        });

    builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy =>
            {
                policy.WithOrigins(
                    "https://invest-naija.vercel.app", // Production
                    "http://localhost:3000",           // Local Development
                    "https://localhost:3000"            // Local Development HTTPS
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
    });

    builder.Host.UseSerilog();
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "InvestNaija API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer' followed by your JWT token in the text box below.\r\n\r\nExample: \"Bearer abcde12345\"",
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
                Array.Empty<string>()
            }
        });
    });

    // Register services
    builder.Services.AddHttpClient<NGXScraperService>();
    builder.Services.AddScoped<IStockService, StockService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<AzureBlobService>();
    builder.Services.AddScoped<IWalletService, WalletService>();
    builder.Services.AddScoped<IPortfolioService, PortfolioService>();
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    app.UseMiddleware<TraceIdEnricherMiddleWare>();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    
    if (!app.Environment.IsDevelopment())
    {
        app.UseSpaStaticFiles();
    }

    app.UseCors("AllowFrontend");

    // Add logging middleware
    app.Use(async (context, next) =>
    {
        var traceId = context.TraceIdentifier;
        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name
            : "Anonymous";

        using (Serilog.Context.LogContext.PushProperty("TraceId", traceId))
        using (Serilog.Context.LogContext.PushProperty("User", userId))
        {
            await next();
        }
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Configure SPA
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "InvestNaija";

        if (app.Environment.IsDevelopment())
        {
            spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
