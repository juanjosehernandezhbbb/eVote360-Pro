using Application.Interfaces;
using Application.Services;
using Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistance.Context;
using Persistance.Ocr;
using Persistance.Repositories;
using Shared.Services;


var builder = WebApplication.CreateBuilder(args);

// Inyección del DbContext de Entity Framework
builder.Services.AddDbContext<eVote360ProContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<eVote360ProContext>(options =>
    options.UseSqlServer(connectionString));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
