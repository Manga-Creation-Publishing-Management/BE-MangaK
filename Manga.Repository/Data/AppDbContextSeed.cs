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
        await SeedReadersAsync(dbContext);
        await SeedSeriesAndSchedulesAsync(dbContext);
        await SeedChaptersAsync(dbContext);
        await SeedMangaTasksAndIncomesAsync(dbContext);
    }

   private static async Task SeedUsersAsync(AppDbContext dbContext)
{
    if (await dbContext.Users.AnyAsync())
        return;

    var now = DateTimeOffset.UtcNow;

    var togoroId = Guid.NewGuid();
    var mikaId = Guid.NewGuid();

    var users = new List<User>
    {
        // =========================
        // Admin
        // =========================
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "System",
            Email = "admin.mangak@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0902234506",
            Bio = "Handles account management.",
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        // =========================
        // Editorial Board
        // =========================
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Editorial",
            LastName = "Board",
            Email = "board.mangak@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0902234507",
            Bio = "Oversees final approval and publication planning.",
            Role = UserRole.Editorial,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        // =========================
        // Tantou
        // =========================
        new User
        {
            Id = togoroId,
            FirstName = "Togoro",
            LastName = "Mori",
            Email = "tranmaiconghung@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0901234567",
            Bio = "Official Tantou account.",
            Role = UserRole.Tantou,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        new User
        {
            Id = mikaId,
            FirstName = "Mika",
            LastName = "Ayashi",
            Email = "michael.anderson@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0975633219",
            Bio = "Fantasy editor and content quality.",
            Role = UserRole.Tantou,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        // =========================
        // Mangaka
        // =========================
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Akira",
            LastName = "Kobayashi",
            Email = "nhathanhthinguyen@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0901234565",
            AuthorName = "Akira Koba",
            Bio = "A versatile manga creator known for sci-fi adventures and immersive world-building.",
            Role = UserRole.Mangaka,
            SupervisorId = mikaId,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Haruto",
            LastName = "Sato",
            Email = "haruto.sato@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0901234561",
            AuthorName = "Haru Sato",
            Bio = "A manga artist specializing in action and adventure stories with dynamic illustrations.",
            Role = UserRole.Mangaka,
            SupervisorId = mikaId,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Ren",
            LastName = "Takahashi",
            Email = "tuanle17082k5@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0901234562",
            AuthorName = "Ren Taka",
            Bio = "Passionate about psychological mysteries and creating emotionally complex characters.",
            Role = UserRole.Mangaka,
            SupervisorId = mikaId,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Yuki",
            LastName = "Nakamura",
            Email = "yuki.nakamura@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0901234563",
            AuthorName = "Yuki Naka",
            Bio = "Creates heartwarming slice-of-life and romance manga inspired by everyday Japanese culture.",
            Role = UserRole.Mangaka,
            SupervisorId = togoroId,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Kaito",
            LastName = "Fujimoto",
            Email = "kaito.fujimoto@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0901234564",
            AuthorName = "Kaito Fuji",
            Bio = "Focuses on dark fantasy and supernatural worlds with highly detailed artwork.",
            Role = UserRole.Mangaka,
            SupervisorId = togoroId,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            ResetPasswordCode = 0,
            CreatedAt = now
        },

        // =========================
        // Assistant
        // =========================
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Aoi",
            LastName = "Yamamoto",
            Email = "xuanhieubato@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0902234501",
            Bio = "Provides editorial support and assists with manga publication workflows.",
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
            FirstName = "Daichi",
            LastName = "Mori",
            Email = "daichi.mori@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0902234502",
            Bio = "Coordinates schedules and communicates with artists and publishers.",
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
            FirstName = "Emi",
            LastName = "Shimizu",
            Email = "kimngan21102020@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0902234503",
            Bio = "Helps manage manuscripts, deadlines, and project documentation.",
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
            FirstName = "Riku",
            LastName = "Hayashi",
            Email = "riku.hayashi@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0902234504",
            Bio = "Supports creative teams by organizing assets and tracking progress.",
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
            FirstName = "Nozomi",
            LastName = "Ishikawa",
            Email = "nozomi.ishikawa@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0902234505",
            Bio = "Assists with content reviews and communication between departments.",
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
            FirstName = "Sora",
            LastName = "Okamoto",
            Email = "sora.okamoto@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0902234506",
            Bio = "Handles administrative tasks and ensures smooth production coordination.",
            Role = UserRole.Assistant,
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

    private static async Task SeedReadersAsync(AppDbContext dbContext)
    {
        if (await dbContext.Readers.AnyAsync())
            return;

        var readers = new List<Reader>
        {
            new Reader
            {
                Id = Guid.NewGuid(),
                Name = "Reader Test",
                Email = "reader@manga.com",
                Status = UserStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        await dbContext.Readers.AddRangeAsync(readers);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedSeriesAndSchedulesAsync(AppDbContext dbContext)
    {
        if (await dbContext.Series.AnyAsync())
            return;

        // Lấy User Mangaka để làm người tạo Series
        var mangaka = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "mangaka@manga.com");
        var admin = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "admin@manga.com");

        // Lấy vài Category để gán cho Series
        var actionCategory = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == "Action");
        var fantasyCategory = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == "Fantasy");

        if (mangaka == null || actionCategory == null || fantasyCategory == null) return;

        var series1Id = Guid.NewGuid();
        var series2Id = Guid.NewGuid();

        var seriesList = new List<Series>
        {
            new Series
            {
                Id = series1Id,
                Title = "The Rising Sun",
                Description = "A story about a young samurai.",
                Status = SeriesStatus.Processing,
                CreatedById = mangaka.Id,
                ApprovedById = admin?.Id,
                CoverFile = "rising_sun_cover.jpg",
            },
            new Series
            {
                Id = series2Id,
                Title = "Magic Academy",
                Description = "Life in a world full of magic and mysteries.",
                Status = SeriesStatus.Processing,
                CreatedById = mangaka.Id,
                ApprovedById = admin?.Id,
                CoverFile = "magic_academy_cover.jpg",
            }
        };

        await dbContext.Series.AddRangeAsync(seriesList);

        // Seed CategorySeries (Nhiều - Nhiều)
        var categorySeries = new List<CategorySeries>
        {
            new CategorySeries { SeriesId = series1Id, CategoryId = actionCategory.Id },
            new CategorySeries { SeriesId = series2Id, CategoryId = fantasyCategory.Id }
        };
        await dbContext.CategorySeries.AddRangeAsync(categorySeries);

        // Seed Publishing Schedule
        var schedules = new List<PublishingSchedule>
        {
            new PublishingSchedule
            {
                Id = Guid.NewGuid(), SeriesId = series1Id, PublishDate = DateTime.UtcNow.AddDays(7),
                PublishPeriod = "Weekly"
            },
            new PublishingSchedule
            {
                Id = Guid.NewGuid(), SeriesId = series2Id, PublishDate = DateTime.UtcNow.AddDays(14),
                PublishPeriod = "Bi-Weekly"
            }
        };
        await dbContext.PublishingSchedules.AddRangeAsync(schedules);

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedChaptersAsync(AppDbContext dbContext)
    {
        if (await dbContext.Chapters.AnyAsync())
            return;

        var series1 = await dbContext.Series.FirstOrDefaultAsync(s => s.Title == "The Rising Sun");
        var series2 = await dbContext.Series.FirstOrDefaultAsync(s => s.Title == "Magic Academy");

        if (series1 == null || series2 == null) return;

        var chapters = new List<Chapter>
        {
            new Chapter
            {
                Id = Guid.NewGuid(), SeriesId = series1.Id, ChapterNumber = 1, Title = "A New Beginning",
                Summary = "First step of the journey.", Status = ChapterStatus.Created
            },
            new Chapter
            {
                Id = Guid.NewGuid(), SeriesId = series1.Id, ChapterNumber = 2, Title = "The First Sword",
                Summary = "Training starts.", Status = ChapterStatus.Created
            },
            new Chapter
            {
                Id = Guid.NewGuid(), SeriesId = series2.Id, ChapterNumber = 1, Title = "Enrollment",
                Summary = "Entering the academy.", Status = ChapterStatus.Created
            }
        };

        await dbContext.Chapters.AddRangeAsync(chapters);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedMangaTasksAndIncomesAsync(AppDbContext dbContext)
    {
        if (await dbContext.MangaTasks.AnyAsync())
            return;

        var chapter1 = await dbContext.Chapters.FirstOrDefaultAsync(c => c.Title == "A New Beginning");
        var mangaka = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "mangaka@manga.com");
        var assistant = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "assistant@manga.com");

        if (chapter1 == null || mangaka == null || assistant == null) return;

        var task1Id = Guid.NewGuid();
        var task2Id = Guid.NewGuid();
        var tasks = new List<MangaTask>
        {
            new MangaTask
            {
                Id = task1Id,
                ChapterId = chapter1.Id,
                TaskTitle = "Draw Background for Chap 1",
                TaskDescription = "Detailed village background required.",
                Status = MangaTaskStatus.Available,
                CreatedById = mangaka.Id,
                AssignedToId = assistant.Id
            },
            new MangaTask
            {
                Id = task2Id,
                ChapterId = chapter1.Id,
                TaskTitle = "Inking Characters Chap 1",
                TaskDescription = "Ink all main characters.",
                Status = MangaTaskStatus.Available,
                CreatedById = mangaka.Id,
                AssignedToId = assistant.Id
            }
        };

        await dbContext.MangaTasks.AddRangeAsync(tasks);
        var incomes = new List<Income>
        {
            new Income { Id = Guid.NewGuid(), MangaTaskId = task1Id, Amount = 150.00m, Status = IncomeStatus.Pending },
            new Income { Id = Guid.NewGuid(), MangaTaskId = task2Id, Amount = 200.50m, Status = IncomeStatus.Pending }
        };

        await dbContext.Incomes.AddRangeAsync(incomes);
        await dbContext.SaveChangesAsync();
    }
}