using Application.Interfaces;
using Application.Options;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Data.Seed;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// DB
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
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
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISessionPricingRepository, SessionPricingRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Services (реєструємо concrete + interface на той самий scoped-інстанс)
builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<IMovieService>(sp => sp.GetRequiredService<MovieService>());

builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<IGenreService>(sp => sp.GetRequiredService<GenreService>());

builder.Services.AddScoped<CountryLookupService>();
builder.Services.AddScoped<ICountryLookupService>(sp => sp.GetRequiredService<CountryLookupService>());

builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<IPersonService>(sp => sp.GetRequiredService<PersonService>());

builder.Services.AddScoped<CinemaService>();
builder.Services.AddScoped<ICinemaService>(sp => sp.GetRequiredService<CinemaService>());

builder.Services.AddScoped<HallService>();
builder.Services.AddScoped<IHallService>(sp => sp.GetRequiredService<HallService>());

builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<ISessionService>(sp => sp.GetRequiredService<SessionService>());

builder.Services.AddScoped<SessionLookupService>();
builder.Services.AddScoped<ISessionLookupService>(sp => sp.GetRequiredService<SessionLookupService>());

builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<IBookingService>(sp => sp.GetRequiredService<BookingService>());

// TMDB
builder.Services.Configure<TmdbOptions>(builder.Configuration.GetSection("Tmdb"));
builder.Services.AddScoped<IImportMovieFromTmdb, ImportMovieFromTmdb>();
builder.Services.AddScoped<IMovieImportRepository, MovieImportRepository>();
builder.Services.AddHttpClient<ITmdbClient, TmdbClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<TmdbOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(15);
});

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

// Seed: Countries + Test User
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

    try
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var email = "test@local.com";
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var created = await userManager.CreateAsync(user, "Test1234!");
            if (!created.Succeeded)
            {
                var errors = string.Join("; ", created.Errors.Select(e => e.Description));
                logger.LogError("Test user create failed: {Errors}", errors);
            }
            else
            {
                logger.LogInformation("Test user created: {Email}", email);
            }
        }
        else
        {
            logger.LogInformation("Test user exists: {Email}", email);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Test user seeding failed.");
    }
}

app.Run();
