using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MrTamal.API.Data;
using MrTamal.API.Endpoints;
using MrTamal.API.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Auth JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ReporteService>();
builder.Services.AddSingleton<PdfService>();

// Swagger con JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MrTamal API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http,
        Scheme = "Bearer", BearerFormat = "JWT", In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// CORS para Blazor
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// CORS primero - antes de todo para que funcione aunque haya errores
app.UseCors();

// Migrar DB automáticamente
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        // Asignar SucursalId=1 a registros sin sucursal (datos históricos)
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE \"Ingresos\" SET \"SucursalId\" = 1 WHERE \"SucursalId\" IS NULL");
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE \"Egresos\" SET \"SucursalId\" = 1 WHERE \"SucursalId\" IS NULL");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al migrar la base de datos.");
    }
}

// Swagger siempre activo para verificar endpoints
app.UseSwagger();
app.UseSwaggerUI();

// Servir archivos estáticos (imágenes del menú)
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(wwwrootPath);
Directory.CreateDirectory(Path.Combine(wwwrootPath, "menu-images"));
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Health check para mantener el servicio despierto
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

// Endpoints
app.MapAuthEndpoints();
app.MapCatalogoEndpoints();
app.MapMovimientoEndpoints();
app.MapReporteEndpoints();
app.MapCargaMasivaEndpoints();
app.MapMenuPublicoEndpoints();
app.MapSucursalEndpoints();
app.MapUsuarioEndpoints();
app.MapProyectadoEndpoints();
app.MapBackupEndpoints();

app.Run();
