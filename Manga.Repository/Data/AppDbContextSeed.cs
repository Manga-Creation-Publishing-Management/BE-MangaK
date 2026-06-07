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
        await SeedDemoDataAsync(dbContext);
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
    
    private static async Task SeedDemoDataAsync(AppDbContext dbContext)
{
    // Tránh seed trùng mỗi lần chạy app
    if (await dbContext.Series.AnyAsync(s => s.Title == "Doraemon" || s.Title == "Naruto"))
        return;

    var now = DateTimeOffset.UtcNow;

    var mangaka = await dbContext.Users.FirstAsync(u => u.Email == "mangaka@manga.com");
    var assistant = await dbContext.Users.FirstAsync(u => u.Email == "assistant@manga.com");
    var tantou = await dbContext.Users.FirstAsync(u => u.Email == "tantou@manga.com");
    var board = await dbContext.Users.FirstAsync(u => u.Email == "board@manga.com");
    var reader = await dbContext.Users.FirstAsync(u => u.Email == "reader@manga.com");

    var adventure = await dbContext.Categories.FirstAsync(c => c.Name == "Adventure");
    var comedy = await dbContext.Categories.FirstAsync(c => c.Name == "Comedy");
    var sciFi = await dbContext.Categories.FirstAsync(c => c.Name == "Sci-Fi");
    var action = await dbContext.Categories.FirstAsync(c => c.Name == "Action");
    var fantasy = await dbContext.Categories.FirstAsync(c => c.Name == "Fantasy");

    var doraemonId = Guid.NewGuid();
    var narutoId = Guid.NewGuid();

    var doraemon = new Series
    {
        Id = doraemonId,
        Title = "Doraemon",
        Description = "A comedy and sci-fi manga series about a robotic cat from the future who helps a young boy in everyday life.",
        CoverFile = "https://res.cloudinary.com/demo/image/upload/doraemon-cover.jpg",
        NameFile = "https://res.cloudinary.com/demo/raw/upload/doraemon-namefile.pdf",
        NameFilePublicId = "seed/doraemon-namefile",
        CreatedById = mangaka.Id,
        ReviewedById = tantou.Id,
        ApprovedById = board.Id,
        Status = SeriesStatus.Approved,
        CreatedAt = now
    };

    var naruto = new Series
    {
        Id = narutoId,
        Title = "Naruto",
        Description = "An action and adventure manga series about a young ninja pursuing his dream and protecting his village.",
        CoverFile = "https://res.cloudinary.com/demo/image/upload/naruto-cover.jpg",
        NameFile = "https://res.cloudinary.com/demo/raw/upload/naruto-namefile.pdf",
        NameFilePublicId = "seed/naruto-namefile",
        CreatedById = mangaka.Id,
        ReviewedById = tantou.Id,
        ApprovedById = board.Id,
        Status = SeriesStatus.Approved,
        CreatedAt = now
    };

    await dbContext.Series.AddRangeAsync(doraemon, naruto);

    await dbContext.CategorySeries.AddRangeAsync(
        new CategorySeries { SeriesId = doraemonId, CategoryId = comedy.Id },
        new CategorySeries { SeriesId = doraemonId, CategoryId = sciFi.Id },
        new CategorySeries { SeriesId = doraemonId, CategoryId = adventure.Id },

        new CategorySeries { SeriesId = narutoId, CategoryId = action.Id },
        new CategorySeries { SeriesId = narutoId, CategoryId = adventure.Id },
        new CategorySeries { SeriesId = narutoId, CategoryId = fantasy.Id }
    );

    var doraemonChapter1Id = Guid.NewGuid();
    var doraemonChapter2Id = Guid.NewGuid();
    var narutoChapter1Id = Guid.NewGuid();
    var narutoChapter2Id = Guid.NewGuid();

    var doraemonChapter1 = new Chapter
    {
        Id = doraemonChapter1Id,
        SeriesId = doraemonId,
        ChapterNumber = 1,
        Title = "Chapter 1 - The Future Gadget",
        Summary = "Doraemon introduces a special gadget that changes Nobita's ordinary day.",
        ManuscriptFileUrl = "https://res.cloudinary.com/demo/raw/upload/doraemon-chapter-1.pdf",
        ChapterFileUrl = null,
        Status = ChapterStatus.Processing,
        CreatedAt = now
    };

    var doraemonChapter2 = new Chapter
    {
        Id = doraemonChapter2Id,
        SeriesId = doraemonId,
        ChapterNumber = 2,
        Title = "Chapter 2 - Nobita's Big Problem",
        Summary = "Nobita faces a school problem and asks Doraemon for help.",
        ManuscriptFileUrl = "https://res.cloudinary.com/demo/raw/upload/doraemon-chapter-2.pdf",
        ChapterFileUrl = null,
        Status = ChapterStatus.Created,
        CreatedAt = now
    };

    var narutoChapter1 = new Chapter
    {
        Id = narutoChapter1Id,
        SeriesId = narutoId,
        ChapterNumber = 1,
        Title = "Chapter 1 - Ninja Academy",
        Summary = "Naruto begins his journey as a young ninja with a dream of becoming stronger.",
        ManuscriptFileUrl = "https://res.cloudinary.com/demo/raw/upload/naruto-chapter-1.pdf",
        ChapterFileUrl = null,
        Status = ChapterStatus.Processing,
        CreatedAt = now
    };

    var narutoChapter2 = new Chapter
    {
        Id = narutoChapter2Id,
        SeriesId = narutoId,
        ChapterNumber = 2,
        Title = "Chapter 2 - First Mission",
        Summary = "Naruto and his team prepare for their first important mission.",
        ManuscriptFileUrl = "https://res.cloudinary.com/demo/raw/upload/naruto-chapter-2.pdf",
        ChapterFileUrl = null,
        Status = ChapterStatus.Created,
        CreatedAt = now
    };

    await dbContext.Chapters.AddRangeAsync(
        doraemonChapter1,
        doraemonChapter2,
        narutoChapter1,
        narutoChapter2
    );

    var task1Id = Guid.NewGuid();
    var task2Id = Guid.NewGuid();

    var task1 = new MangaTask
    {
        Id = task1Id,
        TaskTitle = "Draw background for Doraemon chapter 1",
        TaskDescription = "Draw room background and gadget scene for Doraemon chapter 1.",
        Status = MangaTaskStatus.Processing,
        Deadline = now.AddDays(7),
        AssignedAt = now,
        SubmittedAt = null,
        ChapterId = doraemonChapter1Id,
        CreatedById = mangaka.Id,
        AssignedToId = assistant.Id,
        CreatedAt = now
    };

    var task2 = new MangaTask
    {
        Id = task2Id,
        TaskTitle = "Draw action scene for Naruto chapter 1",
        TaskDescription = "Draw ninja training background and action panels for Naruto chapter 1.",
        Status = MangaTaskStatus.Processing,
        Deadline = now.AddDays(10),
        AssignedAt = now,
        SubmittedAt = null,
        ChapterId = narutoChapter1Id,
        CreatedById = mangaka.Id,
        AssignedToId = assistant.Id,
        CreatedAt = now
    };

    await dbContext.MangaTasks.AddRangeAsync(task1, task2);

    await dbContext.Incomes.AddRangeAsync(
        new Income
        {
            Id = Guid.NewGuid(),
            MangaTaskId = task1Id,
            Amount = 100000,
            Date = null,
            Status = IncomeStatus.Pending,
            CreatedAt = now
        },
        new Income
        {
            Id = Guid.NewGuid(),
            MangaTaskId = task2Id,
            Amount = 150000,
            Date = null,
            Status = IncomeStatus.Pending,
            CreatedAt = now
        }
    );

    await dbContext.PublishingSchedules.AddRangeAsync(
        new PublishingSchedule
        {
            Id = Guid.NewGuid(),
            SeriesId = doraemonId,
            DecidedById = board.Id,
            PublishDate = now.AddDays(30),
            PublishPeriod = "Month",
            CreatedAt = now
        },
        new PublishingSchedule
        {
            Id = Guid.NewGuid(),
            SeriesId = narutoId,
            DecidedById = board.Id,
            PublishDate = now.AddDays(45),
            PublishPeriod = "Month",
            CreatedAt = now
        }
    );

    await dbContext.Feedbacks.AddRangeAsync(
        new Feedback
        {
            Id = Guid.NewGuid(),
            SenderId = tantou.Id,
            ReceiverId = mangaka.Id,
            SeriesId = doraemonId,
            Content = "The story direction is clear. Please polish the chapter pacing before publishing.",
            CreatedAt = now
        },
        new Feedback
        {
            Id = Guid.NewGuid(),
            SenderId = mangaka.Id,
            ReceiverId = assistant.Id,
            MangaTaskId = task2Id,
            Content = "Please focus on the action panels and submit before the deadline.",
            CreatedAt = now
        }
    );

    await dbContext.ChapterVotings.AddRangeAsync(
        new ChapterVoting
        {
            Id = Guid.NewGuid(),
            ReaderId = reader.Id,
            ChapterId = doraemonChapter1Id,
            Rate = 5,
            VoteAt = now,
            Status = VoteStatus.Active,
            CreatedAt = now
        },
        new ChapterVoting
        {
            Id = Guid.NewGuid(),
            ReaderId = reader.Id,
            ChapterId = narutoChapter1Id,
            Rate = 4,
            VoteAt = now,
            Status = VoteStatus.Active,
            CreatedAt = now
        }
    );

    await dbContext.Leaderboards.AddRangeAsync(
        new Leaderboard
        {
            Id = Guid.NewGuid(),
            SeriesId = doraemonId,
            RankingPeriod = "2026-06",
            RankPosition = 1,
            TotalVotes = 5,
            CreatedAt = now
        },
        new Leaderboard
        {
            Id = Guid.NewGuid(),
            SeriesId = narutoId,
            RankingPeriod = "2026-06",
            RankPosition = 2,
            TotalVotes = 4,
            CreatedAt = now
        }
    );

    await dbContext.SaveChangesAsync();
}
}