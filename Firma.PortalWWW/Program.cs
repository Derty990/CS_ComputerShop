using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Firma.Data.Data;
using Firma.Data.Data.Customers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Firma.PortalWWW.Services;
using Firma.PortalWWW.Interfaces; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FirmaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FirmaContext") ?? throw new InvalidOperationException("Connection string 'FirmaContext' not found.")));

// Konfiguracja uwierzytelniania ciasteczkowego
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

// Konfiguracja autoryzacji 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole(UserRole.Admin.ToString(), UserRole.User.ToString()));
});

//  Rejestracja IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

//  Rejestracja serwisu koszyka
builder.Services.AddScoped<ICartService, CartService>();

//  Konfiguracja sesji
builder.Services.AddDistributedMemoryCache(); // Wymagane dla sesji w pamiêci (domyœlne)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Czas, po którym nieaktywna sesja wygasa
    options.Cookie.HttpOnly = true; // Ciasteczko sesji dostêpne tylko przez HTTP
    options.Cookie.IsEssential = true; // Oznacz ciasteczko sesji jako niezbêdne dla dzia³ania aplikacji (np. zgodnoœæ z RODO)
});

var app = builder.Build();

// Konfiguracja requestu HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kolejnoœæ middleware jest wa¿na
app.UseAuthentication(); // Najpierw uwierzytelnianie
app.UseAuthorization();  // Potem autoryzacja

//  middleware sesji
app.UseSession();     

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();