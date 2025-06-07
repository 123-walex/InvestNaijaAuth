using Microsoft.EntityFrameworkCore;
using InvestNaijaAuth.Data;
using Serilog;
using Serilog.Exceptions;
using Swashbuckle.AspNetCore;
using InvestNaijaAuth.Mappings;
using InvestNaijaAuth.MiddleWare;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<InvestNaijaDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Host.UseSerilog();

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

app.UseMiddleware<TraceIdEnricherMiddleWare>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
