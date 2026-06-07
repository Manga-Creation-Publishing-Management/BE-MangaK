using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace Manga.Repository.Data;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        await SeedUsersAsync(dbContext);
        await SeedCategoriesAsync(dbContext);
    }

    private static async Task SeedUsersAsync(AppDbContext dbContext)
    {
        if (await dbContext.Users.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "System",
                Email = "admin@manga.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                Verified = true,
                VerifiedCode = 0,
                ResetPasswordCode = 0,
                CreatedAt = now
            },

            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Manga",
                LastName = "Creator",
                Email = "mangaka@manga.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mangaka@123"),
                Role = UserRole.Mangaka,
                Status = UserStatus.Active,
                Verified = true,
                VerifiedCode = 0,
                ResetPasswordCode = 0,
                AuthorName = "Moon Cat",
                Bio = "Test mangaka account",
                CreatedAt = now
            },

            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Assistant",
                LastName = "One",
                Email = "assistant@manga.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Assistant@123"),
                Role = UserRole.Assistant,
                Status = UserStatus.Active,
                Verified = true,
                VerifiedCode = 0,
                ResetPasswordCode = 0,
                CreatedAt = now
            },

            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Tantou",
                LastName = "Editor",
                Email = "tantou@manga.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tantou@123"),
                Role = UserRole.TantouEditor,
                Status = UserStatus.Active,
                Verified = true,
                VerifiedCode = 0,
                ResetPasswordCode = 0,
                CreatedAt = now
            },

            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Editorial",
                LastName = "Board",
                Email = "board@manga.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Board@123"),
                Role = UserRole.EditorialBoard,
                Status = UserStatus.Active,
                Verified = true,
                VerifiedCode = 0,
                ResetPasswordCode = 0,
                CreatedAt = now
            },

            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Reader",
                LastName = "Test",
                Email = "reader@manga.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Reader@123"),
                Role = UserRole.Reader,
                Status = UserStatus.Active,
                Verified = true,
                VerifiedCode = 0,
                ResetPasswordCode = 0,
                CreatedAt = now
            }
        };

        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(AppDbContext dbContext)
    {
        if (await dbContext.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Action"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Adventure"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Fantasy"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Romance"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Comedy"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Drama"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Horror"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Mystery"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "School Life"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Slice of Life"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Sports"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Sci-Fi"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Supernatural"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Historical"
            }
        };

        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.SaveChangesAsync();
    }
}
