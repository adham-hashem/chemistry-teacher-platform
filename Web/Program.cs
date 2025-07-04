using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using System.Threading.RateLimiting;
using DotNetEnv;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;

Env.Load();

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog as the logging provider
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// Add Swagger for API documentation
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

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("CTPlatform_DEV_DATABASE") ??
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Database connection string is not configured.")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
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
        ValidAudience = Environment.GetEnvironmentVariable("CTP_DEV_JWT_AUDIENCE") ?? "default-audience",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("CTP_DEV_JWT_SECRET") ??
            throw new InvalidOperationException("JWT secret is not configured")))
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Student", policy =>
        policy.RequireRole("Student")); // Student role
    options.AddPolicy("Teacher", policy =>
        policy.RequireRole("Teacher")); // Teacher role
    options.AddPolicy("StudentOrTeacher", policy =>
        policy.RequireRole("Student", "Teacher")); // Allow both roles
});

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("General", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register Repositories
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamResultRepository, ExamResultRepository>();
builder.Services.AddScoped<ILessonAccessCodeRepository, LessonAccessCodeRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

// Register Services
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IExamResultService, ExamResultService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ILessonAccessCodeService, LessonAccessCodeService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddHttpClient<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed roles and data
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

    // Seed initial data (assuming SeedData is defined elsewhere)
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();