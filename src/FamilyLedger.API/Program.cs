using System.Text;
using System.Text.Json.Serialization;
using FamilyLedger.API.Middleware;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Application.Mappings;
using FamilyLedger.Application.Services;
using FluentValidation;
using FamilyLedger.Application.Validators;
using FamilyLedger.Infrastructure.Data;
using FamilyLedger.Infrastructure.Repositories;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IMonthlyRecordRepository, MonthlyRecordRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
builder.Services.AddScoped<IAdminStatsRepository, AdminStatsRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("EditorOrOwner", p => p.RequireClaim("role", "editor", "owner"));
    o.AddPolicy("OwnerOnly", p => p.RequireClaim("role", "owner"));
    o.AddPolicy("SuperAdminOnly", p => p.RequireClaim("is_super_admin", "true"));
});

builder.Services.AddValidatorsFromAssemblyContaining<LogTransactionRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FamilyLedger API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

builder.Services.AddCors(o => o.AddPolicy("UIClients", p =>
    p.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyMethod()
        .AllowAnyHeader()));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("UIClients");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
