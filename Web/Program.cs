using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Data.Seed;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CinemaDbContext>()
    .AddDefaultTokenProviders();

// Repositories
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ICinemaRepository, CinemaRepository>();
builder.Services.AddScoped<IHallRepository, HallRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();

// Services
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<ICountryLookupService, CountryLookupService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<ICinemaService, CinemaService>();
builder.Services.AddScoped<IHallService, HallService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Area route (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Countries
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
        await CountrySeeder.SeedAsync(db);
        logger.LogInformation("Countries seeded/updated.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Countries seeding failed.");
    }
}

app.Run();
