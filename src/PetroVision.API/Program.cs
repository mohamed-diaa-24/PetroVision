using PetroVision.API.Filters;
using PetroVision.API.Hubs;
using PetroVision.API.Extensions;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PetroVision.API.Middleware;
using PetroVision.API.Services;
using PetroVision.BLL;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddBusinessLayer(builder.Configuration);
builder.Services.AddCurrentUser();
builder.Services.AddScoped<ApiKeyAuthFilter>();
builder.Services.AddSingleton<IEmailSender, LoggingEmailSender>();

// ---------- Real-time (SignalR) ----------
// The hub protocol has its own JSON options: without the converter, enums would arrive as numbers
// while REST sends them as strings.
builder.Services.AddSignalR()
    .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<IEventNotifier, SignalREventNotifier>();
builder.Services.AddAppRateLimits();

// ---------- JWT ----------
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
if (jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters. Set it with user-secrets.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };

        // A valid signature is not enough: the account must still be active and the
        // token's security stamp must match (it changes on permission/status/password changes).
        options.Events = new JwtBearerEvents
        {
            // Browsers cannot send an Authorization header on WebSockets / Server-Sent Events,
            // so the SignalR client passes the token as ?access_token=... (only accepted for /hubs).
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && ctx.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;

                return Task.CompletedTask;
            },

            OnTokenValidated = async ctx =>
            {
                var validator = ctx.HttpContext.RequestServices.GetRequiredService<ISessionValidator>();
                var sub = ctx.Principal?.FindFirst("sub")?.Value;
                var stamp = ctx.Principal?.FindFirst("stamp")?.Value;

                if (!int.TryParse(sub, out var userId) || !await validator.ValidateAsync(userId, stamp))
                    ctx.Fail("Session is no longer valid. Please log in again.");
            }
        };
    });

builder.Services.AddAppAuthorization();

// ---------- CORS (development: allow the frontend html to call the api) ----------
builder.Services.AddCors(o => o.AddPolicy("Frontend", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.Services.InitializeDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<EventsHub>("/hubs/events");

app.Run();
