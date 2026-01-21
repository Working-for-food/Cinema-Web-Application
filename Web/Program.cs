using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CinemaDbContext>()
    .AddDefaultTokenProviders();

// ===== DI (DAL) =====
builder.Services.AddScoped<IHallRepository, HallRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();

// ===== DI (BLL) =====
builder.Services.AddScoped<IHallService, HallService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ===== Area route (Admin) =====
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Halls}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
