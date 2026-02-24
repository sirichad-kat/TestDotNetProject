# DotnetProject -- Step-by-Step Coding Guide

> คู่มือปฏิบัติสำหรับเขียน feature ใหม่แบบทำตามได้ทันที
> อ้างอิงโค้ดจริงในโปรเจค -- ทุกตัวอย่างคือไฟล์ที่มีอยู่จริง
> สำหรับแนวคิดและสถาปัตยกรรม ดูที่ [01_PROJECT_OVERVIEW.md](./01_PROJECT_OVERVIEW.md)

---

## สารบัญ

- [Part A -- Program.cs: การ Configure ค่าต่างๆ](#part-a----programcs-การ-configure-ค่าต่างๆ)
- [Part B -- OpenTelemetry: Tracing, Metrics, Logging](#part-b----opentelemetry-tracing-metrics-logging)
- [Part C -- FastEndpoints: วิธีใช้งาน GET / POST / PUT / DELETE](#part-c----fastendpoints-วิธีใช้งาน-get--post--put--delete)
- [Part D -- ToApiResponse: แปลง Result เป็น HTTP Response](#part-d----toapiresponse-แปลง-result-เป็น-http-response)
- [Part E -- Checklist สำหรับ Feature ใหม่](#part-e----checklist-สำหรับ-feature-ใหม่)
- [Part F -- หลักการตั้งชื่อ ViewReader vs Repository](#part-f----หลักการตั้งชื่อ-viewreader-vs-repository)
- [Part G -- เขียน Query Feature (อ่านข้อมูล)](#part-g----เขียน-query-feature-อ่านข้อมูล)
- [Part H -- เขียน Command Feature (เขียนข้อมูล)](#part-h----เขียน-command-feature-เขียนข้อมูล)
- [Part I -- Serilog: Logger Setup และวิธีใช้งาน](#part-i----serilog-logger-setup-และวิธีใช้งาน)
- [Part J -- FeedCommonLib: วิธีใช้งาน Shared Library](#part-j----feedcommonlib-วิธีใช้งาน-shared-library)
- [Part K -- IntegrationTestLib: วิธีเขียน Integration Test](#part-k----integrationtestlib-วิธีเขียน-integration-test)
- [Part L -- Unit Test: วิธีเขียน Unit Test](#part-l----unit-test-วิธีเขียน-unit-test)

---

## Part A -- Program.cs: การ Configure ค่าต่างๆ

> อ้างอิง: `DotnetProject.Api/Program.cs`

### ภาพรวม Program.cs

`Program.cs` แบ่งเป็น 2 ส่วนหลัก:

```
  1. Service Registration  (builder.Services.Add...)
  2. Middleware Pipeline    (app.Use... / app.Map...)
```

### ส่วนที่ 1 -- Service Registration

#### 1a. Database (EF Core + PostgreSQL)

```csharp
builder.Services.AddDbContext<PostgresqlApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// บอก Dapper ให้ map column ที่มี underscore (snake_case) เป็น PascalCase อัตโนมัติ
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DOTNET_DATA_SHADOW;Username=dotnet;Password=postgres"
  },
  "ProjectName": "DotnetProject.Api"
}
```

#### 1b. Auto-register DI (Infrastructure + Application)

```csharp
// scan Infrastructure assembly --> ลงทะเบียน Repository, ViewReader
builder.Services.AddInfrastructureServices(typeof(PostgresqlApplicationDbContext).Assembly);

// scan Application assembly --> ลงทะเบียน Handler, Validator
builder.Services.AddApplicationServices(typeof(ApplicationServiceExtensions).Assembly);
```

> ไม่ต้อง register DI เอง -- ทุกอย่างถูก scan อัตโนมัติ

#### 1c. FastEndpoints

```csharp
builder.Services.AddFastEndpoints(options =>
{
    options.Assemblies = [
        typeof(Program).Assembly,                       // Api layer
        typeof(ApplicationServiceExtensions).Assembly,  // Application layer
        typeof(GetUserProjectQuery).Assembly            // Application layer (queries)
    ];
    options.IncludeAbstractValidators = true;  // รองรับ TracedValidator (abstract)
});
```

#### 1d. OpenTelemetry (ดูรายละเอียดใน [Part B](#part-b----opentelemetry-tracing-metrics-logging))

#### 1e. Serilog (ดูรายละเอียดใน [Part I](#part-i----serilog-logger-setup-และวิธีใช้งาน))

### ส่วนที่ 2 -- Middleware Pipeline

**ลำดับสำคัญ** -- ต้องเรียงตามนี้:

```csharp
var app = builder.Build();

// 1. Swagger
app.UseSwagger();
app.UseSwaggerUI();

// 2. Routing
app.UseRouting();

// 3. Health Check
app.MapHealthChecks("/health");

// 4. Authorization
app.UseAuthorization();

// 5. FastEndpoints
app.UseFastEndpoints(config =>
{
    config.Errors.UseProblemDetails();   // error format เป็น RFC 7807
    config.Endpoints.ShortNames = true;  // Swagger แสดงชื่อสั้น
});

// 6. Serilog Request Logging
app.UseSerilogRequestLogging();

// 7. MVC Controllers (ถ้าต้องใช้ควบคู่)
app.MapControllers();

await app.RunAsync();
```

### NuGet Packages ที่ต้องติดตั้ง (Api project)

| Package | Version | หน้าที่ |
|---------|---------|---------|
| `FastEndpoints` | 7.2.0 | Endpoint framework |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.0 | EF Core + PostgreSQL |
| `Npgsql.OpenTelemetry` | 10.0.1 | Auto-trace SQL queries |
| `OpenTelemetry.Extensions.Hosting` | 1.15.0 | OpenTelemetry DI |
| `OpenTelemetry.Instrumentation.AspNetCore` | 1.15.0 | Auto-trace HTTP |
| `OpenTelemetry.Instrumentation.Http` | 1.15.0 | Auto-trace HttpClient |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | 1.15.0 | OTLP exporter |
| `OpenTelemetry.Exporter.Console` | 1.15.0 | Console exporter (dev) |
| `Serilog.AspNetCore` | 10.0.0 | Serilog integration |
| `Serilog.Enrichers.Span` | 3.1.0 | เพิ่ม TraceId/SpanId ใน log |
| `Serilog.Sinks.File` | 7.0.0 | เขียน log ลงไฟล์ |
| `Serilog.Sinks.Console` | 6.1.1 | เขียน log ลง console |
| `FluentValidation` | 12.1.1 | Input validation |
| `Scrutor` | 7.0.0 | Assembly scanning (DI) |
| `Swashbuckle.AspNetCore` | 10.1.0 | Swagger UI |

---

## Part B -- OpenTelemetry: Tracing, Metrics, Logging

> อ้างอิง: `Program.cs`
> โปรเจคนี้ใช้ OpenTelemetry **3 สาย**: Tracing + Metrics + Logging

### ภาพรวม

```
                    OpenTelemetry
                    /     |      \
              Tracing   Metrics   Logging
                |         |         |
           OTLP Exporter  ASP.NET   OTLP Exporter
           Console Exp.   Core      Console Exp.
```

### Step 1 -- ตั้งค่า ActivitySource

```csharp
// Program.cs -- ก่อน service registration
var projectName = builder.Configuration["ProjectName"] ?? "DotnetProject.Api";
ActivitySourceProvider.Source = new ActivitySource(projectName);
```

`ActivitySourceProvider.Source` ถูกใช้ทั่วทั้งโปรเจค:

- `InstrumentedResultExtensions` -- สร้าง span ใน ROP chain
- `TracedValidator` -- สร้าง span ใน validation
- Manual instrumentation ใน Controller/Service

### Step 2 -- Configure OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(projectName))

    // === TRACING ===
    .WithTracing(tracing => tracing
        .AddSource(projectName)                    // Manual Instrumentation
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = (httpContext) => true;
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
            options.FilterHttpRequestMessage = (msg) => true;
        })
        .AddNpgsql()                               // Auto trace SQL queries
        .AddOtlpExporter()
        .AddConsoleExporter()
    )

    // === METRICS ===
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
    })

    // === LOGGING ===
    .WithLogging(logging =>
    {
        logging.AddOtlpExporter();
        logging.AddConsoleExporter();
    });
```

### Auto Instrumentation ที่ได้

| Instrumentation | สิ่งที่ trace อัตโนมัติ |
|-----------------|----------------------|
| `AddAspNetCoreInstrumentation` | ทุก HTTP request เข้า -- method, path, status code, duration |
| `AddHttpClientInstrumentation` | ทุก HttpClient call ออก -- URL, status, duration |
| `AddNpgsql` | ทุก SQL query -- query text, duration, database |

### Manual Instrumentation -- ROP Chain

ทุก step ใน ROP chain สร้าง span อัตโนมัติผ่าน `InstrumentedResultExtensions`:

```csharp
return await InstrumentedResultExtensions
    .BeginTracingAsync(...)    // [Span] GetNextValueAsync
    .Then(...)                 // [Span] CreateCollab
    .ThenAsync(...)            // [Span] SaveGiveStar
    .Map(...);                 // [Span] Mapping GiveStarResultDto
```

**ตัวอย่าง Trace ที่ได้ (ดูใน Jaeger/Grafana):**

```
Trace: POST /api/collaboration/givestar
  |
  +-- [Span] HTTP POST /api/collaboration/givestar     (auto: ASP.NET Core)
       |
       +-- [Span] GetNextValueAsync                     (manual: ROP chain)
       |    +-- [Span] SELECT nextval(...)              (auto: Npgsql)
       |
       +-- [Span] CreateCollab                          (manual: ROP chain)
       |
       +-- [Span] SaveGiveStar                          (manual: ROP chain)
       |    +-- [Span] INSERT INTO axons_collab...      (auto: Npgsql)
       |
       +-- [Span] Mapping GiveStarResultDto             (manual: ROP chain)
```

### Manual Instrumentation -- สร้าง Span เอง (กรณีพิเศษ)

```csharp
using var activity = ActivitySourceProvider.Source.StartActivity("MyCustomOperation");
activity?.SetTag("input.type", "MyType");

// ... ทำงาน ...

activity?.SetStatus(ActivityStatusCode.Ok);
// หรือ
activity?.SetStatus(ActivityStatusCode.Error, "Something went wrong");
```

### OTLP Configuration (environment variables)

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_SERVICE_NAME=DotnetProject.Api
```

---

## Part C -- FastEndpoints: วิธีใช้งาน GET / POST / PUT / DELETE

> อ้างอิง: `DotnetProject.Api/Endpoints/` folder
> NuGet: `FastEndpoints 7.2.0`

### แนวคิด

FastEndpoints ใช้แนวคิด **1 Endpoint = 1 Class** แทน Controller ที่รวมหลาย action

> **เทียบกับ MVC:** แทนที่จะมี `CollabController` ที่มี `GiveStar()`, `GetStars()`, `DeleteStar()` รวมกัน --
> โปรเจคนี้แยกเป็น `GiveStarEndpoint`, `GetStarsEndpoint`, `DeleteStarEndpoint` แต่ละ class

### เลือก Base Class

| Base Class | ใช้เมื่อ | ตัวอย่าง |
|------------|---------|---------|
| `Endpoint<TRequest, TResponse>` | มีทั้ง request + response | **ใช้เป็นหลัก** |
| `EndpointWithoutRequest<TResponse>` | ไม่มี request | `GET /api/health` |
| `Endpoint<TRequest>` | มี request แต่ไม่มี response body | `DELETE /api/items/{id}` |
| `EndpointWithoutRequest` | ไม่มีทั้ง request และ response | `POST /api/cache/clear` |

### โครงสร้างพื้นฐานของ Endpoint

```csharp
public class MyEndpoint : Endpoint<MyRequest, MyResponse>
{
    // 1. Configure -- กำหนด route, HTTP method, auth
    public override void Configure()
    {
        Get("/api/my-resource");       // หรือ Post, Put, Delete, Patch
        AllowAnonymous();              // หรือใช้ auth policy
    }

    // 2. HandleAsync -- logic 3 บรรทัด: Resolve -> Handle -> Response
    public override async Task HandleAsync(MyRequest req, CancellationToken ct)
    {
        var handler = Resolve<MyHandler>();
        var result = await handler.Handle(req, ct);
        await result.ToApiResponse(HttpContext, Logger);
    }
}
```

### GET Endpoint (โค้ดจริง)

> อ้างอิง: `DotnetProject.Api/Endpoints/UserInfo/GetUserProjectEndpoint.cs`

```csharp
using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using DotnetProject.Core.Features.UserInfo;
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.EndpointExtension;

namespace DotnetProject.Api.Endpoints.UserInfo
{
    public class GetUserProjectEndpoint : Endpoint<GetUserProjectQuery, IEnumerable<ProjectRecord>>
    {
        public override void Configure()
        {
            Get("/api/userinfo/project");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetUserProjectQuery req, CancellationToken ct)
        {
            var handler = Resolve<GetUserProjectHandler>();
            var result = await handler.Handle(req, ct);
            await result.ToApiResponse(HttpContext, Logger);
        }
    }
}
```

**GET -- การรับ parameter:**
- Query string: `GET /api/userinfo/project?userName=john`
- FastEndpoints จะ bind query string เข้า property `userName` ของ `GetUserProjectQuery` อัตโนมัติ

### POST Endpoint (โค้ดจริง)

> อ้างอิง: `DotnetProject.Api/Endpoints/Collaboration/GiveStarEndpoint.cs`

```csharp
using DotnetProject.Application.Features.Collaboration.Commands.GiveStar;
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.EndpointExtension;

namespace DotnetProject.Api.Endpoints.UserInfo
{
    public class GiveStarEndpoint : Endpoint<GiveStarCommand, GiveStarResultDto>
    {
        public override void Configure()
        {
            Post("/api/collaboration/givestar");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GiveStarCommand req, CancellationToken ct)
        {
            GiveStarHandler handler = Resolve<GiveStarHandler>();
            Result<GiveStarResultDto> result = await handler.Handle(req, ct);
            await result.ToApiResponse(HttpContext, Logger);
        }
    }
}
```

**POST -- การรับ parameter:**
- JSON body จะ bind เข้า `GiveStarCommand` record อัตโนมัติ

### PUT Endpoint (template)

```csharp
public class UpdateStarEndpoint : Endpoint<UpdateStarCommand, UpdateStarResultDto>
{
    public override void Configure()
    {
        Put("/api/collaboration/{id}");   // route parameter
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateStarCommand req, CancellationToken ct)
    {
        var handler = Resolve<UpdateStarHandler>();
        var result = await handler.Handle(req, ct);
        await result.ToApiResponse(HttpContext, Logger);
    }
}

// Command ต้องมี property ชื่อตรงกับ route parameter
public record UpdateStarCommand(int Id, string? Remark) : ICommandRequest<UpdateStarResultDto>;
```

### DELETE Endpoint (template)

```csharp
public class DeleteStarEndpoint : Endpoint<DeleteStarCommand>
{
    public override void Configure()
    {
        Delete("/api/collaboration/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteStarCommand req, CancellationToken ct)
    {
        var handler = Resolve<DeleteStarHandler>();
        var result = await handler.Handle(req, ct);
        await result.ToApiResponse(HttpContext, Logger);
    }
}
```

### Configure() -- ตัวเลือกที่ใช้บ่อย

```csharp
public override void Configure()
{
    // --- HTTP Method + Route ---
    Get("/api/resource");
    Post("/api/resource");
    Put("/api/resource/{id}");
    Delete("/api/resource/{id}");

    // --- Authentication ---
    AllowAnonymous();   // ไม่ต้อง login (ไม่ใส่ = ต้อง authenticate)

    // --- Swagger ---
    Tags("Collaboration");
    Summary(s =>
    {
        s.Summary = "Give a star";
        s.Description = "Give a star to another user";
    });
}
```

### สรุป Pattern ของ Endpoint

```
ทุก Endpoint มีโครงสร้างเดียวกัน:

  Configure()   -->  กำหนด route, method, auth
  HandleAsync()  -->  Resolve<Handler>()
                      handler.Handle(req, ct)
                      result.ToApiResponse(HttpContext, Logger)
```

---

## Part D -- ToApiResponse: แปลง Result เป็น HTTP Response

> อ้างอิง: `FeedCommonLib.Application.Abstractions/EndpointExtension/ApiResponseExtension.cs`

### แนวคิด

ทุก Handler return `Result<T>` -- แต่ HTTP ต้องการ status code + JSON body
`ToApiResponse` ทำหน้าที่เป็นสะพานเชื่อมระหว่าง **Result Pattern** กับ **HTTP Response** โดยอัตโนมัติ

```
Result<T>  -->  ToApiResponse()  -->  HTTP Response (ApiResponse<T>)
```

### วิธีใช้งานใน Endpoint

```csharp
// ใช้ทุก Endpoint -- เรียกแค่บรรทัดเดียว
await result.ToApiResponse(HttpContext, Logger);
```

**Parameters:**

| Parameter | ค่า | คำอธิบาย |
|-----------|-----|---------|
| `HttpContext` | HTTP context | ดึง TraceId, ส่ง response |
| `Logger` | ILogger | log error อัตโนมัติเมื่อ failure |
| `successMessage` | (optional) | custom message เมื่อสำเร็จ |

### พฤติกรรมอัตโนมัติ

```
Result.IsSuccess == true
  --> HTTP 200 + ApiResponse<T>.Success(data, traceId)
  --> codeResult = "SUC200" (default)

Result.IsSuccess == false
  --> HTTP {error.HttpStatusCode} + ApiResponse<T>.Error(error, traceId)
  --> logger.Log(error) อัตโนมัติ
```

### ApiResponse&lt;T&gt; -- รูปแบบ JSON Response

**Success:**

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "traceId": "abc123def456...",
  "codeResult": "SUC200",
  "message": "Request completed successfully",
  "dataResult": { ... }
}
```

**Error:**

```json
{
  "statusCode": 400,
  "isSuccess": false,
  "traceId": "abc123def456...",
  "codeResult": "ERR301",
  "message": "You cannot give a star to yourself.",
  "dataResult": null
}
```

### TraceId -- ลำดับความสำคัญในการดึงค่า

```
1. Activity.Current?.TraceId   -- จาก OpenTelemetry (distributed tracing)
2. Header "X-Trace-Id"         -- จาก client ส่งมา
3. Guid.NewGuid()              -- generate ใหม่ถ้าไม่มี
```

### มี 2 overload

| Method | ใช้เมื่อ |
|--------|---------|
| `ToApiResponse<T>(Result<T>)` | Handler return ข้อมูล (Query, Command ที่มี DTO) |
| `ToApiResponse(Result)` | Handler ไม่ return ข้อมูล (Command แบบ void) |

### ตัวอย่างเต็ม

```csharp
// อ้างอิง: GiveStarEndpoint.cs
public override async Task HandleAsync(GiveStarCommand req, CancellationToken ct)
{
    GiveStarHandler handler = Resolve<GiveStarHandler>();
    Result<GiveStarResultDto> result = await handler.Handle(req, ct);

    // แค่บรรทัดเดียว -- จัดการ success/error ให้หมด
    await result.ToApiResponse(HttpContext, Logger);

    // ไม่ต้องเขียน if/else:
    // if (result.IsSuccess) await SendOkAsync(result.Value);
    // else await SendErrorsAsync();
}
```

### Success Code -- กำหนด HTTP Status Code สำหรับ Success

> อ้างอิง: `FeedCommonLib.Application.Abstractions/ResponseCodes/SuccessCode.cs`

#### แนวคิด

**Error Code** ใช้สำหรับกรณี failure (ERR001, ERR201, ...)
**Success Code** ใช้สำหรับกรณี success พร้อมกำหนด HTTP status code ที่เหมาะสม

```
Result.Success(data, SuccessCode)
  --> ToApiResponse()
  --> HTTP {SuccessCode.HttpStatusCode}
  --> JSON { "codeResult": "SUC200", "statusCode": 200 }
```

#### Success Code ที่มีอยู่

| Code | HTTP Status | DefaultMessage | ใช้เมื่อ |
|------|-------------|----------------|---------|
| `SuccessCodes.Ok` | 200 | Request completed successfully | Query (GET) หรือ Command ทั่วไป |
| `SuccessCodes.Created` | 201 | Resource created successfully | POST -- สร้างข้อมูลใหม่ |
| `SuccessCodes.NoContent` | 204 | Request completed with no content | DELETE หรือ Command ที่ไม่ return ข้อมูล |

#### วิธีการลงทะเบียน Success Code ใหม่

```csharp
// ใน SuccessCode.cs (FeedCommonLib)
public static readonly SuccessCode Accepted = Register(
    "SUC202",
    "Request accepted for processing",
    StatusCodes.Status202Accepted
);

public static readonly SuccessCode PartialContent = Register(
    "SUC206",
    "Partial content returned",
    StatusCodes.Status206PartialContent
);
```

#### วิธีใช้งานใน ROP Chain -- ส่ง Success Code ที่ท้าย Map

**Pattern:** ส่ง `SuccessCode` เป็นพารามิเตอร์ที่ 2 ของ `Result.Success()`

```csharp
// Query Handler -- ใช้ SuccessCodes.Ok (200)
public async Task<Result<IEnumerable<ProjectRecord>>> Handle(GetUserProjectQuery query, CancellationToken ct)
{
    return await InstrumentedResultExtensions
        .BeginTracingAsync(async () => await _userProjectViewReader.GetProjectsByUserNameAsync(query.userName, ct))
        .Map(res => Result<IEnumerable<ProjectRecord>>.Success(res, SuccessCodes.Ok));
        //                                                           ^^^^^^^^^^^^^^^^
        //                                           ส่ง SuccessCode ที่นี่
}
```

```csharp
// Command Handler (POST) -- ใช้ SuccessCodes.Created (201)
public async Task<Result<GiveStarResultDto>> Handle(GiveStarCommand command, CancellationToken ct)
{
    return await InstrumentedResultExtensions
        .BeginTracingAsync(() => _collabViewReader.GetNextValueAsync("axons_collab_id_seq", ct))
        .Then(newId => CollabFactory.CreateCollab(
            _logger,
            newId,
            command.Year,
            command.Sprint,
            command.GivenUser,
            command.GivenFullname,
            command.StarUser,
            command.StarFullname,
            command.SubTeam,
            command.Remark))
        .ThenAsync(async saveData => await _collabRepository.SaveGiveStar(saveData, ct))
        .Map(res => Result<GiveStarResultDto>.Success(
            new GiveStarResultDto { IsSuccess = true, Id = res.Id },
            SuccessCodes.Created));  // <-- POST = 201 Created
}
```

```csharp
// Command Handler (DELETE) -- ใช้ SuccessCodes.NoContent (204)
public async Task<Result> Handle(DeleteStarCommand command, CancellationToken ct)
{
    return await InstrumentedResultExtensions
        .BeginTracingAsync(() => _collabRepository.GetByIdAsync(command.Id, ct))
        .ThenAsync(async entity => await _collabRepository.DeleteAsync(entity, ct))
        .Map(() => Result.Success(SuccessCodes.NoContent));  // <-- DELETE = 204
}
```

#### ตารางแนะนำการใช้ Success Code

| HTTP Method | Operation | Success Code | HTTP Status |
|-------------|-----------|--------------|-------------|
| GET | Query ข้อมูล | `SuccessCodes.Ok` | 200 |
| POST | Create ข้อมูลใหม่ | `SuccessCodes.Created` | 201 |
| PUT | Update ข้อมูล | `SuccessCodes.Ok` | 200 |
| PATCH | Update บางส่วน | `SuccessCodes.Ok` | 200 |
| DELETE | ลบข้อมูล | `SuccessCodes.NoContent` | 204 |

#### JSON Response ที่ได้

**SuccessCodes.Ok (200):**

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "traceId": "...",
  "codeResult": "SUC200",
  "message": "Request completed successfully",
  "dataResult": { ... }
}
```

**SuccessCodes.Created (201):**

```json
{
  "statusCode": 201,
  "isSuccess": true,
  "traceId": "...",
  "codeResult": "SUC201",
  "message": "Resource created successfully",
  "dataResult": {
    "isSuccess": true,
    "id": 42
  }
}
```

**SuccessCodes.NoContent (204):**

```json
{
  "statusCode": 204,
  "isSuccess": true,
  "traceId": "...",
  "codeResult": "SUC204",
  "message": "Request completed with no content",
  "dataResult": null
}
```

#### สรุป

```
1. เลือก SuccessCode ให้เหมาะกับ HTTP method:
     GET    --> SuccessCodes.Ok
     POST   --> SuccessCodes.Created
     DELETE --> SuccessCodes.NoContent

2. ส่ง SuccessCode ที่ท้าย ROP Chain (ใน Map):
     .Map(res => Result.Success(data, SuccessCodes.Created))

3. ToApiResponse จะแปลง SuccessCode เป็น HTTP response อัตโนมัติ
```

---

## Part E -- Checklist สำหรับ Feature ใหม่

### Query Feature (GET -- อ่านข้อมูล)

| | Layer | ไฟล์ | คำอธิบาย |
|---|---|---|---|
| &#9744; | Core | `Features/{Name}/{Record}.cs` | result record/DTO |
| &#9744; | Core | `Features/{Name}/Abstractions/I{Name}ViewReader.cs` | interface |
| &#9744; | Application | `Features/{Name}/Queries/{Action}/{Action}Query.cs` | `IQueryRequest<T>` |
| &#9744; | Application | `Features/{Name}/Queries/{Action}/{Action}QueryValidator.cs` | `TracedValidator<T>` |
| &#9744; | Application | `Features/{Name}/Queries/{Action}/{Action}Handler.cs` | `IQueryRequestHandler<,>` |
| &#9744; | Infrastructure | `Queries/{Name}ViewReader.cs` | Dapper implementation |
| &#9744; | Api | `Endpoints/{Name}/{Action}Endpoint.cs` | FastEndpoints |

### Command Feature (POST/PUT/DELETE -- เขียนข้อมูล)

| | Layer | ไฟล์ | คำอธิบาย |
|---|---|---|---|
| &#9744; | Core | `Domain/Entities/{Entity}.cs` | entity (ถ้ายังไม่มี) |
| &#9744; | Core | `Shared/FeatureErrors.cs` | เพิ่ม Error |
| &#9744; | Core | `Features/{Name}/Abstractions/I{Name}Repository.cs` | CRUD interface |
| &#9744; | Core | `Features/{Name}/Abstractions/I{Name}ViewReader.cs` | complex query interface (ถ้ามี) |
| &#9744; | Core | `Features/{Name}/Operations/{Name}Factory.cs` | domain validation + entity creation |
| &#9744; | Application | `Features/{Name}/Commands/{Action}/{Action}Command.cs` | `ICommandRequest<T>` |
| &#9744; | Application | `Features/{Name}/Commands/{Action}/{Action}CommandValidator.cs` | `AbstractValidator<T>` |
| &#9744; | Application | `Features/{Name}/Commands/{Action}/{Action}ResultDto.cs` | `BaseResponse` |
| &#9744; | Application | `Features/{Name}/Commands/{Action}/{Action}Handler.cs` | `ICommandRequestHandler<,>` + ROP chain |
| &#9744; | Infrastructure | `Repositories/{Name}Repository.cs` | EF Core implementation |
| &#9744; | Infrastructure | `Queries/{Name}ViewReader.cs` | Dapper implementation (ถ้ามี) |
| &#9744; | Api | `Endpoints/{Name}/{Action}Endpoint.cs` | FastEndpoints |

> DI ลงทะเบียนอัตโนมัติ -- ไม่ต้องเพิ่มโค้ดใน `Program.cs`

---

## Part F -- หลักการตั้งชื่อ ViewReader vs Repository

### Repository -- ลงท้ายด้วย `Repository`

| ใช้เมื่อ | เทคโนโลยี | Folder |
|---------|-----------|--------|
| Save / Update / Delete | **EF Core** | `Infrastructure.Postgresql/Repositories/` |
| GetById / GetList จากตารางเดียว | **EF Core** (LINQ) | `Infrastructure.Postgresql/Repositories/` |
| Return Entity | **EF Core** | `Infrastructure.Postgresql/Repositories/` |

```
ICollabRepository  -->  CollabRepository
  SaveGiveStar()          -- EF Core: AddAsync + SaveChangesAsync
  GetListStarReceive()    -- EF Core: LINQ query ตารางเดียว
  GetListStarGiven()      -- EF Core: LINQ query ตารางเดียว
```

### ViewReader -- ลงท้ายด้วย `ViewReader`

| ใช้เมื่อ | เทคโนโลยี | Folder |
|---------|-----------|--------|
| JOIN หลายตาราง | **Dapper** (raw SQL) | `Infrastructure.Postgresql/Queries/` |
| Aggregate / Window function | **Dapper** (raw SQL) | `Infrastructure.Postgresql/Queries/` |
| Database function / Sequence | **Dapper** (raw SQL) | `Infrastructure.Postgresql/Queries/` |
| Return DTO/Record (ไม่ใช่ Entity) | **Dapper** (raw SQL) | `Infrastructure.Postgresql/Queries/` |

```
IUserProjectViewReader  -->  UserProjectViewReader
  GetProjectsByUserNameAsync()  -- Dapper: JOIN axons_project + axons_member

ICollabViewReader       -->  CollabViewReader
  GetNextValueAsync()           -- Dapper: SELECT nextval(sequence)
```

### ถามตัวเอง

```
"ต้องเขียน raw SQL ไหม?"

  ไม่ (LINQ ได้ / CRUD)       -->  Repository   (EF Core)
  ใช่ (JOIN / aggregate)      -->  ViewReader   (Dapper)

"return อะไร?"

  Entity (เช่น AxonsCollab)               -->  Repository
  DTO/Record (เช่น ProjectRecord) / scalar -->  ViewReader
```

---

## Part G -- เขียน Query Feature (อ่านข้อมูล)

> ตัวอย่างอ้างอิง: **GetUserProject** -- ดึงรายชื่อโปรเจคของ user
> ใช้ **ViewReader** (Dapper) เพราะ query ต้อง JOIN หลายตาราง

### Step 1 -- สร้าง Record สำหรับ query result

**ที่อยู่ไฟล์:** `DotnetProject.Core/Features/{FeatureName}/{RecordName}.cs`

Record คือ model สำหรับรับผลลัพธ์จาก SQL query -- property ต้องตรงกับ column ที่ SELECT

```csharp
// ไฟล์: DotnetProject.Core/Features/UserInfo/ProjectRecord.cs

namespace DotnetProject.Core.Features.UserInfo
{
    public record ProjectRecord
    {
        public int Year { get; set; }
        public string ProjectCode { get; set; } = null!;
        public string? ProjectName { get; set; }
        public string? SubTeam { get; set; }
        public DateTime? ProjectCreatedAt { get; set; }
        public long MemberId { get; set; }
        public string? Username { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
        public string? SquadName { get; set; }
        public char? Status { get; set; }
    }
}
```

> **Tip:** ใช้ `record` แทน `class` เพราะเหมาะกับ read-only query result
> Dapper จะ map column snake_case เป็น PascalCase อัตโนมัติ (ตั้งค่าไว้ใน Program.cs)

---

### Step 2 -- สร้าง Interface ViewReader ใน Core

**ที่อยู่ไฟล์:** `DotnetProject.Core/Features/{FeatureName}/Abstractions/I{Name}ViewReader.cs`

```csharp
// ไฟล์: DotnetProject.Core/Features/UserInfo/Abstractions/IUserProjectViewReader.cs

using FeedCommonLib.Application.Abstractions.Primitives;

namespace DotnetProject.Core.Features.UserInfo.Abstractions
{
    public interface IUserProjectViewReader
    {
        Task<Result<IEnumerable<ProjectRecord>>> GetProjectsByUserNameAsync(
            string? userName,
            CancellationToken ct = default);
    }
}
```

**กฎการตั้งชื่อ:**

- ชื่อ interface: `I` + `{Name}` + `ViewReader`
- Return type ต้องเป็น `Task<Result<T>>` **เสมอ**

---

### Step 3 -- สร้าง Query (request model)

**ที่อยู่ไฟล์:** `DotnetProject.Application/Features/{FeatureName}/Queries/{ActionName}/{ActionName}Query.cs`

```csharp
// ไฟล์: DotnetProject.Application/Features/UserInfo/Queries/GetUserProject/GetUserProjectQuery.cs

using DotnetProject.Core.Features.UserInfo;
using FeedCommonLib.Application.Abstractions.Messaging;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserProject
{
    public record GetUserProjectQuery(string userName) : IQueryRequest<IEnumerable<ProjectRecord>>;
}
```

**กฎ:**

- ต้อง implement `IQueryRequest<TResponse>` (ไม่ใช่ `ICommandRequest`)
- `TResponse` ต้องตรงกับ return type ของ Handler

---

### Step 4 -- สร้าง Validator

**ที่อยู่ไฟล์:** `DotnetProject.Application/Features/{FeatureName}/Queries/{ActionName}/{ActionName}QueryValidator.cs`

ใช้ `TracedValidator<T>` (FluentValidation + OpenTelemetry) เพื่อตรวจ input ก่อนเข้า Handler

```csharp
// ไฟล์: DotnetProject.Application/Features/UserInfo/Queries/GetUserProject/GetUserProjectQueryValidator.cs

using DotnetProject.Application.Features.Common;
using FluentValidation;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserProject
{
    public class GetUserProjectQueryValidator : TracedValidator<GetUserProjectQuery>
    {
        public GetUserProjectQueryValidator()
        {
            RuleFor(x => x.userName)
                .NotNull().WithMessage("UserName cannot be null.")
                .NotEmpty().WithMessage("UserName is required.")
                .Must(userName => !string.IsNullOrWhiteSpace(userName))
                    .WithMessage("UserName cannot be empty or whitespace.");
        }
    }
}
```

**เลือก base class:**

- `TracedValidator<T>` -- สร้าง trace span อัตโนมัติ (แนะนำ)
- `AbstractValidator<T>` -- validator ปกติ (ไม่มี trace)

---

### Step 5 -- สร้าง Handler

**ที่อยู่ไฟล์:** `DotnetProject.Application/Features/{FeatureName}/Queries/{ActionName}/{ActionName}Handler.cs`

```csharp
// ไฟล์: DotnetProject.Application/Features/UserInfo/Queries/GetUserProject/GetUserProjectHandler.cs

using FeedCommonLib.Application.Abstractions.Messaging;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.Tracing;
using DotnetProject.Core.Features.UserInfo.Abstractions;
using DotnetProject.Core.Features.UserInfo;

namespace DotnetProject.Application.Features.UserInfo.Queries.GetUserProject
{
    public class GetUserProjectHandler : IQueryRequestHandler<GetUserProjectQuery, IEnumerable<ProjectRecord>>
    {
        private readonly IUserProjectViewReader _userProjectViewReader;

        public GetUserProjectHandler(IUserProjectViewReader userProjectViewReader)
        {
            _userProjectViewReader = userProjectViewReader;
        }

        public async Task<Result<IEnumerable<ProjectRecord>>> Handle(
            GetUserProjectQuery query,
            CancellationToken ct)
        {
            return await InstrumentedResultExtensions
                .BeginTracingAsync(async () =>
                    await _userProjectViewReader.GetProjectsByUserNameAsync(query.userName, ct))
                .Map(res => Result<IEnumerable<ProjectRecord>>.Success(res, SuccessCodes.Ok));
        }
    }
}
```

**กฎ:**

- implement `IQueryRequestHandler<TQuery, TResponse>`
- Inject **interface** จาก Core (ไม่ใช่ concrete class จาก Infrastructure)
- เริ่ม chain ด้วย `InstrumentedResultExtensions.BeginTracingAsync()`

---

### Step 6 -- Implement ViewReader ใน Infrastructure

**ที่อยู่ไฟล์:** `DotnetProject.Infrastructure.Postgresql/Queries/{Name}ViewReader.cs`

ViewReader ใช้ **Dapper** สำหรับ raw SQL query ที่ซับซ้อน

```csharp
// ไฟล์: DotnetProject.Infrastructure.Postgresql/Queries/UserProjectViewReader.cs

using Dapper;
using DotnetProject.Core.Features.UserInfo;
using DotnetProject.Core.Features.UserInfo.Abstractions;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace DotnetProject.Infrastructure.Postgresql.Queries
{
    public class UserProjectViewReader : IUserProjectViewReader
    {
        readonly PostgresqlApplicationDbContext context;
        private readonly ILogger<UserProjectViewReader> _logger;

        public UserProjectViewReader(
            PostgresqlApplicationDbContext _context,
            IConfiguration configuration,
            ILogger<UserProjectViewReader> logger)
        {
            context = _context;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<ProjectRecord>>> GetProjectsByUserNameAsync(
            string? userName,
            CancellationToken ct = default)
        {
            try
            {
                const string sql = @"
                    SELECT
                        p.year,
                        p.project_code,
                        p.project_name,
                        p.sub_team,
                        m.member_id,
                        m.username,
                        m.full_name,
                        m.email,
                        m.role,
                        m.department,
                        m.squad_name
                    FROM axons_project p
                    INNER JOIN axons_member m ON p.sub_team = m.squad_name
                    WHERE m.username = @UserName";

                using (DbConnection connection = context.Database.GetDbConnection())
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();

                    IEnumerable<ProjectRecord> results =
                        await connection.QueryAsync<ProjectRecord>(sql, new { UserName = userName });

                    return Result<IEnumerable<ProjectRecord>>.Success(results);
                }
            }
            catch (Exception ex)
            {
                StdResponse error = StdResponse.FromException(Errors.Database, ex);
                _logger.Log(error,
                    context: nameof(GetProjectsByUserNameAsync),
                    customMessage: $"Error occurred while fetching projects for user {userName}");

                return Result<IEnumerable<ProjectRecord>>.Failure(error);
            }
        }
    }
}
```

**Pattern สำคัญ:**

- ดึง `DbConnection` จาก `context.Database.GetDbConnection()` เพื่อใช้ Dapper
- `try/catch` ครอบทั้งหมด -- return `Result.Failure` **แทน throw**
- ใช้ `StdResponse.FromException(Errors.Database, ex)` สำหรับ database error
- ใช้ `_logger.Log(error, ...)` สำหรับ structured logging

---

### Step 7 -- สร้าง Endpoint

**ที่อยู่ไฟล์:** `DotnetProject.Api/Endpoints/{FeatureName}/{ActionName}Endpoint.cs`

```csharp
// ไฟล์: DotnetProject.Api/Endpoints/UserInfo/GetUserProjectEndpoint.cs

using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using DotnetProject.Core.Features.UserInfo;
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.EndpointExtension;

namespace DotnetProject.Api.Endpoints.UserInfo
{
    public class GetUserProjectEndpoint : Endpoint<GetUserProjectQuery, IEnumerable<ProjectRecord>>
    {
        public override void Configure()
        {
            Get("/api/userinfo/project");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetUserProjectQuery req, CancellationToken ct)
        {
            var handler = Resolve<GetUserProjectHandler>();
            var result = await handler.Handle(req, ct);
            await result.ToApiResponse(HttpContext, Logger);
        }
    }
}
```

### Query Summary -- ไฟล์ทั้งหมดที่ต้องสร้าง

```
1. Core/Features/UserInfo/ProjectRecord.cs                           -- result record
2. Core/Features/UserInfo/Abstractions/IUserProjectViewReader.cs     -- interface
3. Application/Features/UserInfo/Queries/GetUserProject/
     GetUserProjectQuery.cs                                          -- request model
     GetUserProjectQueryValidator.cs                                 -- validation
     GetUserProjectHandler.cs                                        -- handler
4. Infrastructure.Postgresql/Queries/UserProjectViewReader.cs        -- Dapper impl
5. Api/Endpoints/UserInfo/GetUserProjectEndpoint.cs                  -- endpoint
```

---

## Part H -- เขียน Command Feature (เขียนข้อมูล)

> ตัวอย่างอ้างอิง: **GiveStar** -- บันทึกการให้ดาว
> ใช้ทั้ง **Repository** (EF Core) สำหรับ save และ **ViewReader** (Dapper) สำหรับ sequence

### Step 1 -- สร้าง Entity ใน Core (ถ้ายังไม่มี)

**ที่อยู่ไฟล์:** `DotnetProject.Core/Domain/Entities/{EntityName}.cs`

```csharp
// ไฟล์: DotnetProject.Core/Domain/Entities/AxonsCollab.cs

namespace DotnetProject.Core.Entities;

public partial class AxonsCollab
{
    public int Id { get; set; }
    public string StarUser { get; set; } = null!;
    public string? StarFullname { get; set; }
    public int? Sprint { get; set; }
    public int? Year { get; set; }
    public string? CategoryCode { get; set; }
    public string? Remark { get; set; }
    public string? GivenUser { get; set; }
    public string? GivenFullname { get; set; }
    public DateTime? GivenDate { get; set; }
    public string? SubTeam { get; set; }
}
```

> ถ้ามี entity แล้วไม่ต้องสร้างใหม่ -- ข้ามไป step ถัดไป

---

### Step 2 -- ลงทะเบียน Error

**ที่อยู่ไฟล์:** `DotnetProject.Core/Shared/FeatureErrors.cs`

```csharp
// ไฟล์: DotnetProject.Core/Shared/FeatureErrors.cs

using FeedCommonLib.Application.Abstractions.ResponseCodes;
using Microsoft.AspNetCore.Http;

namespace DotnetProject.Core.Shared
{
    public static class FeatureErrors
    {
        #region "Collaboration"

        public static readonly Error CannotGiveStarToSelf = Errors.Register(
            "ERR301",
            "You cannot give a star to yourself.",
            StatusCodes.Status400BadRequest
        );

        public static readonly Error StarAlreadyGiven = Errors.Register(
            "ERR302",
            "You have already given a star to this person this month.",
            StatusCodes.Status409Conflict
        );

        public static readonly Error InvalidStarData = Errors.Register(
            "ERR303",
            "The provided star data is invalid.",
            StatusCodes.Status400BadRequest
        );

        public static readonly Error GivenUserIsNull = Errors.Register(
            "ERR304",
            "Given User is null or whitespace.",
            StatusCodes.Status400BadRequest
        );

        public static readonly Error StarUserIsNull = Errors.Register(
            "ERR305",
            "Star User is null or whitespace.",
            StatusCodes.Status400BadRequest
        );

        #endregion
    }
}
```

**Error Code Convention:**

| ช่วง | ประเภท | ตัวอย่าง |
|---|---|---|
| `ERR0xx` | Domain (NotFound, Validation) | `ERR001`, `ERR002` |
| `ERR1xx` | Application (Auth, Permission) | `ERR101`, `ERR102` |
| `ERR2xx` | Infrastructure (Database, Timeout) | `ERR201`, `ERR204` |
| `ERR3xx` | Custom / Feature-specific | `ERR301`, `ERR302` |
| `ERR999` | Unexpected | unhandled exception |

---

### Step 3 -- สร้าง Interface (Repository + ViewReader)

#### 3a. Repository Interface -- สำหรับ CRUD

**ที่อยู่ไฟล์:** `DotnetProject.Core/Features/{FeatureName}/Abstractions/I{Name}Repository.cs`

```csharp
// ไฟล์: DotnetProject.Core/Features/Collaboration/Abstractions/ICollabRepository.cs

using DotnetProject.Core.Entities;
using FeedCommonLib.Application.Abstractions.Primitives;

namespace DotnetProject.Core.Features.Collaboration.Abstractions
{
    public interface ICollabRepository
    {
        Task<Result<List<AxonsCollab>?>> GetListStarReceive(string username, int year);
        Task<Result<List<AxonsCollab>?>> GetListStarGiven(string username, int year);
        Task<Result<AxonsCollab>> SaveGiveStar(AxonsCollab data, CancellationToken ct = default);
    }
}
```

#### 3b. ViewReader Interface -- สำหรับ query ซับซ้อน

```csharp
// ไฟล์: DotnetProject.Core/Features/Collaboration/Abstractions/ICollabViewReader.cs

using FeedCommonLib.Application.Abstractions.Primitives;

namespace DotnetProject.Core.Features.Collaboration.Abstractions
{
    public interface ICollabViewReader
    {
        Task<Result<int>> GetNextValueAsync(string sequenceName, CancellationToken ct = default);
    }
}
```

---

### Step 4 -- สร้าง Factory (Domain Logic)

**ที่อยู่ไฟล์:** `DotnetProject.Core/Features/{FeatureName}/Operations/{Name}Factory.cs`

Factory ทำหน้าที่ **validate business rules + สร้าง entity**

```csharp
// ไฟล์: DotnetProject.Core/Features/Collaboration/Operations/CollabFactory.cs

using DotnetProject.Core.Entities;
using DotnetProject.Core.Shared;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Primitives;
using Microsoft.Extensions.Logging;

namespace DotnetProject.Core.Features.Collaboration.Operations
{
    public static class CollabFactory
    {
        public static Result<AxonsCollab> CreateCollab(
            ILogger logger,
            int? id, int? year, int? sprint,
            string? givenUser, string? givenFullname,
            string? starUser, string? startFullname,
            string? subTeam, string? remark)
        {
            var _data = new
            {
                Id = id, Year = year, Sprint = sprint,
                GivenUser = givenUser, StarUser = starUser
            };

            // --- Business Rule Validation ---

            if (string.IsNullOrWhiteSpace(givenUser))
            {
                StdResponse error = StdResponse.Create(FeatureErrors.GivenUserIsNull, data: _data);
                logger.Log(error, context: nameof(CreateCollab));
                return Result<AxonsCollab>.Failure(error);
            }

            if (string.IsNullOrWhiteSpace(starUser))
            {
                StdResponse error = StdResponse.Create(FeatureErrors.StarUserIsNull, data: _data);
                logger.Log(error, context: nameof(CreateCollab));
                return Result<AxonsCollab>.Failure(error);
            }

            if (givenUser == starUser)
            {
                StdResponse error = StdResponse.Create(FeatureErrors.CannotGiveStarToSelf, data: _data);
                logger.Log(error, context: nameof(CreateCollab));
                return Result<AxonsCollab>.Failure(error);
            }

            if (id == null || year == null)
            {
                StdResponse error = StdResponse.Create(FeatureErrors.InvalidStarData, data: _data);
                logger.Log(error, context: nameof(CreateCollab),
                    customMessage: $"Validation failed: id={id}, year={year}");
                return Result<AxonsCollab>.Failure(error);
            }

            // --- สร้าง Entity เมื่อ Validation ผ่าน ---

            var collab = new AxonsCollab
            {
                Id = id.Value,
                Year = year,
                Sprint = sprint,
                GivenUser = givenUser,
                StarUser = starUser,
                GivenFullname = givenFullname,
                StarFullname = startFullname,
                Remark = remark,
                SubTeam = subTeam,
                GivenDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            return Result<AxonsCollab>.Success(collab);
        }
    }
}
```

**Pattern สำคัญ:**

- Factory เป็น `static class` + `static method`
- รับ `ILogger` เป็น parameter (ไม่ inject ผ่าน constructor)
- **ไม่ throw exception** -- return `Result.Failure(StdResponse)` เสมอ
- Log error ทุกครั้งก่อน return failure

---

### Step 5 -- สร้าง Command + Validator + ResultDto

**ที่อยู่ไฟล์:** `DotnetProject.Application/Features/{FeatureName}/Commands/{ActionName}/`

#### 5a. Command (request model)

```csharp
// ไฟล์: .../GiveStar/GiveStarCommand.cs

using FeedCommonLib.Application.Abstractions.Messaging;

namespace DotnetProject.Application.Features.Collaboration.Commands.GiveStar
{
    public record GiveStarCommand(
        string StarUser,
        string? StarFullname,
        int? Sprint,
        int? Year,
        string? Remark,
        string? GivenUser,
        string? GivenFullname,
        string? SubTeam
    ) : ICommandRequest<GiveStarResultDto>;
}
```

#### 5b. Validator

```csharp
// ไฟล์: .../GiveStar/GiveStarCommandValidator.cs

using FluentValidation;

namespace DotnetProject.Application.Features.Collaboration.Commands.GiveStar
{
    public class GiveStarCommandValidator : AbstractValidator<GiveStarCommand>
    {
        public GiveStarCommandValidator()
        {
            RuleFor(x => x.Year)
                .NotNull().WithMessage("Year cannot be null.")
                .GreaterThan(0).WithMessage("Year must be greater than 0.");

            RuleFor(x => x.Sprint)
                .NotNull().WithMessage("Sprint cannot be null.")
                .GreaterThan(0).WithMessage("Sprint must be greater than 0.");

            RuleFor(x => x.StarUser)
                .NotNull().WithMessage("StarUser cannot be null.")
                .NotEmpty().WithMessage("StarUser is required.");

            RuleFor(x => x.GivenUser)
                .NotNull().WithMessage("GivenUser cannot be null.")
                .NotEmpty().WithMessage("GivenUser is required.");
        }
    }
}
```

#### 5c. ResultDto

```csharp
// ไฟล์: .../GiveStar/GiveStarResultDto.cs

using DotnetProject.Application.Features.Common;

namespace DotnetProject.Application.Features.Collaboration.Commands.GiveStar
{
    public class GiveStarResultDto : BaseResponse
    {
        public int Id { get; set; }
    }
}
```

> `BaseResponse` มี `IsSuccess` กับ `Message` อยู่แล้ว -- เพิ่มเฉพาะ field ที่ต้องการ

---

### Step 6 -- สร้าง Handler (ROP Chain)

**ที่อยู่ไฟล์:** `DotnetProject.Application/Features/{FeatureName}/Commands/{ActionName}/{ActionName}Handler.cs`

```csharp
// ไฟล์: .../GiveStar/GiveStarHandler.cs

using DotnetProject.Core.Features.Collaboration.Abstractions;
using DotnetProject.Core.Features.Collaboration.Operations;
using FeedCommonLib.Application.Abstractions.Messaging;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.Tracing;
using Microsoft.Extensions.Logging;

namespace DotnetProject.Application.Features.Collaboration.Commands.GiveStar
{
    public class GiveStarHandler : ICommandRequestHandler<GiveStarCommand, GiveStarResultDto>
    {
        private readonly ICollabRepository _collabRepository;
        private readonly ICollabViewReader _collabViewReader;
        private readonly ILogger<GiveStarHandler> _logger;

        public GiveStarHandler(
            ICollabRepository collabRepository,
            ICollabViewReader collabViewReader,
            ILogger<GiveStarHandler> logger)
        {
            _collabRepository = collabRepository;
            _collabViewReader = collabViewReader;
            _logger = logger;
        }

        public async Task<Result<GiveStarResultDto>> Handle(
            GiveStarCommand command,
            CancellationToken ct)
        {
            return await InstrumentedResultExtensions

                // Step 1: ดึง sequence id (async, ViewReader)
                .BeginTracingAsync(() =>
                    _collabViewReader.GetNextValueAsync("axons_collab_id_seq", ct))

                // Step 2: validate + สร้าง entity (sync, Factory)
                .Then(newId => CollabFactory.CreateCollab(
                    _logger, newId,
                    command.Year, command.Sprint,
                    command.GivenUser, command.GivenFullname,
                    command.StarUser, command.StarFullname,
                    command.SubTeam, command.Remark))

                // Step 3: บันทึกลง database (async, Repository)
                .ThenAsync(async saveData =>
                    await _collabRepository.SaveGiveStar(saveData, ct))

                // Step 4: แปลงเป็น response DTO (sync, Map)
                .Map(res => Result<GiveStarResultDto>.Success(
                    new GiveStarResultDto { IsSuccess = true, Id = res.Id },
                    SuccessCodes.Created));
        }
    }
}
```

### ROP Chain อธิบายทีละ step

```
Step 1: BeginTracingAsync(GetNextValueAsync)
  Input:  (none)
  Output: Result<int>              -- sequence id เช่น 42
  Fail:   Result.Failure(ERR201)   -- database error --> STOP

Step 2: Then(CollabFactory.CreateCollab)
  Input:  int (newId = 42)
  Output: Result<AxonsCollab>      -- entity พร้อม save
  Fail:   Result.Failure(ERR301)   -- business rule error --> STOP

Step 3: ThenAsync(SaveGiveStar)
  Input:  AxonsCollab (entity)
  Output: Result<AxonsCollab>      -- saved entity
  Fail:   Result.Failure(ERR204)   -- unique constraint --> STOP

Step 4: Map(create DTO)
  Input:  AxonsCollab (saved)
  Output: Result<GiveStarResultDto>  -- final response
```

**เลือก method ให้ถูก:**

| Method | ใช้เมื่อ | ตัวอย่าง |
|--------|---------|---------|
| `BeginTracingAsync` | เริ่มต้น chain | เรียก ViewReader ครั้งแรก |
| `Then` | sync operation | Factory.Create(), validation |
| `ThenAsync` | async operation | Repository.Save() |
| `Map` | แปลงผลลัพธ์สุดท้าย (sync) | สร้าง ResultDto |
| `MapAsync` | แปลงผลลัพธ์สุดท้าย (async) | ถ้าต้อง await ตอน map |
| `TapAsync` | side-effect (ไม่เปลี่ยนค่า) | log, send notification |

---

### Step 7 -- Implement Repository + ViewReader ใน Infrastructure

#### 7a. Repository (EF Core)

**ที่อยู่ไฟล์:** `DotnetProject.Infrastructure.Postgresql/Repositories/{Name}Repository.cs`

```csharp
// ไฟล์: DotnetProject.Infrastructure.Postgresql/Repositories/CollabRepository.cs

using DotnetProject.Core.Entities;
using DotnetProject.Core.Features.Collaboration.Abstractions;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DotnetProject.Infrastructure.Postgresql.Repositories
{
    public class CollabRepository : ICollabRepository
    {
        readonly PostgresqlApplicationDbContext context;
        ILogger<CollabRepository> logger;

        public CollabRepository(
            PostgresqlApplicationDbContext _context,
            ILogger<CollabRepository> _logger)
        {
            context = _context;
            logger = _logger;
        }

        public async Task<Result<AxonsCollab>> SaveGiveStar(
            AxonsCollab data,
            CancellationToken ct = default)
        {
            try
            {
                await context.AxonsCollabs.AddAsync(data);
                await context.SaveChangesAsync();
                return Result<AxonsCollab>.Success(data);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pgEx
                      && pgEx.SqlState == "23505")
            {
                var error = StdResponse.FromException(Errors.UniqueConstraint, ex, data);
                logger.Log(error, context: nameof(SaveGiveStar));
                return Result<AxonsCollab>.Failure(error);
            }
            catch (Exception ex)
            {
                return Result<AxonsCollab>.Failure(
                    StdResponse.FromException(Errors.Database, ex));
            }
        }

        public async Task<Result<List<AxonsCollab>?>> GetListStarReceive(
            string username, int year)
        {
            try
            {
                var result = await (
                    from col in context.AxonsCollabs
                    where col.StarUser != null
                       && col.StarUser.ToLower() == username.ToLower()
                       && col.Year == year
                    orderby col.Id descending
                    select col
                ).AsNoTracking().ToListAsync();

                return Result<List<AxonsCollab>?>.Success(result);
            }
            catch (Exception ex)
            {
                var error = StdResponse.FromException(
                    Errors.Database, ex, new { username, year });
                logger.Log(error, context: nameof(GetListStarReceive));
                return Result<List<AxonsCollab>?>.Failure(error);
            }
        }

        public async Task<Result<List<AxonsCollab>?>> GetListStarGiven(
            string username, int year)
        {
            try
            {
                var result = await (
                    from col in context.AxonsCollabs
                    where col.GivenUser != null
                       && col.GivenUser.ToLower() == username.ToLower()
                       && col.Year == year
                    select col
                ).AsNoTracking().ToListAsync();

                return Result<List<AxonsCollab>?>.Success(result);
            }
            catch (Exception ex)
            {
                var error = StdResponse.FromException(
                    Errors.Database, ex, new { username, year });
                logger.Log(error, context: nameof(GetListStarGiven));
                return Result<List<AxonsCollab>?>.Failure(error);
            }
        }
    }
}
```

#### 7b. ViewReader (Dapper)

**ที่อยู่ไฟล์:** `DotnetProject.Infrastructure.Postgresql/Queries/{Name}ViewReader.cs`

```csharp
// ไฟล์: DotnetProject.Infrastructure.Postgresql/Queries/CollabViewReader.cs

using Dapper;
using DotnetProject.Core.Features.Collaboration.Abstractions;
using DotnetProject.Infrastructure.Postgresql.Persistence;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace DotnetProject.Infrastructure.Postgresql.Queries
{
    public class CollabViewReader : ICollabViewReader
    {
        readonly PostgresqlApplicationDbContext context;

        public CollabViewReader(
            PostgresqlApplicationDbContext _context,
            IConfiguration configuration)
        {
            context = _context;
        }

        public async Task<Result<int>> GetNextValueAsync(
            string sequenceName,
            CancellationToken ct = default)
        {
            const string sql = @"SELECT nextval(@SequenceName)";

            try
            {
                DbConnection connection = context.Database.GetDbConnection();
                int result = await connection.QuerySingleAsync<int>(
                    sql, new { SequenceName = sequenceName });

                return Result<int>.Success(Convert.ToInt32(result));
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(
                    StdResponse.FromException(Errors.Database, ex));
            }
        }
    }
}
```

---

### Step 8 -- สร้าง Endpoint

**ที่อยู่ไฟล์:** `DotnetProject.Api/Endpoints/{FeatureName}/{ActionName}Endpoint.cs`

```csharp
// ไฟล์: DotnetProject.Api/Endpoints/Collaboration/GiveStarEndpoint.cs

using DotnetProject.Application.Features.Collaboration.Commands.GiveStar;
using FastEndpoints;
using FeedCommonLib.Application.Abstractions.Primitives;
using FeedCommonLib.Application.Abstractions.EndpointExtension;

namespace DotnetProject.Api.Endpoints.UserInfo
{
    public class GiveStarEndpoint : Endpoint<GiveStarCommand, GiveStarResultDto>
    {
        public override void Configure()
        {
            Post("/api/collaboration/givestar");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GiveStarCommand req, CancellationToken ct)
        {
            GiveStarHandler handler = Resolve<GiveStarHandler>();
            Result<GiveStarResultDto> result = await handler.Handle(req, ct);
            await result.ToApiResponse(HttpContext, Logger);
        }
    }
}
```

### Command Summary -- ไฟล์ทั้งหมดที่ต้องสร้าง

```
1. Core/Domain/Entities/AxonsCollab.cs                               -- entity (ถ้ายังไม่มี)
2. Core/Shared/FeatureErrors.cs                                      -- เพิ่ม Error
3. Core/Features/Collaboration/Abstractions/ICollabRepository.cs     -- CRUD interface
4. Core/Features/Collaboration/Abstractions/ICollabViewReader.cs     -- complex query interface
5. Core/Features/Collaboration/Operations/CollabFactory.cs           -- domain logic
6. Application/Features/Collaboration/Commands/GiveStar/
     GiveStarCommand.cs                                              -- request model
     GiveStarCommandValidator.cs                                     -- validation
     GiveStarResultDto.cs                                            -- response DTO
     GiveStarHandler.cs                                              -- handler (ROP chain)
7. Infrastructure.Postgresql/Repositories/CollabRepository.cs        -- EF Core impl
8. Infrastructure.Postgresql/Queries/CollabViewReader.cs             -- Dapper impl
9. Api/Endpoints/Collaboration/GiveStarEndpoint.cs                   -- endpoint
```

---

## Part I -- Serilog: Logger Setup และวิธีใช้งาน

> อ้างอิง: `Program.cs`
> NuGet: `Serilog.AspNetCore`, `Serilog.Enrichers.Span`, `Serilog.Sinks.File`, `Serilog.Sinks.Console`

### ภาพรวม

```
                    Serilog
                   /   |   \
            Console   File   OpenTelemetry
                       |
                 +-----+-----+
                 |           |
            info/*.txt   error/*.txt
          (30 วัน)      (90 วัน)
```

### Step 1 -- Configure Serilog

```csharp
var logOutputTemplate =
    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] " +
    "{Message:lj} {Properties} | TraceId: {TraceId} SpanId: {SpanId}" +
    "{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()                                            // TraceId + SpanId
    .Enrich.WithProperty("service.name", projectName)
    .Enrich.WithProperty("deployment.environment",
        builder.Environment.EnvironmentName)

    // --- Sink: Console ---
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] " +
            "{Message:lj} {Properties} | TraceId: {TraceId} SpanId: {SpanId}" +
            "{NewLine}{Exception}"
    )

    // --- Sink: File (Info + Warning) ---
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt =>
            evt.Level == LogEventLevel.Information ||
            evt.Level == LogEventLevel.Warning)
        .WriteTo.File(
            "logs/info/log-info-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: logOutputTemplate,
            retainedFileCountLimit: 30               // เก็บ 30 วัน
        )
    )

    // --- Sink: File (Error + Fatal) ---
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt =>
            evt.Level >= LogEventLevel.Error)
        .WriteTo.File(
            "logs/error/log-error-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: logOutputTemplate,
            retainedFileCountLimit: 90               // เก็บ 90 วัน
        )
    )

    .CreateLogger();
```

### Step 2 -- เชื่อมต่อ Serilog กับ ASP.NET Core + OpenTelemetry

```csharp
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
    loggingBuilder.AddOpenTelemetry();
});
builder.Host.UseSerilog();

// --- ใน middleware pipeline ---
app.UseSerilogRequestLogging();
```

### โครงสร้างไฟล์ Log

```
logs/
  info/
    log-info-20250115.txt    -- Information + Warning (เก็บ 30 วัน)
    log-info-20250116.txt
  error/
    log-error-20250115.txt   -- Error + Fatal (เก็บ 90 วัน)
    log-error-20250116.txt
```

### วิธีใช้ Logger ในโค้ด

#### ใน Handler / Service -- ผ่าน DI

```csharp
public class GiveStarHandler : ICommandRequestHandler<GiveStarCommand, GiveStarResultDto>
{
    private readonly ILogger<GiveStarHandler> _logger;

    public GiveStarHandler(ILogger<GiveStarHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<GiveStarResultDto>> Handle(...)
    {
        _logger.LogInformation("Processing GiveStar command for user {User}", command.StarUser);
    }
}
```

#### ใน Factory -- ผ่าน parameter

```csharp
public static Result<AxonsCollab> CreateCollab(ILogger logger, ...)
{
    StdResponse error = StdResponse.Create(FeatureErrors.CannotGiveStarToSelf, data: inputData);
    logger.Log(error, context: nameof(CreateCollab));
    // output: Error [ERR301] CreateCollab: You cannot give a star to yourself. HttpStatus: 400.
}
```

#### ใน Repository/ViewReader -- ผ่าน DI

```csharp
public class CollabRepository : ICollabRepository
{
    ILogger<CollabRepository> logger;
    // ...
    // ใน catch block:
    var error = StdResponse.FromException(Errors.Database, ex, data);
    logger.Log(error, context: nameof(SaveGiveStar));
}
```

### StdResponse + Logger -- Extension Method

> อ้างอิง: `FeedCommonLib.Application.Abstractions/ResponseCodes/ErrorLogginExtension.cs`

```csharp
// extension method: logger.Log(StdResponse, ...)
logger.Log(error, context: "MethodName", customMessage: "Additional info");
```

**Log Level ถูกกำหนดอัตโนมัติจาก Error Code:**

| Error Code Range | Log Level | ตัวอย่าง |
|-----------------|-----------|---------|
| `ERR0xx` (Domain) | Warning | NotFound, Validation |
| `ERR1xx` (Application) | Warning | Unauthorized, Forbidden |
| `ERR2xx` (Infrastructure) | Error | Database, Timeout |
| `ERR3xx` (Custom) | Warning | CannotGiveStarToSelf |
| `ERR999` (Unexpected) | Critical | Unhandled exception |

### สรุป Logging Strategy

```
เลือกใช้:

  ข้อมูลทั่วไป (flow, timing)
    --> _logger.LogInformation("...")
    --> _logger.LogWarning("...")

  error ที่มี Error code (business rule, database)
    --> StdResponse error = StdResponse.Create(Error, data)
    --> _logger.Log(error, context: "MethodName")

  exception ที่ catch ได้
    --> StdResponse error = StdResponse.FromException(Error, ex, data)
    --> _logger.Log(error, context: "MethodName")
```

---

## Part J -- FeedCommonLib: วิธีใช้งาน Shared Library

> อ้างอิง: `FeedCommonLib.Application.Abstractions` project
> Library กลางที่ใช้ร่วมกันข้ามหลาย microservice

### 1. Result Pattern -- ห่อผลลัพธ์ทุก operation

```csharp
// สร้าง success result
Result<MyData> success = Result<MyData>.Success(data);

// สร้าง success result พร้อม success code
Result<MyData> success = Result<MyData>.Success(data, SuccessCodes.Created);

// สร้าง failure result
StdResponse error = StdResponse.Create(Errors.NotFound);
Result<MyData> failure = Result<MyData>.Failure(error);

// ตรวจสอบผลลัพธ์
if (result.IsSuccess)
{
    var data = result.Value;  // ข้อมูล
}
if (result.IsFailure)
{
    var error = result.Error;  // StdResponse
}
```

### 2. StdResponse -- สร้าง Error Object

```csharp
// จาก built-in Error
StdResponse error = StdResponse.Create(Errors.NotFound);

// จาก custom Error + data
StdResponse error = StdResponse.Create(FeatureErrors.CannotGiveStarToSelf, data: inputData);

// จาก error code string
StdResponse error = StdResponse.Create("ERR301", data: inputData, customMessage: "custom msg");

// จาก Exception
StdResponse error = StdResponse.FromException(Errors.Database, ex, data: inputData);
// Details จะมี: ExceptionType, StackTrace, InnerException
```

### 3. Errors -- Built-in Error Codes

```csharp
// ใช้ได้ทันทีจาก FeedCommonLib
Errors.NotFound           // ERR001, 404
Errors.Validation         // ERR002, 400
Errors.BusinessRule       // ERR003, 400
Errors.Conflict           // ERR004, 409
Errors.InvalidInput       // ERR005, 400
Errors.Unauthorized       // ERR101, 401
Errors.Forbidden          // ERR102, 403
Errors.Database           // ERR201, 500
Errors.ExternalService    // ERR202, 502
Errors.Timeout            // ERR203, 504
Errors.UniqueConstraint   // ERR204, 409
Errors.Unexpected         // ERR999, 500

// ลงทะเบียน Error ใหม่
public static readonly Error MyError = Errors.Register(
    "ERR306", "My custom error message", StatusCodes.Status400BadRequest);
```

### 4. Logger Extension -- Structured Logging

```csharp
// Log error พร้อม context (log level ถูกกำหนดอัตโนมัติ)
logger.Log(error, context: nameof(MyMethod));

// Log error พร้อม custom message
logger.Log(error, context: nameof(MyMethod), customMessage: "Additional info");

// Output format:
// Error [ERR301] MyMethod: You cannot give a star to yourself. HttpStatus: 400. Data: {...}
```

### 5. InstrumentedResultExtensions -- ROP Chain + Tracing

```csharp
// เริ่ม chain ใหม่ (async)
return await InstrumentedResultExtensions
    .BeginTracingAsync(() => _viewReader.GetDataAsync(id, ct))

    // ต่อ chain (sync) -- ใช้กับ Factory, validation
    .Then(data => MyFactory.Create(logger, data))

    // ต่อ chain (async) -- ใช้กับ Repository, external service
    .ThenAsync(async entity => await _repository.SaveAsync(entity, ct))

    // side-effect (ไม่เปลี่ยนค่า)
    .TapAsync(saved => _logger.LogInformation("Saved: {Id}", saved.Id))

    // map เป็น response DTO (sync) พร้อมส่ง SuccessCode
    .Map(saved => Result<MyResultDto>.Success(
        new MyResultDto { Id = saved.Id },
        SuccessCodes.Created));
```

### 6. ToApiResponse -- แปลง Result เป็น HTTP Response

```csharp
// ใน Endpoint -- ใช้แค่บรรทัดเดียว
await result.ToApiResponse(HttpContext, Logger);

// Success --> HTTP {statusCode} + ApiResponse<T> { isSuccess: true, codeResult: "SUC200", dataResult: ... }
// Failure --> HTTP {statusCode} + ApiResponse<T> { isSuccess: false, codeResult: "ERR301", ... }
```

### 7. Messaging Interfaces -- CQRS

```csharp
// Command (เขียนข้อมูล)
public record MyCommand(...) : ICommandRequest<MyResultDto>;
public class MyHandler : ICommandRequestHandler<MyCommand, MyResultDto>
{
    public async Task<Result<MyResultDto>> Handle(MyCommand command, CancellationToken ct) { ... }
}

// Query (อ่านข้อมูล)
public record MyQuery(...) : IQueryRequest<MyData>;
public class MyHandler : IQueryRequestHandler<MyQuery, MyData>
{
    public async Task<Result<MyData>> Handle(MyQuery query, CancellationToken ct) { ... }
}
```

### 8. ActivitySourceProvider -- Manual Tracing

```csharp
// สร้าง span เอง (กรณีพิเศษที่ไม่ได้ใช้ ROP chain)
using var activity = ActivitySourceProvider.Source.StartActivity("MyOperation");
activity?.SetTag("input.type", "MyType");

// ... ทำงาน ...

activity?.SetStatus(ActivityStatusCode.Ok);
```

---

## Part K -- IntegrationTestLib: วิธีเขียน Integration Test

> อ้างอิง: `DotnetProject.IntegrationTest` project + `IntegrationTestLib` library
> Framework: NUnit + WebApplicationFactory

### โครงสร้าง Integration Test

```
DotnetProject.IntegrationTest/
  TestFixture.cs                -- OneTimeSetUp (shared resources)
  IntegrationTestBase.cs        -- base class สำหรับทุก test
  jsonResponse/                 -- JSON test data (request + expected response)
    UserInfo/
      GetUserProject/
        GetUserProjectSuccess_Should_GetData.json
        GetUserProjectUserIsEmpty_Should_ReturnBadRequest.json
  Scripts/                      -- SQL scripts สำหรับ setup/teardown data
    UserInfo/
      GetUserProject/
        Setup/
          SetupUserProject.sql
        TearDown/
          TeardownUserProject.sql
```

### Step 1 -- TestFixture (shared resources)

```csharp
// อ้างอิง: TestFixture.cs

[SetUpFixture]
public class TestFixture
{
    public static WebApplicationFactory<Program> Factory { get; private set; }
    public static IConfiguration Configuration { get; private set; }
    public static IntegrationFunction Fn { get; private set; }
    public static string JsonBasePath { get; private set; }
    public static string ScriptBasePath { get; private set; }

    [OneTimeSetUp]
    public void AssemblyInitialize()
    {
        Factory = new WebApplicationFactory<Program>();

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        Configuration = configurationBuilder.Build();

        Fn = new IntegrationFunction(Configuration);
        JsonBasePath = @"D:\Project\DotnetProject\src\DotnetProject.IntegrationTest\jsonResponse\";
        ScriptBasePath = @"D:\Project\DotnetProject\src\DotnetProject.IntegrationTest\Scripts\";
    }

    [OneTimeTearDown]
    public void AssemblyCleanup()
    {
        Factory?.Dispose();
    }
}
```

### Step 2 -- IntegrationTestBase (base class)

```csharp
// อ้างอิง: IntegrationTestBase.cs

public class IntegrationTestBase : IDisposable
{
    protected static WebApplicationFactory<Program> Factory => TestFixture.Factory;
    protected HttpClient Client;
    protected IConfiguration Configuration => TestFixture.Configuration;
    protected IntegrationFunction fn => TestFixture.Fn;
    protected string jsonBasePath => TestFixture.JsonBasePath;
    protected string scriptBasePath => TestFixture.ScriptBasePath;

    protected IntegrationTestBase()
    {
        Client = Factory.CreateClient();
    }

    public void Dispose()
    {
        Client?.Dispose();
    }
}
```

### Step 3 -- เขียน JSON test data

```json
{
  "key": "GetUserProjectSuccess",
  "desc": "Should return project data for valid user",
  "withRequest": {
    "method": "GET",
    "path": "/api/userinfo/project",
    "headers": {},
    "query": {
      "userName": "john.doe"
    }
  },
  "withResponse": {
    "body": {
      "statusCode": 200,
      "isSuccess": true,
      "codeResult": "SUC200",
      "message": "Request completed successfully",
      "dataResult": [
        {
          "year": 2025,
          "projectCode": "PRJ001",
          "projectName": "Project A",
          "username": "john.doe",
          "fullName": "John Doe"
        }
      ]
    }
  }
}
```

### Step 4 -- เขียน Test Class

```csharp
// อ้างอิง: GetUserProjectTest.cs

[TestFixture]
public class GetUserProjectTest : IntegrationTestBase
{
    private static PostgresqlTestDataHelper _testDataHelper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();
        _testDataHelper = new PostgresqlTestDataHelper(
            configuration, "DefaultConnection", scriptBasePath);
    }

    [SetUp]
    public async Task Setup()
    {
        // เตรียมข้อมูลก่อนแต่ละ test case
        await _testDataHelper.SetupTestData(
            "UserInfo\\GetUserProject\\Setup\\SetupUserProject.sql");
    }

    [TearDown]
    public async Task TearDown()
    {
        // ลบข้อมูลหลังแต่ละ test case
        await _testDataHelper.CleanupTestData(
            "UserInfo\\GetUserProject\\TearDown\\TeardownUserProject.sql");
    }

    [Test]
    public async Task GetUserProjectSuccess_Should_GetData()
    {
        // Arrange
        string _filePath = jsonBasePath +
            "UserInfo/GetUserProject/GetUserProjectSuccess_Should_GetData.json";
        var jData = fn.GetJsonObject<GetUserProjectQuery, IEnumerable<ProjectRecord>>(_filePath);

        // Act
        string url = $"/api/userinfo/project?{jData.QueryString}";
        HttpResponseMessage actualResponse = await Client.GetAsync(url);

        // Assert
        CompareResult compareResult =
            await fn.CompareResponseByObject<IEnumerable<ProjectRecord>>(
                actualResponse, jData.WithResponse);
        Assert.That(compareResult.IsEqual, Is.True, compareResult.Message);
    }
}
```

### IntegrationTestLib -- Functions ที่ใช้บ่อย

| Function | คำอธิบาย |
|----------|---------|
| `fn.GetJsonObject<TReq, TRes>(path)` | อ่าน JSON file เป็น test model |
| `fn.GetJsonObjectWithToken<TReq, TRes>(path)` | อ่าน JSON + แนบ JWT |
| `fn.GenerateJwtToken(user, issuer)` | สร้าง JWT token |
| `fn.CompareResponseByObject<T>(response, expected)` | เปรียบเทียบ response |
| `_testDataHelper.SetupTestData(script)` | รัน SQL setup |
| `_testDataHelper.CleanupTestData(script)` | รัน SQL cleanup |

---

## Part L -- Unit Test: วิธีเขียน Unit Test

> อ้างอิง: `DotnetProject.Test` project
> Framework: NUnit + NSubstitute

### ตัวอย่าง: Test Handler

```csharp
// อ้างอิง: GetUserProjectHandlerTests.cs

using DotnetProject.Application.Features.UserInfo.Queries.GetUserProject;
using DotnetProject.Core.Features.UserInfo;
using DotnetProject.Core.Features.UserInfo.Abstractions;
using FeedCommonLib.Application.Abstractions.ResponseCodes;
using FeedCommonLib.Application.Abstractions.Primitives;
using NSubstitute;

[TestFixture]
public class GetUserProjectHandlerTests
{
    private IUserProjectViewReader _userProjectViewReader;
    private GetUserProjectHandler _handler;

    [SetUp]
    public void SetUp()
    {
        // Mock interface ด้วย NSubstitute
        _userProjectViewReader = Substitute.For<IUserProjectViewReader>();
        _handler = new GetUserProjectHandler(_userProjectViewReader);
    }

    [Test]
    public async Task Handle_WhenProjectsExist_ShouldReturnSuccess()
    {
        // Arrange
        var query = new GetUserProjectQuery("john.doe");
        var projects = new List<ProjectRecord>
        {
            new() { ProjectCode = "PRJ001", ProjectName = "Project A",
                     Username = "john.doe", FullName = "John Doe", Year = 2025 },
        };

        _userProjectViewReader.GetProjectsByUserNameAsync("john.doe", Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProjectRecord>>.Success(projects));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Handle_WhenReaderFails_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetUserProjectQuery("john.doe");
        var error = StdResponse.Create(Errors.Database);

        _userProjectViewReader.GetProjectsByUserNameAsync("john.doe", Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProjectRecord>>.Failure(error));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }
}
```

### Unit Test Pattern

```
1. SetUp     -- สร้าง mock (NSubstitute) + สร้าง handler
2. Arrange   -- เตรียม input + กำหนด mock behavior
3. Act       -- เรียก handler.Handle()
4. Assert    -- ตรวจสอบ result (IsSuccess/IsFailure, Value, Error)
```

### NSubstitute -- วิธีใช้งาน

```csharp
// สร้าง mock
var mock = Substitute.For<IMyInterface>();

// กำหนด return value
mock.MyMethod(Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(Result<MyData>.Success(data));

// กำหนด return failure
mock.MyMethod(Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(Result<MyData>.Failure(StdResponse.Create(Errors.NotFound)));

// ตรวจสอบว่า method ถูกเรียก
await mock.Received(1).MyMethod("expected-value", Arg.Any<CancellationToken>());
```
