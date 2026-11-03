using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinalMVC;
using ProyectoFinalMVC.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<ProyectoFinalMVCContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConexionSQL")));

string? claveJwt = builder.Configuration["Jwt:Clave"];

if (string.IsNullOrEmpty(claveJwt) || Encoding.UTF8.GetByteCount(claveJwt) < 32)
{
    throw new InvalidOperationException(
        "Falta la clave de firma JWT (configurar Jwt:Clave en user-secrets) o mide menos de 256 bits.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// CAPA DE SERVICIOS
builder.Services.AddScoped<IUsuariosServicio, UsuariosServicio>();
builder.Services.AddScoped<ITecnologiasServicio, TecnologiasServicio>();
builder.Services.AddScoped<ITutorialesServicio, TutorialesServicio>();
builder.Services.AddScoped<IComentariosServicio, ComentariosServicio>();
builder.Services.AddScoped<ILikesServicio, LikesServicio>();
builder.Services.AddScoped<IListasServicio, ListasServicio>();
builder.Services.AddScoped<ITutorialGuardadoServicio, TutorialGuardadoServicio>();
builder.Services.AddScoped<IArchivoTutorialServicio, ArchivoTutorialServicio>();
builder.Services.AddScoped<ITokensServicio, TokensServicio>();

var app = builder.Build();


// CARGA INICIAL: lista fija de tecnologías.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProyectoFinalMVCContext>();
    await DatosIniciales.SembrarTecnologiasAsync(db);
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// RUTAS DE LOS CONTROLLERS API ([Route("api/...")])
app.MapControllers();


app.Run();
