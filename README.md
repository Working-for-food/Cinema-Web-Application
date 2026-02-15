# Cinema Web Application

A Ukrainian-language cinema management and ticket-booking web application built with ASP.NET Core MVC, Entity Framework Core, ASP.NET Core Identity, and SQL Server.
The application provides a public cinema catalogue and schedule, authenticated seat booking, personal saved movies, and an administrative dashboard for managing cinema content and operations.

### Features
#### For visitors and registered users:
- Browse movies that are currently showing or coming soon
- View movie information, posters, ratings, genres, cast, directors, and trailers
- Browse cinema schedules and available sessions
- Filter sessions by cinema, movie, date, and presentation type
- Register and sign in using ASP.NET Core Identity
- Confirm accounts through email
- Save movies to a personal list
- Select seats and create bookings
- View and cancel personal bookings
- Generate QR codes for booking information
#### For administrators
- Manage movies, genres, people, cinemas, halls, and seats
- Create and manage cinema sessions
- Import movie data from TMDB
- Retrieve external ratings from OMDb
- Upload media through Cloudinary
- Configure row prices and seat-category multipliers
- Create reusable pricing templates
- View booking and revenue statistics
- Export statistics to Excel
- Manage users and roles
- Automatically seed countries, roles, and an initial administrator


### Technologies
- C#
- .NET 9
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- TMDB API
- OMDb API
- Cloudinary

---

### How to Run
Requirements:
- .NET 9 SDK
- SQL Server LocalDB

Clone the project:
- `git clone https://github.com/Working-for-food/Cinema-Web-Application.git`
- `cd Cinema-Web-Application`

Restore the project and its tools:
- `dotnet restore`
- `dotnet tool restore`

Create the local database and apply migrations:
- `dotnet ef database update --project Infrastructure --startup-project Web`

Run the application:
- `dotnet run --project Web`