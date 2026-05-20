using MudBlazor.Services;
using MroPlan;
using MroPlan.Components;
using MroPlan.Data;
using Microsoft.EntityFrameworkCore;
using MroPlan.Services;
using MroPlan.Models;
using MroPlan.Models.Enums;
using Microsoft.EntityFrameworkCore.Diagnostics;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();



// Database connection — Factory pattern Blazor Server concurrency sorununu önler
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Custom Services
builder.Services.AddScoped<IPersonelService, PersonelService>();
builder.Services.AddScoped<IBakimService, BakimService>();
builder.Services.AddScoped<IYetkinlikService, YetkinlikService>();
builder.Services.AddScoped<IYillikPlanlamaService, YillikPlanlamaService>();

// Bildirim sistemi
builder.Services.AddSingleton<MroPlan.Services.BildirimServisi>();
builder.Services.AddHostedService<MroPlan.Services.BildirimArkaplanServisi>();

// Zaman yönetimi (tatil API + mola + setup hesabı)
builder.Services.AddHttpClient<MroPlan.Services.ZamanYonetimServisi>(c =>
{
    c.BaseAddress = new Uri("https://date.nager.at/api/v3/");
    c.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<MroPlan.Services.ZamanYonetimServisi>();

// Gemini AI Servisi
builder.Services.AddHttpClient<MroPlan.Services.GeminiService>(c =>
{
    var baseUrl = builder.Configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/";
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = TimeSpan.FromSeconds(25);
});

var app = builder.Build();

// Otomatik migration — uygulama başladığında bekleyen migration'ları uygula
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
// Static dosyalar için .glb desteği ekle
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";
provider.Mappings[".gltf"] = "model/gltf+json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
app.UseAntiforgery();

app.MapRazorComponents<MroPlan.App>()
    .AddInteractiveServerRenderMode();

app.Run();
