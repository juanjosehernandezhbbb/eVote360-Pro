using Application.Interfaces;
using Application.Services;
using Application.Settings;
using Core.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Persistance.Context;
using Persistance.Ocr;
using Persistance.Repositories;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuracion de Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<eVote360ProContext>(options => options.UseSqlServer(connectionString));

// Configuracion de EmailSettings para el servicio de correos
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configuracion de Autenticacion
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Usuario/Login";
        options.AccessDeniedPath = "/Home/Index";
    });

// Configuracion de Sesion
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Inyeccion de Dependencias
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IOcrService, TesseractOcrService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICiudadanoService, CiudadanoService>();
builder.Services.AddScoped<IPartidoPoliticoService, PartidoPoliticoService>();
builder.Services.AddScoped<IPuestoElectivoService, PuestoElectivoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAsignacionDirigenteService, AsignacionDirigenteService>();
builder.Services.AddScoped<IAlianzaPoliticaService, AlianzaPoliticaService>();
builder.Services.AddScoped<IAsignacionCandidatoPuestoService, AsignacionCandidatoPuestoService>();
builder.Services.AddScoped<IEleccionService, EleccionService>();
builder.Services.AddScoped<IElectorService, ElectorService>();
builder.Services.AddScoped<ICandidatoService, CandidatoService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuracion del Pipeline de Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// EL ORDEN ES CRITICO PARA AUTENTICACION Y SESION
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();