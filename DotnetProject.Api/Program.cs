using DotnetProject.Application.Extensions;
using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using DotnetProject.Core.ServiceClient;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.Tracing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var projectName = builder.Configuration["ProjectName"] ?? "DotnetProject.Api";
ActivitySourceProvider.Source = new ActivitySource(projectName);
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy",
                      builder =>
                      {
                          builder.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowAnyOrigin();
                      });
});

builder.Services.AddHttpClient();

builder.Services.AddDbContext<PostgresqlApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddInfrastructureServices(typeof(PostgresqlApplicationDbContext).Assembly);
builder.Services.AddApplicationServices(typeof(ApplicationServiceExtensions).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(PostgresqlApplicationDbContext).Assembly);
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ExternalServiceClient>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi(); 

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(projectName)) // เพิ่ม attribute service.name ให้กับทุกๆ Span 
    .WithTracing(tracerProviderBuilder => tracerProviderBuilder
        .AddSource(projectName) // กำหนดชื่อสำหรับ Manual Instrumentation
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            // 🔥 บังคับให้เริ่ม trace ใหม่ถ้าไม่มี parent
            options.Filter = (httpContext) => true;
        }) // เก็บ Trace จาก HTTP Request/Response อัตโนมัติ
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
            // 🔥 Propagate trace headers
            options.FilterHttpRequestMessage = (httpRequestMessage) => true;
        }) // เก็บ Trace จาก HttpClient ที่เรียกออกไปข้างนอกอัตโนมัติ
        .AddNpgsql() // Auto trace Npgsql 
        .AddOtlpExporter()
        .AddConsoleExporter() // เพิ่มบรรทัดนี้
    ).WithMetrics(metrics =>
    {
        //Setup Metrics(ถ้าต้องการใช้ตาม config PROMETHEUS_METRICS_PORT)
        metrics.AddAspNetCoreInstrumentation();
        // ถ้า OTEL_METRICS_EXPORTER: "none" ส่วนนี้อาจจะไม่ส่งออกไป OTLP แต่ถ้าใช้ Prometheus:
        // metrics.AddPrometheusExporter(); 
    }) 
    .WithLogging(logging =>
    {
        // ✅ เพิ่ม Logging ให้ส่งไป OTLP
        logging.AddOtlpExporter();
        logging.AddConsoleExporter(); // optional
    });

builder.Services.AddFastEndpoints(options =>
{
    options.Assemblies = [
        typeof(Program).Assembly,
        typeof(ApplicationServiceExtensions).Assembly,
        typeof(GetUserProjectQuery).Assembly
    ];
    options.IncludeAbstractValidators = true;
});
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
var logOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj} {Properties} | TraceId: {TraceId} SpanId: {SpanId}{NewLine}{Exception}";

// ===== Configure Serilog =====
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithSpan() // เพิ่ม TraceId และ SpanId เข้าไปใน Log
    .Enrich.WithProperty("service.name", projectName)
    .Enrich.WithProperty("deployment.environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties} | TraceId: {TraceId} SpanId: {SpanId}{NewLine}{Exception}"
    )
    // ✅ ไฟล์สำหรับ Debug logs
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information || evt.Level == LogEventLevel.Warning)
        .WriteTo.File(
            "logs/info/log-info-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: logOutputTemplate,
            retainedFileCountLimit: 30 // เก็บ 7 วัน
        )
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Level >= LogEventLevel.Error)
        .WriteTo.File(
            "logs/error/log-error-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: logOutputTemplate,
            retainedFileCountLimit: 90 // เก็บ 90 วัน (error สำคัญกว่า)
        )
    )
    .CreateLogger();

// Bridge Serilog to Microsoft.Extensions.Logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
    loggingBuilder.AddOpenTelemetry(); // ✅ เชื่อมต่อกับ OpenTelemetry
});
builder.Host.UseSerilog();

builder.Services.AddHealthChecks();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();


app.UseRouting();

app.MapHealthChecks("/health");

app.UseAuthorization();

app.UseFastEndpoints(config =>
{
    config.Errors.UseProblemDetails();
    config.Endpoints.ShortNames = true;
});
app.UseSerilogRequestLogging();

app.MapControllers();


await app.RunAsync();