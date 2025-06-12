using Microsoft.EntityFrameworkCore;
using InvestNaijaAuth.Data;
using Serilog;
using Serilog.Exceptions;
using Swashbuckle.AspNetCore;
using InvestNaijaAuth.Mappings;
using InvestNaijaAuth.MiddleWare;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InvestNaijaAuth.Servicies;

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

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

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

    builder.Host.UseSerilog();

    builder.Services.AddAutoMapper(typeof(MappingProfile));

    builder.Services.AddScoped<ITokenService, TokenService>();

    builder.Services.AddScoped<AzureBlobService>();

    builder.Services.AddScoped<IWalletService, WalletService>();


    var app = builder.Build();

    app.UseMiddleware<TraceIdEnricherMiddleWare>();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

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
