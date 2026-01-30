using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seed;

public static class MovieSessionSeeder
{
    public static async Task SeedAsync(CinemaDbContext db)
    {
        if (await db.Movies.AnyAsync()) return;

        var director = new Person
        {
            FirstName = "Іван",
            LastName = "Коваль",
            CountryCode = null,
            PhotoUrl = null
        };

        db.People.Add(director);
        await db.SaveChangesAsync();

        var movies = new List<Movie>
        {
            new Movie
            {
                Title = "Паддінгтон у Перу",
                OriginalName = "Paddington in Peru",
                ReleaseDate = new DateOnly(2026, 1, 10),
                DirectorId = director.Id,
                Description = "Пригоди доброго ведмедика, який знову вирушає в подорож.",
                Language = "англійська",
                Duration = 105,
                TrailerUrl = null,
                Rating = 7.8m
            },
            new Movie
            {
                Title = "Сонік 3",
                OriginalName = "Sonic the Hedgehog 3",
                ReleaseDate = new DateOnly(2026, 1, 20),
                DirectorId = director.Id,
                Description = "Швидкі пригоди у новій частині франшизи.",
                Language = "англійська",
                Duration = 110,
                TrailerUrl = null,
                Rating = 7.4m
            },
            new Movie
            {
                Title = "Носферату",
                OriginalName = "Nosferatu",
                ReleaseDate = new DateOnly(2026, 2, 1),
                DirectorId = director.Id,
                Description = "Готична історія з атмосферою класичного хорору.",
                Language = "англійська",
                Duration = 122,
                TrailerUrl = null,
                Rating = 7.2m
            },
            new Movie
            {
                Title = "Wicked: Чародійка",
                OriginalName = "Wicked",
                ReleaseDate = new DateOnly(2026, 3, 1),
                DirectorId = director.Id,
                Description = "Фентезі-історія про магію та вибір.",
                Language = "англійська",
                Duration = 130,
                TrailerUrl = null,
                Rating = 7.6m
            }
        };

        db.Movies.AddRange(movies);
        await db.SaveChangesAsync();

        var cinema = new Cinema
        {
            Name = "Кінотеатр Центр",
            Address = "Хрещатик, 1",
            City = "Київ",
            IsDeleted = false
        };

        db.Cinemas.Add(cinema);
        await db.SaveChangesAsync();

        var hall = new Hall
        {
            Name = "Зал 1",
            CinemaId = cinema.Id
        };

        db.Halls.Add(hall);
        await db.SaveChangesAsync();

        var nowMovie1 = movies[0];
        var nowMovie2 = movies[1];

        var sessions = new List<Session>
        {
            new Session
            {
                MovieId = nowMovie1.Id,
                HallId = hall.Id,
                StartTime = DateTime.Now.AddHours(2),
                EndTime = DateTime.Now.AddHours(4),
                PresentationType = PresentationType.TwoD,
                IsCancelled = false
            },
            new Session
            {
                MovieId = nowMovie2.Id,
                HallId = hall.Id,
                StartTime = DateTime.Now.AddDays(1).AddHours(3),
                EndTime = DateTime.Now.AddDays(1).AddHours(5),
                PresentationType = PresentationType.ThreeD,
                IsCancelled = false
            }
        };

        db.Sessions.AddRange(sessions);
        await db.SaveChangesAsync();
    }
}
