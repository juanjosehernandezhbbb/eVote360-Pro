using Application.Interfaces;
using Application.Services;
using Core.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Persistance.Context;
using Persistance.Ocr;
using Persistance.Repositories;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuracion de base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<eVote360ProContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configuracion de autenticacion
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuario/Login";
        options.AccessDeniedPath = "/Usuario/AccesoDenegado";
    });

// 3. Inyeccion de Dependencias
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

// Registramos el servicio que faltaba
builder.Services.AddScoped<ICandidatoService, CandidatoService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Configuracion del pipeline de la aplicacion
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 5. Activacion de Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();