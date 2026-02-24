# DotnetProject -- Architecture & Concept Overview

> เอกสารนี้อธิบาย **ภาพรวมสถาปัตยกรรม** และ **แนวคิดการออกแบบ** ของโปรเจค DotnetProject
> เขียนสำหรับ **Programmer ที่เคยเขียน C# แบบ MVC** มาก่อน
> สำหรับขั้นตอนการเขียนโค้ดแบบละเอียด ดูที่ [02_CODING_GUIDE.md](./02_CODING_GUIDE.md)

---

## สารบัญ

- [DotnetProject -- Architecture \& Concept Overview](#dotnetproject----architecture--concept-overview)
  - [สารบัญ](#สารบัญ)
  - [1. ถ้าเคยเขียน MVC มาก่อน -- อะไรเปลี่ยนไปบ้าง?](#1-ถ้าเคยเขียน-mvc-มาก่อน----อะไรเปลี่ยนไปบ้าง)
  - [2. ภาพรวมสถาปัตยกรรม (Clean Architecture)](#2-ภาพรวมสถาปัตยกรรม-clean-architecture)
  - [3. โครงสร้างโปรเจค](#3-โครงสร้างโปรเจค)
  - [4. Railway Oriented Programming (ROP)](#4-railway-oriented-programming-rop)
    - [แนวคิด](#แนวคิด)
    - [ทำไมถึงใช้ ROP ในโปรเจคนี้](#ทำไมถึงใช้-rop-ในโปรเจคนี้)
    - [ตัวอย่างเปรียบเทียบ](#ตัวอย่างเปรียบเทียบ)
    - [Flow Diagram ของ GiveStar](#flow-diagram-ของ-givestar)
  - [5. Instrumented ROP Chain (Tracing)](#5-instrumented-rop-chain-tracing)
    - [Chain Methods ที่ใช้ได้](#chain-methods-ที่ใช้ได้)
    - [หลักการเลือก method](#หลักการเลือก-method)
    - [OpenTelemetry Trace ที่ได้](#opentelemetry-trace-ที่ได้)
  - [6. Result Pattern และ StdResponse](#6-result-pattern-และ-stdresponse)
    - [`Result<T>` -- ห่อผลลัพธ์ทุก operation](#resultt----ห่อผลลัพธ์ทุก-operation)
    - [`StdResponse` -- มาตรฐาน error ทุก layer](#stdresponse----มาตรฐาน-error-ทุก-layer)
  - [7. CQRS -- Command / Query Separation](#7-cqrs----command--query-separation)
    - [Command (เขียนข้อมูล)](#command-เขียนข้อมูล)
    - [Query (อ่านข้อมูล)](#query-อ่านข้อมูล)
    - [Folder Convention](#folder-convention)
  - [8. ViewReader vs Repository](#8-viewreader-vs-repository)
    - [ถามตัวเองก่อนเลือก](#ถามตัวเองก่อนเลือก)
    - [ตัวอย่าง](#ตัวอย่าง)
  - [9. Validation Strategy](#9-validation-strategy)
    - [ระดับ 1: Endpoint Validator (input validation)](#ระดับ-1-endpoint-validator-input-validation)
    - [ระดับ 2: Domain Validation (business rules)](#ระดับ-2-domain-validation-business-rules)
  - [10. Error Code \& Success Code System](#10-error-code--success-code-system)
    - [โครงสร้าง Error Code](#โครงสร้าง-error-code)
    - [Built-in Errors จาก FeedCommonLib](#built-in-errors-จาก-feedcommonlib)
    - [Built-in Success Codes จาก FeedCommonLib](#built-in-success-codes-จาก-feedcommonlib)
    - [ลงทะเบียน Error Code ใหม่ (Feature-specific)](#ลงทะเบียน-error-code-ใหม่-feature-specific)
  - [11. DI Auto-Registration](#11-di-auto-registration)
  - [12. API Response Format](#12-api-response-format)
    - [Success Response](#success-response)
    - [Error Response](#error-response)
  - [13. FeedCommonLib -- Shared Library](#13-feedcommonlib----shared-library)
    - [วิธีใช้ FeedCommonLib ที่สำคัญ](#วิธีใช้-feedcommonlib-ที่สำคัญ)
  - [14. IntegrationTestLib -- Integration Test Library](#14-integrationtestlib----integration-test-library)
    - [วิธีใช้ใน Integration Test](#วิธีใช้ใน-integration-test)
    - [IntegrationFunction -- Methods ที่ใช้บ่อย](#integrationfunction----methods-ที่ใช้บ่อย)
    - [PostgresqlTestDataHelper -- Methods](#postgresqltestdatahelper----methods)

---

## 1. ถ้าเคยเขียน MVC มาก่อน -- อะไรเปลี่ยนไปบ้าง?

ถ้าเคยเขียน ASP.NET MVC / Web API แบบปกติ จะคุ้นเคยกับ pattern นี้:

```
Controller  -->  Service  -->  Repository  -->  Database
```

โปรเจคนี้ **เปลี่ยนจาก MVC** ไปใช้โครงสร้างแบบ **Clean Architecture + CQRS + ROP**

| สิ่งที่คุ้นเคย (MVC) | สิ่งที่เปลี่ยนไป (โปรเจคนี้) | ทำไมถึงเปลี่ยน |
|---|---|---|
| **Controller** รวมหลาย action | **FastEndpoints** -- 1 Endpoint = 1 Class | แยกความรับผิดชอบชัดเจน |
| `return Ok(data)` / `return BadRequest()` | `Result<T>` + `ToApiResponse()` | จัดการ success/error เป็นมาตรฐานเดียว |
| `try/catch` ซ้อนกันหลายชั้น | **ROP chain** (`.Then()` / `.ThenAsync()`) | ถ้า step ใดพัง หยุดทันที ไม่ต้องเขียน if/else |
| Service class ทำทุกอย่าง | **Handler** (orchestrate) + **Factory** (validate + สร้าง entity) | แยก business logic ออกจาก orchestration |
| Error คือ exception | Error คือ **`StdResponse`** ห่อใน `Result.Failure()` | ไม่ throw exception -- ส่ง error ผ่าน Result |
| Register DI เอง ใน `Startup.cs` | **Auto-scan** assembly | ไม่ต้อง register เอง สร้าง class ให้ถูก interface แค่นั้น |

> **สิ่งสำคัญ:** ไม่ต้อง throw exception ในโค้ด business logic --
> ใช้ `Result.Failure(StdResponse)` แทนทุกกรณี

---

## 2. ภาพรวมสถาปัตยกรรม (Clean Architecture)

โปรเจกต์นี้ใช้ 3 แนวคิดหลัก:

1. **Clean Architecture** -- แยก layer ชัดเจน ไม่ให้ business logic ผูกกับ framework
2. **CQRS** -- แยก "อ่านข้อมูล" (Query) กับ "เขียนข้อมูล" (Command)
3. **Railway Oriented Programming (ROP)** -- chain operation แบบ 2 ราง (Success / Failure)

```
  HTTP Request
       |
       v
 +------------------+
 |    Api Layer      |   FastEndpoints -- รับ/ส่ง HTTP
 +------------------+
       |
       v
 +------------------+
 | Application Layer |   Handler + Validator -- orchestrate business flow
 +------------------+
       |
       v
 +------------------+
 |   Core Layer      |   Entity, Interface, Factory -- domain logic
 +------------------+
       ^
       | (implement)
 +-----+--------------+
 | Infrastructure     |   Repository (EF Core) + ViewReader (Dapper)
 +---------------------+
```

**Dependency Rule** -- ทุก layer ชี้เข้าหา Core Layer เสมอ:

- Api --> Application --> Core
- Infrastructure --> Core
- **Core ไม่รู้จัก layer อื่นเลย** (ไม่ import namespace ของ layer อื่น)

> **เทียบกับ MVC:** แต่เดิม Controller เรียก Service เรียก Repository ตรงๆ --
> ในโปรเจคนี้ทุกอย่างผ่าน **interface** ที่อยู่ใน Core Layer

---

## 3. โครงสร้างโปรเจค

| Project | Target | หน้าที่ |
|---------|--------|---------|
| **DotnetProject.Api** | .NET 10 | HTTP Endpoint, DI setup, OpenTelemetry, Swagger |
| **DotnetProject.Application** | .NET 10 | Handler, Validator, DTO, Feature orchestration |
| **DotnetProject.Core** | .NET 10 | Entity, Interface (abstractions), Factory, Error code |
| **DotnetProject.Infrastructure** | .NET 10 | Base DbContext, Entity Configurations |
| **DotnetProject.Infrastructure.Postgresql** | .NET 10 | PostgreSQL DbContext, Repository (EF Core), ViewReader (Dapper) |
| **DotnetProject.Test** | .NET 10 | Unit Tests (NUnit + NSubstitute) |
| **DotnetProject.IntegrationTest** | .NET 10 | Integration Tests (NUnit + WebApplicationFactory) |

```
DotnetProject.Api/
  Endpoints/
    Collaboration/
      GiveStarEndpoint.cs              -- POST (command)
    UserInfo/
      GetUserProjectEndpoint.cs        -- GET  (query)
  Program.cs                           -- DI, Middleware, OpenTelemetry, Serilog

DotnetProject.Application/
  Features/
    Collaboration/
      Commands/
        GiveStar/
          GiveStarCommand.cs           -- ICommandRequest
          GiveStarCommandValidator.cs  -- FluentValidation
          GiveStarHandler.cs           -- ICommandRequestHandler (ROP chain)
          GiveStarResultDto.cs         -- response DTO
    UserInfo/
      Queries/
        GetUserProject/
          GetUserProjectQuery.cs       -- IQueryRequest
          GetUserProjectQueryValidator.cs -- TracedValidator
          GetUserProjectHandler.cs     -- IQueryRequestHandler
    Common/
      TracedValidator.cs               -- FluentValidation + OpenTelemetry
      BaseResponse.cs                  -- base class สำหรับ result DTO
  Extensions/
    ApplicationExtensions.cs           -- auto-register DI

DotnetProject.Core/
  Domain/
    Models/
      AxonsCollab.cs, AxonsMember.cs, AxonsProject.cs, FwInit.cs
  Features/
    Collaboration/
      Abstractions/
        ICollabRepository.cs           -- CRUD interface
        ICollabViewReader.cs           -- complex query interface
      Operations/
        CollabFactory.cs               -- domain validation + entity creation
    UserInfo/
      Abstractions/
        IUserProjectViewReader.cs      -- complex query interface
      ProjectRecord.cs                 -- query result record
  Shared/
    FeatureErrors.cs                   -- Error definitions

DotnetProject.Infrastructure/
  Models/
    AxonsCollab.cs
    AxonsMember.cs
    AxonsProject.cs
    FwInit.cs
  Persistence/
    ApplicationDbContext.cs            -- Base DbContext (DbSet definitions)
    Configurations/
      EntityConfigurations.cs          -- EF Core entity configurations

DotnetProject.Infrastructure.Postgresql/
  Persistence/
    PostgresqlApplicationDbContext.cs   -- PostgreSQL-specific DbContext
  Repositories/
    CollabRepository.cs                -- EF Core (CRUD)
  Queries/
    CollabViewReader.cs                -- Dapper (raw SQL)
    UserProjectViewReader.cs           -- Dapper (raw SQL + JOIN)

DotnetProject.Test/                    -- Unit Tests
DotnetProject.IntegrationTest/         -- Integration Tests
```

---

## 4. Railway Oriented Programming (ROP)

### แนวคิด

**ROP** คือรูปแบบการเขียนโค้ดที่มองทุก operation เป็น "ทางรถไฟ 2 ราง":

```
  Success Track (ราง Happy Path)
  ===========================================>  Result.Success(value)
       |           |           |
     Step 1      Step 2      Step 3
       |           |           |
  ===========================================>  Result.Failure(error)
  Failure Track (ราง Error)
```

- ถ้า Step ใดสำเร็จ --> ส่งต่อไป Step ถัดไปบนราง **Success**
- ถ้า Step ใดล้มเหลว --> "สับราง" ลงราง **Failure** ทันที **ข้าม Step ที่เหลือทั้งหมด**
- **ไม่ต้องเขียน `if/else` หรือ `try/catch` ซ้อนกันหลายชั้น**

### ทำไมถึงใช้ ROP ในโปรเจคนี้

| ปัญหาแบบเดิม (MVC) | ROP แก้ปัญหาอย่างไร |
|---|---|
| `if (result == null) return error;` ซ้อนกันหลายชั้น | chain `.Then()` / `.ThenAsync()` -- ล้มเหลวเมื่อไหร่ก็หยุดทันที |
| `try/catch` กระจายทุก layer | error ถูกห่อใน `Result<T>` ส่งต่อโดยไม่ต้อง throw |
| ยากต่อการ trace ว่า error เกิดที่ขั้นตอนไหน | ทุก step มี OpenTelemetry span อัตโนมัติ |
| error format ไม่เป็นมาตรฐาน | `StdResponse` บังคับ format เดียวกันทุก layer |

### ตัวอย่างเปรียบเทียบ

**แบบเดิม (MVC-style / Imperative):**

```csharp
public async Task<Result<GiveStarResultDto>> Handle(GiveStarCommand cmd, CancellationToken ct)
{
    // Step 1: ดึง sequence id
    var idResult = await _collabViewReader.GetNextValueAsync("axons_collab_id_seq", ct);
    if (idResult.IsFailure) return Result<GiveStarResultDto>.Failure(idResult.Error);

    // Step 2: validate + สร้าง entity
    var collabResult = CollabFactory.CreateCollab(_logger, idResult.Value, cmd.Year, ...);
    if (collabResult.IsFailure) return Result<GiveStarResultDto>.Failure(collabResult.Error);

    // Step 3: บันทึกลง database
    var saveResult = await _collabRepository.SaveGiveStar(collabResult.Value, ct);
    if (saveResult.IsFailure) return Result<GiveStarResultDto>.Failure(saveResult.Error);

    // Step 4: map เป็น DTO
    return Result<GiveStarResultDto>.Success(
        new GiveStarResultDto { IsSuccess = true, Id = saveResult.Value.Id });
}
```

**แบบ ROP (Declarative) -- โค้ดจริงในโปรเจค:**

```csharp
// อ้างอิง: GiveStarHandler.cs
public async Task<Result<GiveStarResultDto>> Handle(GiveStarCommand command, CancellationToken ct)
{
    return await InstrumentedResultExtensions
        .BeginTracingAsync(() =>
            _collabViewReader.GetNextValueAsync("axons_collab_id_seq", ct))
        .Then(newId => CollabFactory.CreateCollab(
            _logger, newId,
            command.Year, command.Sprint,
            command.GivenUser, command.GivenFullname,
            command.StarUser, command.StarFullname,
            command.SubTeam, command.Remark))
        .ThenAsync(async saveData =>
            await _collabRepository.SaveGiveStar(saveData, ct))
        .Map(res => Result<GiveStarResultDto>.Success(
            new GiveStarResultDto { IsSuccess = true, Id = res.Id }));
}
```

### Flow Diagram ของ GiveStar

```
BeginTracingAsync(GetNextValueAsync)
        |
        | Success: newId = 42
        v
  Then(CollabFactory.CreateCollab)
        |
        |--- Failure: "Cannot give star to self"  --> Result.Failure(ERR301)  [STOP]
        |
        | Success: AxonsCollab entity
        v
  ThenAsync(SaveGiveStar)
        |
        |--- Failure: "Unique constraint"  --> Result.Failure(ERR204)  [STOP]
        |
        | Success: saved entity
        v
  Map(create GiveStarResultDto)
        |
        v
  Result.Success({ IsSuccess: true, Id: 42 })
```

---

## 5. Instrumented ROP Chain (Tracing)

โปรเจคใช้ `InstrumentedResultExtensions` จาก **FeedCommonLib** ที่ **รวม ROP + OpenTelemetry** เข้าด้วยกัน
ทุก step ใน chain จะสร้าง trace span อัตโนมัติ

### Chain Methods ที่ใช้ได้

| Method | ประเภท | ใช้เมื่อ | ตัวอย่าง |
|--------|--------|---------|----------|
| `BeginTracingAsync` | จุดเริ่มต้น | เริ่ม chain ใหม่ + สร้าง root span | เรียก ViewReader ครั้งแรก |
| `Then` | Sync | แปลง/validate ข้อมูล (ไม่มี await) | `Factory.Create()`, validation |
| `ThenAsync` | Async | เรียก database/service (มี await) | `Repository.Save()` |
| `Map` | Sync | แปลงผลลัพธ์สุดท้ายเป็น DTO | สร้าง `ResultDto` |
| `MapAsync` | Async | แปลงผลลัพธ์สุดท้าย (async) | ถ้าต้อง await ตอน map |
| `TapAsync` | Side-effect | ทำบางอย่างโดยไม่เปลี่ยนค่า | log, send notification |

### หลักการเลือก method

```
ต้องการทำอะไร?
   |
   |-- เริ่มต้น chain ใหม่           --> BeginTracingAsync
   |
   |-- แปลงข้อมูล (sync)             --> Then
   |      เช่น Factory.Create()
   |
   |-- เรียก DB/Service (async)      --> ThenAsync
   |      เช่น repository.Save()
   |
   |-- แปลงเป็น response DTO (sync)  --> Map
   |      เช่น new ResultDto { ... }
   |
   |-- แปลงเป็น response DTO (async) --> MapAsync
   |
   |-- side-effect (log, notify)      --> TapAsync
```

### OpenTelemetry Trace ที่ได้

ทุก method ในสาย chain จะสร้าง span พร้อม tags อัตโนมัติ:

```
Trace: GiveStarHandler.Handle
  |
  +-- [Span] GetNextValueAsync        operation.type=TraceOperation
  |     status: OK
  |
  +-- [Span] CreateCollab              operation.type=Then
  |     status: OK
  |
  +-- [Span] SaveGiveStar              operation.type=ThenAsync
  |     status: OK
  |
  +-- [Span] Mapping GiveStarResultDto operation.type=Map
        status: OK
```

ถ้า step ใดล้มเหลว span จะแสดง `status: ERROR` พร้อม `error.message`

---

## 6. Result Pattern และ StdResponse

### `Result<T>` -- ห่อผลลัพธ์ทุก operation

> อ้างอิง: `FeedCommonLib.Application.Abstractions/Primitives/Result.cs`

```csharp
public class Result<T> : Result
{
    public T Value { get; }

    public static Result<T> Success(T value, SuccessCode? successCode = null);
    public static Result<T> Failure(StdResponse? error);
}
```

| Property | ความหมาย |
|---|---|
| `IsSuccess` | operation สำเร็จหรือไม่ |
| `IsFailure` | ตรงข้าม `IsSuccess` |
| `Value` | ข้อมูลผลลัพธ์ (มีค่าเมื่อ success) |
| `Error` | `StdResponse` object (มีค่าเมื่อ failure) |
| `SuccessResponse` | `StdResponse` object สำหรับ success code (optional) |

### `StdResponse` -- มาตรฐาน error ทุก layer

> อ้างอิง: `FeedCommonLib.Application.Abstractions/ResponseCodes/StdResponse.cs`

```csharp
public class StdResponse
{
    public string Code { get; }           // "ERR301"
    public string Message { get; }        // "You cannot give a star to yourself."
    public int HttpStatusCode { get; }    // 400
    public Dictionary<string, object>? Details { get; }  // exception info
    public object? Data { get; }          // request data ที่ทำให้เกิด error
}
```

**วิธีสร้าง StdResponse:**

```csharp
// จาก Error ที่ลงทะเบียนไว้ -- ใช้กับ business rule
StdResponse error = StdResponse.Create(FeatureErrors.CannotGiveStarToSelf, data: inputData);

// จาก Exception -- ใช้ใน catch block
StdResponse error = StdResponse.FromException(Errors.Database, ex, data: inputData);

// จาก error code string
StdResponse error = StdResponse.Create("ERR301", data: inputData, customMessage: "custom msg");
```

> **เทียบกับ MVC:** แทนที่จะ throw exception แล้ว catch ที่ Controller --
> โปรเจคนี้ห่อ error ไว้ใน `Result.Failure(StdResponse)` แล้วส่งต่อไปเรื่อยๆ

---

## 7. CQRS -- Command / Query Separation

> **เทียบกับ MVC:** แทนที่จะมี Service class เดียวที่ทำทั้งอ่านและเขียน --
> โปรเจคนี้แยก Handler เป็น **Command** (เขียน) กับ **Query** (อ่าน) ชัดเจน

### Command (เขียนข้อมูล)

```csharp
// Request -- ใช้ record + ICommandRequest
public record GiveStarCommand(...) : ICommandRequest<GiveStarResultDto>;

// Handler -- implement ICommandRequestHandler
public class GiveStarHandler : ICommandRequestHandler<GiveStarCommand, GiveStarResultDto>
{
    public async Task<Result<GiveStarResultDto>> Handle(
        GiveStarCommand command, CancellationToken ct) { ... }
}
```

### Query (อ่านข้อมูล)

```csharp
// Request -- ใช้ record + IQueryRequest
public record GetUserProjectQuery(string userName) : IQueryRequest<IEnumerable<ProjectRecord>>;

// Handler -- implement IQueryRequestHandler
public class GetUserProjectHandler : IQueryRequestHandler<GetUserProjectQuery, IEnumerable<ProjectRecord>>
{
    public async Task<Result<IEnumerable<ProjectRecord>>> Handle(
        GetUserProjectQuery query, CancellationToken ct) { ... }
}
```

### Folder Convention

```
Features/
  {FeatureName}/
    Commands/                <-- เขียนข้อมูล (POST, PUT, DELETE)
      {ActionName}/
        {Action}Command.cs
        {Action}CommandValidator.cs
        {Action}Handler.cs
        {Action}ResultDto.cs
    Queries/                 <-- อ่านข้อมูล (GET)
      {ActionName}/
        {Action}Query.cs
        {Action}QueryValidator.cs
        {Action}Handler.cs
```

---

## 8. ViewReader vs Repository

> **กฎสำคัญ:** เลือกชื่อ class ตามลักษณะงาน -- ไม่ใช้ชื่อเดียวกันหมด

| | Repository | ViewReader |
|---|---|---|
| **ชื่อ class** | ลงท้ายด้วย `Repository` | ลงท้ายด้วย `ViewReader` |
| **ใช้เมื่อ** | CRUD, ตารางเดียว, LINQ ง่ายๆ | Query ซับซ้อน, JOIN, raw SQL, aggregate, sequence |
| **เทคโนโลยี** | **EF Core** (LINQ) | **Dapper** (raw SQL) |
| **Folder** | `Infrastructure.Postgresql/Repositories/` | `Infrastructure.Postgresql/Queries/` |
| **Return** | Entity (เช่น `AxonsCollab`) | DTO/Record/scalar (เช่น `ProjectRecord`, `int`) |

### ถามตัวเองก่อนเลือก

```
"ต้องเขียน raw SQL ไหม?"

  ไม่ (LINQ ได้ / CRUD)       -->  Repository   (EF Core)
  ใช่ (JOIN / aggregate)      -->  ViewReader   (Dapper)

"return อะไร?"

  Entity (เช่น AxonsCollab)               -->  Repository
  DTO/Record (เช่น ProjectRecord) / scalar -->  ViewReader
```

### ตัวอย่าง

```
ICollabRepository  -->  CollabRepository (Infrastructure.Postgresql/Repositories/)
  SaveGiveStar()          -- EF Core: AddAsync + SaveChangesAsync
  GetListStarReceive()    -- EF Core: LINQ query ตารางเดียว

ICollabViewReader  -->  CollabViewReader (Infrastructure.Postgresql/Queries/)
  GetNextValueAsync()     -- Dapper: SELECT nextval(sequence)

IUserProjectViewReader  -->  UserProjectViewReader (Infrastructure.Postgresql/Queries/)
  GetProjectsByUserNameAsync()  -- Dapper: JOIN axons_project + axons_member
```

---

## 9. Validation Strategy

โปรเจคใช้ **FluentValidation** มี 2 ระดับ:

### ระดับ 1: Endpoint Validator (input validation)

ตรวจสอบ request เบื้องต้นก่อนถึง Handler -- ใช้ `TracedValidator<T>` เพื่อสร้าง trace span อัตโนมัติ

```csharp
public class GetUserProjectQueryValidator : TracedValidator<GetUserProjectQuery>
{
    public GetUserProjectQueryValidator()
    {
        RuleFor(x => x.userName)
            .NotNull().WithMessage("UserName cannot be null.")
            .NotEmpty().WithMessage("UserName is required.");
    }
}
```

### ระดับ 2: Domain Validation (business rules)

ตรวจสอบ business rules ใน **Factory** -- return `Result.Failure` แทน throw exception

```csharp
// อ้างอิง: CollabFactory.cs
public static Result<AxonsCollab> CreateCollab(ILogger logger, ...)
{
    if (givenUser == starUser)
    {
        StdResponse error = StdResponse.Create(FeatureErrors.CannotGiveStarToSelf);
        logger.Log(error, context: nameof(CreateCollab));
        return Result<AxonsCollab>.Failure(error);  // ไม่ throw, ใช้ Result
    }
    // ...
    return Result<AxonsCollab>.Success(collab);
}
```

---

## 10. Error Code & Success Code System

### โครงสร้าง Error Code

| ช่วง | ประเภท | Log Level | ตัวอย่าง |
|-------|--------|-----------|----------|
| `ERR0xx` | Domain Errors | Warning | `ERR001` NotFound, `ERR002` Validation |
| `ERR1xx` | Application Errors | Warning | `ERR101` Unauthorized, `ERR102` Forbidden |
| `ERR2xx` | Infrastructure Errors | Error | `ERR201` Database, `ERR204` UniqueConstraint |
| `ERR3xx` | Custom/Feature Errors | Warning | `ERR301` CannotGiveStarToSelf |
| `ERR888` | Unknown Error | Error | `ERR888` UnknownError |
| `ERR999` | Unexpected | Critical | Unhandled exception |

### Built-in Errors จาก FeedCommonLib

> อ้างอิง: `FeedCommonLib.Application.Abstractions/ResponseCodes/ErrorCode.cs`

| Code | ชื่อ | HTTP Status | Default Message | ใช้เมื่อ |
|------|------|-------------|---------|---------|
| `ERR001` | `Errors.NotFound` | 404  | The requested resource was not found |หาข้อมูลไม่เจอ |
| `ERR002` | `Errors.Validation` | 400 |  validation failed |
| `ERR003` | `Errors.BusinessRule` | 400 | business rule violation |
| `ERR004` | `Errors.Conflict` | 409 | resource conflict |
| `ERR005` | `Errors.InvalidInput` | 400 | input ไม่ถูกต้อง |
| `ERR006` | `Errors.PreconditionFailed` | 412 | precondition not met |
| `ERR101` | `Errors.Unauthorized` | 401 | ยังไม่ login |
| `ERR102` | `Errors.Forbidden` | 403 | ไม่มีสิทธิ์ |
| `ERR103` | `Errors.RateLimitExceeded` | 429 | too many requests |
| `ERR104` | `Errors.MethodNotAllowed` | 405 | HTTP method ไม่รองรับ |
| `ERR201` | `Errors.Database` | 500 | database error |
| `ERR202` | `Errors.ExternalService` | 502 | external service error |
| `ERR203` | `Errors.Timeout` | 504 | timeout |
| `ERR204` | `Errors.UniqueConstraint` | 409 | unique constraint violation |
| `ERR205` | `Errors.ServiceUnavailable` | 503 | service temporarily unavailable |
| `ERR888` | `Errors.UnknownError` | 400 | unknown error |
| `ERR999` | `Errors.Unexpected` | 500 | unhandled exception |


### Built-in Success Codes จาก FeedCommonLib

| Code | ชื่อ | HTTP Status |
|------|------|-------------|
| `SUC200` | `SuccessCodes.Ok` | 200 |
| `SUC201` | `SuccessCodes.Created` | 201 |
| `SUC204` | `SuccessCodes.NoContent` | 204 |

### ลงทะเบียน Error Code ใหม่ (Feature-specific)

```csharp
// Core/Shared/FeatureErrors.cs
public static readonly Error CannotGiveStarToSelf = Errors.Register(
    "ERR301",                               // unique code
    "You cannot give a star to yourself.",   // default message
    StatusCodes.Status400BadRequest          // HTTP status
);
```

---

## 11. DI Auto-Registration

Handler, Validator, Repository, ViewReader ลงทะเบียนอัตโนมัติทั้งหมด -- **ไม่ต้องเพิ่ม DI เอง**

```csharp
// Program.cs
builder.Services.AddApplicationServices(typeof(ApplicationServiceExtensions).Assembly);
builder.Services.AddInfrastructureServices(typeof(PostgresqlApplicationDbContext).Assembly);
```

| สิ่งที่ scan | เงื่อนไข | Lifetime |
|---|---|---|
| Command Handler | implement `ICommandRequestHandler<>` หรือ `ICommandRequestHandler<,>` + ชื่อลงท้ายด้วย `Handler` | Scoped |
| Query Handler | implement `IQueryRequestHandler<,>` + ชื่อลงท้ายด้วย `Handler` | Scoped |
| Validator | implement `IValidator<>` (scan จาก FluentValidation) | Scoped |
| Repository / ViewReader | concrete class ที่ implement interface ใน namespace `DotnetProject.*` | Scoped |

> **สำคัญ:** แค่สร้าง class ที่ implement interface ที่ถูกต้อง -- DI จะถูก register อัตโนมัติ
> ไม่ต้องเพิ่มโค้ดใน `Program.cs`

---

## 12. API Response Format

ทุก endpoint ใช้ `ApiResponse<T>` format เดียวกัน ผ่าน `result.ToApiResponse()`

### Success Response

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

### Error Response

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


---

## 13. FeedCommonLib -- Shared Library

> อ้างอิง: `FeedCommonLib.Application.Abstractions` project

Library กลางที่ใช้ร่วมกันข้ามหลาย microservice -- **ไม่ต้องแก้ไข** แค่เรียกใช้

| Module | Class / Interface | หน้าที่ |
|--------|-------------------|---------|
| **Primitives** | `Result`, `Result<T>` | Result pattern -- ห่อผลลัพธ์ทุก operation |
| **PrimitivesExtension** | `ResultExtensions` | `.Then()`, `.Map()`, `.TapAsync()` แบบไม่มี trace |
| **ResponseCodes** | `StdResponse` | Standard error/success object |
| **ResponseCodes** | `Error`, `Errors` | Error code registry + built-in errors |
| **ResponseCodes** | `SuccessCode`, `SuccessCodes` | Success code registry |
| **ResponseCodes** | `ErrorLogExtensions` | `logger.Log(StdResponse)` extension method |
| **Messaging** | `ICommandRequest<T>` | Marker interface -- Command request |
| **Messaging** | `ICommandRequestHandler<TCmd, TRes>` | Command Handler interface |
| **Messaging** | `IQueryRequest<T>` | Marker interface -- Query request |
| **Messaging** | `IQueryRequestHandler<TQuery, TRes>` | Query Handler interface |
| **Tracing** | `InstrumentedResultExtensions` | ROP chain + OpenTelemetry tracing |
| **Tracing** | `ActivitySourceProvider` | `ActivitySource` สำหรับ manual instrumentation |
| **EndpointExtension** | `ApiResponse<T>` | Standard API response format |
| **EndpointExtension** | `ApiResponseExtension` | `.ToApiResponse()` extension method |

### วิธีใช้ FeedCommonLib ที่สำคัญ

```csharp
// 1. สร้าง error จาก Error ที่ลงทะเบียนไว้
StdResponse error = StdResponse.Create(FeatureErrors.CannotGiveStarToSelf, data: inputData);

// 2. สร้าง error จาก exception (ใน catch block)
StdResponse error = StdResponse.FromException(Errors.Database, ex, data: inputData);

// 3. Log error (extension method -- log level ถูกกำหนดอัตโนมัติจาก error code)
logger.Log(error, context: nameof(CreateCollab), customMessage: "additional info");

// 4. ROP chain พร้อม OpenTelemetry trace
return await InstrumentedResultExtensions
    .BeginTracingAsync(() => _viewReader.GetDataAsync(...))
    .Then(data => Factory.Create(data))
    .ThenAsync(entity => _repository.SaveAsync(entity))
    .Map(saved => Result<MyDto>.Success(new MyDto { ... }));

// 5. แปลง Result เป็น HTTP Response (ใน Endpoint)
await result.ToApiResponse(HttpContext, Logger);
```

---

## 14. IntegrationTestLib -- Integration Test Library

> อ้างอิง: `IntegrationTestLib` project

Library สำหรับ Integration Test -- ช่วยจัดการ test data, JWT token, และเปรียบเทียบ response

| Class | หน้าที่ |
|-------|---------|
| `IntegrationFunction` | สร้าง JWT token, อ่าน JSON test data, เปรียบเทียบ response |
| `PostgresqlTestDataHelper` | Setup/Cleanup test data ด้วย SQL scripts |
| `JsonModelTest<T, TT>` | Model สำหรับ JSON test file (request + expected response) |
| `CompareResult` | ผลลัพธ์การเปรียบเทียบ response |

### วิธีใช้ใน Integration Test

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
        await _testDataHelper.SetupTestData(
            "UserInfo\\GetUserProject\\Setup\\SetupUserProject.sql");
    }

    [TearDown]
    public async Task TearDown()
    {
        await _testDataHelper.CleanupTestData(
            "UserInfo\\GetUserProject\\TearDown\\TeardownUserProject.sql");
    }

    [Test]
    public async Task GetUserProjectSuccess_Should_GetData()
    {
        // Arrange -- อ่าน request + expected response จาก JSON
        string _filePath = jsonBasePath +
            "UserInfo/GetUserProject/GetUserProjectSuccess_Should_GetData.json";
        var jData = fn.GetJsonObject<GetUserProjectQuery, IEnumerable<ProjectRecord>>(_filePath);

        // Act -- เรียก API
        string url = $"/api/userinfo/project?{jData.QueryString}";
        HttpResponseMessage actualResponse = await Client.GetAsync(url);

        // Assert -- เปรียบเทียบ response กับ expected
        CompareResult compareResult =
            await fn.CompareResponseByObject<IEnumerable<ProjectRecord>>(
                actualResponse, jData.WithResponse);
        Assert.That(compareResult.IsEqual, Is.True, compareResult.Message);
    }
}
```

### IntegrationFunction -- Methods ที่ใช้บ่อย

| Method | คำอธิบาย |
|--------|---------|
| `GenerateJwtToken(userName, issuer)` | สร้าง JWT token สำหรับ test |
| `GetJsonObject<T, TT>(filePath)` | อ่าน JSON test file เป็น `JsonModelTest` |
| `GetJsonObjectWithToken<T, TT>(filePath)` | อ่าน JSON + แนบ JWT token ใน header |
| `CompareResponseByObject<T>(response, expected)` | เปรียบเทียบ response -- return `CompareResult` |

### PostgresqlTestDataHelper -- Methods

| Method | คำอธิบาย |
|--------|---------|
| `SetupTestData(scriptName)` | รัน SQL script เพื่อ insert test data |
| `CleanupTestData(scriptName)` | รัน SQL script เพื่อ delete test data |
