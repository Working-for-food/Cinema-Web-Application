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
using Web.Helpers;



var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
}).AddRazorOptions(options =>
{
    options.AreaViewLocationFormats.Clear();
    options.AreaViewLocationFormats.Add("/Views/{2}/{1}/{0}.cshtml"); 
    options.AreaViewLocationFormats.Add("/Views/{2}/Shared/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
});

builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DB
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<CinemaDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<UkrainianIdentityErrorDescriber>();

builder.Services.AddTransient<Application.Interfaces.IEmailService, Application.Services.EmailService>();

// Repositories
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ICinemaRepository, CinemaRepository>();
builder.Services.AddScoped<IHallRepository, HallRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IAfishaRepository, AfishaRepository>();
builder.Services.AddScoped<IUserMovieRepository, UserMovieRepository>();
builder.Services.AddScoped<IAfishaService, AfishaService>();
builder.Services.AddScoped<IMoviePublicService, MoviePublicService>();
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

app.UseSession();

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

// Seed Countries and Roles + Test User
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");

    try
    {
        var db = services.GetRequiredService<CinemaDbContext>();
        await CountrySeeder.SeedAsync(db);
        await MovieSessionSeeder.SeedAsync(db);

        logger.LogInformation("Countries seeded/updated.");

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        var configuration = services.GetRequiredService<IConfiguration>();
        await Infrastructure.Data.Seed.RoleInitializer.InitializeAsync(userManager, roleManager, configuration);
        logger.LogInformation("Roles and SuperAdmin seeded.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
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
