using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class CinemaDbContext : IdentityDbContext<ApplicationUser>
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options) { }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<MovieActor> MovieActors => Set<MovieActor>();
    public DbSet<MovieDirector> MovieDirectors => Set<MovieDirector>();
    public DbSet<MovieCountry> MovieCountries => Set<MovieCountry>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();

    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionSeat> SessionSeats => Set<SessionSeat>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Countries
        modelBuilder.Entity<Country>(e =>
        {
            e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(2).IsFixedLength();
            e.Property(x => x.Name).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        // People
        modelBuilder.Entity<Person>(e =>
        {
            e.Property(x => x.FirstName).IsRequired();
            e.Property(x => x.LastName).IsRequired();
            e.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength();

            e.HasOne(x => x.Country)
                .WithMany(c => c.People)
                .HasForeignKey(x => x.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Movies
        modelBuilder.Entity<Movie>(e =>
        {
            e.Property(x => x.Title).IsRequired();
            e.Property(x => x.Rating).HasPrecision(4, 1);
            e.Property(x => x.ProductionCountryCode).HasMaxLength(2).IsFixedLength();

            e.HasOne(x => x.Director)
                .WithMany(p => p.DirectedMoviesMain)
                .HasForeignKey(x => x.DirectorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ProductionCountry)
                .WithMany(c => c.ProducedMovies)
                .HasForeignKey(x => x.ProductionCountryCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Genres
        modelBuilder.Entity<Genre>(e =>
        {
            e.Property(x => x.Name).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        // Cinemas
        modelBuilder.Entity<Cinema>(e =>
        {
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Address).IsRequired();
        });

        // Halls
        modelBuilder.Entity<Hall>(e =>
        {
            e.Property(x => x.Name).IsRequired();

            e.HasOne(x => x.Cinema)
                .WithMany(c => c.Halls)
                .HasForeignKey(x => x.CinemaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seats
        modelBuilder.Entity<Seat>(e =>
        {
            e.HasOne(x => x.Hall)
                .WithMany(h => h.Seats)
                .HasForeignKey(x => x.HallId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.HallId, x.RowNumber, x.SeatNumber }).IsUnique();
        });

        // Sessions
        modelBuilder.Entity<Session>(e =>
        {
            e.Property(x => x.StartTime).IsRequired();
            e.Property(x => x.EndTime).IsRequired();

            e.HasOne(x => x.Movie)
                .WithMany(m => m.Sessions)
                .HasForeignKey(x => x.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Hall)
                .WithMany(h => h.Sessions)
                .HasForeignKey(x => x.HallId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Bookings
        modelBuilder.Entity<Booking>(e =>
        {
            e.Property(x => x.UserId).IsRequired();
            e.Property(x => x.TotalAmount).HasPrecision(10, 2);
            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.BookedAt).IsRequired();

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Session)
                .WithMany(s => s.Bookings)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // (id, sessionId) unique (в схеме есть, хоть Id и так PK)
            e.HasIndex(x => new { x.Id, x.SessionId }).IsUnique();
        });

        // SessionSeats
        modelBuilder.Entity<SessionSeat>(e =>
        {
            e.Property(x => x.Price).HasPrecision(10, 2);

            e.HasOne(x => x.Session)
                .WithMany(s => s.SessionSeats)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Seat)
                .WithMany(seat => seat.SessionSeats)
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Booking)
                .WithMany(b => b.SessionSeats)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => new { x.SessionId, x.SeatId }).IsUnique();
            e.HasIndex(x => x.BookingId);
        });

        // MovieActors
        modelBuilder.Entity<MovieActor>(e =>
        {
            e.HasKey(x => new { x.MovieId, x.ActorId });

            e.HasOne(x => x.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(x => x.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Actor)
                .WithMany(p => p.MovieActors)
                .HasForeignKey(x => x.ActorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.MovieId, x.ActorId }).IsUnique();
            e.HasIndex(x => new { x.MovieId, x.CustOrder }).IsUnique();
        });

        // MovieDirectors
        modelBuilder.Entity<MovieDirector>(e =>
        {
            e.HasKey(x => new { x.MovieId, x.DirectorId });

            e.HasOne(x => x.Movie)
                .WithMany(m => m.MovieDirectors)
                .HasForeignKey(x => x.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Director)
                .WithMany(p => p.MovieDirectors)
                .HasForeignKey(x => x.DirectorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.MovieId, x.DirectorId }).IsUnique();
            e.HasIndex(x => new { x.MovieId, x.BillingOrder }).IsUnique();
        });

        // MovieCountries
        modelBuilder.Entity<MovieCountry>(e =>
        {
            e.HasKey(x => new { x.MovieId, x.CountryCode });

            e.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength();

            e.HasOne(x => x.Movie)
                .WithMany(m => m.MovieCountries)
                .HasForeignKey(x => x.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Country)
                .WithMany(c => c.MovieCountries)
                .HasForeignKey(x => x.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.MovieId, x.CountryCode }).IsUnique();
        });

        // MovieGenres
        modelBuilder.Entity<MovieGenre>(e =>
        {
            e.HasKey(x => new { x.MovieId, x.GenreId });

            e.HasOne(x => x.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(x => x.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Genre)
                .WithMany(g => g.MovieGenres)
                .HasForeignKey(x => x.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.MovieId, x.GenreId }).IsUnique();
            e.HasIndex(x => x.MovieId);
            e.HasIndex(x => x.GenreId);
        });
    }
}
