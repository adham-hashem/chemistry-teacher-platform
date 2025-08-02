using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using DotNetEnv;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using Infrastructure.Repositories.Implementations.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.Features;

Env.Load();

// Configure Serilog with verbose logging and request logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose() // Increased verbosity for debugging
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Host.UseSerilog();

// Log environment variables for debugging (avoid logging sensitive data in production)
Log.Information("JWT Issuer: {Issuer}, DB: {DB}",
    Environment.GetEnvironmentVariable("CTP_DEV_JWT_ISSUER"),
    Environment.GetEnvironmentVariable("CTPlatform_DEV_DATABASE"));

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CTPlatform API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
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
            new string[] { }
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("CTPlatform_DEV_DATABASE") ??
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Database connection string is not configured."))
        .EnableSensitiveDataLogging() // Caution: Use only in development
        .EnableDetailedErrors()); // Detailed EF Core errors for debugging

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("CTP_DEV_JWT_ISSUER") ?? "default-issuer",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("CTP_DEV_JWT_SECRET") ??
            throw new InvalidOperationException("JWT secret is not configured")))
    };
    // Add JWT debugging events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Error("Authentication failed: {Error}, Path: {Path}",
                context.Exception.Message, context.Request.Path);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Information("Token validated for user: {User}, Path: {Path}",
                context.Principal?.Identity?.Name, context.Request.Path);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
    options.AddPolicy("Teacher", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("StudentOrTeacher", policy => policy.RequireRole("Student", "Teacher"));
});

// Rate limiting (commented out, but included for reference)
// If enabled, check X-Rate-Limit-* headers in responses for debugging
//builder.Services.AddRateLimiter(options =>
//{
//    options.AddFixedWindowLimiter("General", opt =>
//    {
//        opt.PermitLimit = 100;
//        opt.Window = TimeSpan.FromMinutes(1);
//        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//        opt.QueueLimit = 0;
//    });
//    options.AddPolicy("NoLimitAuth", new RateLimitPolicy
//    {
//        Limiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
//            context.Request.Path.StartsWithSegments("/api/Auth/login")
//                ? RateLimitPartition.GetNoLimiter("NoLimitAuth")
//                : RateLimitPartition.GetFixedWindowLimiter("General", _ => new FixedWindowRateLimiterOptions
//                {
//                    PermitLimit = 100,
//                    Window = TimeSpan.FromMinutes(1)
//                }))
//    });
//});

// Use a specific CORS policy for debugging (replace with your frontend URL)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", builder =>
    {
        builder.WithOrigins("http://localhost:5173/")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

// Configure form options for file uploads
//builder.Services.Configure<FormOptions>(options =>
//{
//    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB limit
//});

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamResultRepository, ExamResultRepository>();
builder.Services.AddScoped<ILessonAccessCodeRepository, LessonAccessCodeRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<IHonorRepository, HonorRepository>();

builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IDiscountCodeService, DiscountCodeService>();
builder.Services.AddScoped<IExamResultService, ExamResultService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ILessonAccessCodeService, LessonAccessCodeService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddHttpClient<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IHonorService, HonorService>();

var app = builder.Build();

// Add request logging middleware for debugging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseStaticFiles();
//app.UseRateLimiter();

// Add custom middleware to log authorization failures
app.UseWhen(context => context.Request.Method != "OPTIONS", appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        await next();
        if (context.Response.StatusCode == 403)
        {
            Log.Warning("Authorization failed for user: {User}, Path: {Path}",
                context.User.Identity?.Name, context.Request.Path);
        }
    });
    appBuilder.UseAuthentication();
    appBuilder.UseAuthorization();
});

app.MapControllers();

// Add health check endpoint for debugging
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = new[] { "Teacher", "Student" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    await SeedData.InitializeAsync(scope.ServiceProvider, app.Configuration);
}

app.Run();