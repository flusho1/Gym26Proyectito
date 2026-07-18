using DevExpress.Blazor;
using Gym26.Components;
using Gym26.Services; // Asegúrate de que este namespace sea correcto
using Npgsql;
using System.Data;
using Gym26.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 1. Agregar servicios de componentes Razor e interactividad
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Configuración de DevExpress 24.1
// Es recomendable agregar el soporte para el tema moderno aquí
builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = BootstrapVersion.v5;
    options.SizeMode = SizeMode.Medium;
});

// 3. Servicios de datos
builder.Services.AddSingleton<WeatherForecastService>();
// Agrega esta línea en Program.cs antes de var app = builder.Build();
builder.Services.AddScoped<GymService>();

builder.Services.AddTransient<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<SesionService>();

builder.Services.AddScoped<UserSession>();

builder.Services.AddScoped<GymService>();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromDays(7); 
});

var app = builder.Build();
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// 4. IMPORTANTE: Los archivos estáticos deben estar antes de MapRazorComponents
app.UseStaticFiles();
app.UseAntiforgery();

// 5. Mapeo de componentes interactivos
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();