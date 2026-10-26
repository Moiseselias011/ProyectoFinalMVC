using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC;
using ProyectoFinalMVC.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddDbContext<ProyectoFinalMVCContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConexionSQL")));

// AUTENTICACIÓN POR COOKIES (la usan el Login y el Logout)
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
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
