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
        new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Mangaka",
            LastName = "System",
            Email = "mangaksystem.admin@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "0865326785",
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

        var users = await dbContext.Users.ToListAsync();
        var board = users.FirstOrDefault(u => u.Role == UserRole.Editorial);
        var categories = await dbContext.Categories.ToListAsync();

        var seriesList = new List<Series>();
        var categorySeriesList = new List<CategorySeries>();
        var schedules = new List<PublishingSchedule>();

        var addSeries = (Guid id, string title, string desc, string email, string coverFile, string nameFile, string nameFilePublicId,SeriesStatus status, string createAt, string pubDateStr, string period, params string[] cats) => {
            var mangaka = users.FirstOrDefault(u => u.Email == email);
            DateTimeOffset created = DateTimeOffset.Parse(createAt + "T00:00:00Z");
            DateTimeOffset? pubDate = string.IsNullOrEmpty(pubDateStr) ? null : DateTimeOffset.Parse(pubDateStr + "T00:00:00Z");
            seriesList.Add(new Series {
                Id = id, Title = title, Description = desc, Status = status,
                CreatedById = mangaka?.Id ?? Guid.Empty,
                ApprovedById = board?.Id,
                ReviewedById = mangaka?.SupervisorId,
                CoverFile = "cover.jpg",
                CreatedAt = created,
                UpdatedAt = pubDate
            });
            foreach(var cat in cats) {
                var c = categories.FirstOrDefault(x => x.Name == cat);
                if(c != null) categorySeriesList.Add(new CategorySeries { SeriesId = id, CategoryId = c.Id });
            }
            if(pubDate.HasValue && !string.IsNullOrEmpty(period)) {
                schedules.Add(new PublishingSchedule {
                    Id = Guid.NewGuid(), SeriesId = id, PublishDate = pubDate.Value, PublishPeriod = period, DecidedById = board?.Id, CreatedAt = created, UpdatedAt = pubDate
                });
            }
        };

        addSeries(Guid.NewGuid(), "Berserk", "A dark fantasy masterpiece following Guts, a lone swordsman seeking revenge against the God Hand after a tragic betrayal.", "nhathanhthinguyen@gmail.com", "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571598/Berserk_cover_kuwolf.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784572575/Berserk_Name_za18ip.pdf",
            "Berserk_Chapter1_Final_dzhogs",
             SeriesStatus.Publishing, "2026-02-12", "2026-03-20", "Monthly", "Fantasy", "Action", "Horror");
        addSeries(Guid.NewGuid(), "Vinland Saga", "Historical manga about Thorfinn's journey from revenge to redemption during the Viking era.", "haruto.sato@gmail.com",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574973/Vinland_Saga_Cover_rhaup6.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574976/Vinland_Saga_Name_pp220a.pdf",
            "Vinland_Saga_Name_pp220a",
            SeriesStatus.Publishing, "2026-03-01", "2026-04-15", "Monthly", "Historical", "Action");
        addSeries(Guid.NewGuid(), "Land of the Lustrous", "Fantasy manga featuring immortal gemstone beings fighting mysterious invaders from the moon.", "tuanle17082k5@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784579147/Land_of_the_Lustrous_Cover_eumo5s.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784579154/Land_of_the_Lustrous_Name_jzyzgu.pdf",
            "Land_of_the_Lustrous_Name_jzyzgu"
            ,SeriesStatus.Publishing, "2026-03-02", "2026-04-14", "Monthly", "Fantasy", "Action");
        addSeries(Guid.NewGuid(), "Blue Period", "A coming-of-age story about Yatora discovering his passion for painting and pursuing art school.", "yuki.nakamura@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573494/BluePeriod_cover_uakozb.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573613/Blue_Period_name_edtacb.pdf",
            "Blue_Period_name_edtacb",
            SeriesStatus.Publishing, "2026-03-14", "2026-04-24", "Monthly", "Slice of Life", "School Life");
        addSeries(Guid.NewGuid(), "The Ancient Magus' Bride", "Fantasy romance following Chise Hatori after becoming the apprentice and bride of the mysterious mage Elias.", "kaito.fujimoto@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627707/The_Ancient_Magus_Bride_cover_ymybxl.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573796/The_Ancient_Magus_Bride_name_orjk7s.pdf",
            "The_Ancient_Magus_Bride_name_orjk7s",
            SeriesStatus.Publishing, "2026-02-03", "2026-03-16", "Monthly", "Fantasy", "Romance");
        addSeries(Guid.NewGuid(), "One Piece", "Monkey D. Luffy sails across the Grand Line searching for the legendary One Piece treasure to become the Pirate King.", "nhathanhthinguyen@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573392/OnePiece_cover_g61dgt.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573394/OnePiece_name_x9nxoo.pdf",
            "OnePiece_name_x9nxoo",
            SeriesStatus.Publishing, "2026-06-06", "2026-06-20", "Weekly", "Adventure", "Action", "Comedy");
        addSeries(Guid.NewGuid(), "Kingdom", "Epic historical manga following Xin's dream of becoming the greatest general under Qin Shi Huang.", "haruto.sato@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784566427/Kingdom_Cover_cmrkqa.png",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784566428/Kingdom_Chapter1_Name_hkvzco.pdf",
            "Kingdom_Chapter1_Name_hkvzco",
            SeriesStatus.Publishing, "2026-06-15", "2026-06-29", "Weekly", "Historical", "Action", "Drama");
        addSeries(Guid.NewGuid(), "Blue Lock", "Japan's top young strikers compete in an intense survival program to create the world's greatest forward.", "tuanle17082k5@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613378/Bluelock_Cover_gtsxhs.jpg.",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613380/Bluelock_Name_p3l01a.pdf",
            "Bluelock_Name_p3l01a",
            SeriesStatus.Publishing, "2026-06-18", "2026-07-01", "Weekly", "Sports", "Drama");
        addSeries(Guid.NewGuid(), "Hajime no Ippo", "A boxing manga chronicling Ippo Makunouchi's rise from a bullied student to a professional boxer.", "yuki.nakamura@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575298/HajimeNoIppo_Cover_naev4r.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575327/HajimeNoIppo_Name_xdlop6.pdf",
            "HajimeNoIppo_Name_xdlop6",
            SeriesStatus.Publishing, "2026-06-24", "2026-07-10", "Weekly", "Sports", "Action");
        addSeries(Guid.NewGuid(), "Frieren: Beyond Journey's End", "After defeating the Demon King, the elf mage Frieren begins a new journey to understand humanity.", "nhathanhthinguyen@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575279/FrierenBeyondJourneysEnd_Cover_pjj5z9.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575280/FrierenBeyondJourneysEnd_Name_crb3ll.pdf",
            "FrierenBeyondJourneysEnd_Name_crb3ll",
            SeriesStatus.Processing, "2026-07-14", "", "", "Fantasy", "Adventure");
        addSeries(Guid.NewGuid(), "20th Century Boys", "A suspense manga where childhood friends uncover a conspiracy connected to a mysterious cult leader known as Friend.", "haruto.sato@gmail.com",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784620288/Monster_Cover_kbkfgu.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784620337/Monster_Name_pyn3cl.pdf",
            "Monster_Name_pyn3cl",
            SeriesStatus.Approved, "2026-07-18", "", "", "Mystery", "Drama", "Sci-Fi");
        addSeries(Guid.NewGuid(), "Oyasumi PunPun", "A newly submitted psychological coming-of-age manga.", "tuanle17082k5@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784620352/20thCenturyBoys_Cover_erzci8.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784620388/20CenturiesBoys_Name_gqrapm.pdf",
            "20CenturiesBoys_Name_gqrapm",
            SeriesStatus.Processing, "2026-07-20", "", "", "Drama", "Slice of Life");
        addSeries(Guid.NewGuid(), "Dandadan", "New action comedy series submitted for approval workflow testing", "yuki.nakamura@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784620980/OyasumiPunPun_Cover_i1iftz.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784620980/OyasumiPunPun_Cover_i1iftz.jpg",
            "OyasumiPunPun_Cover_i1iftz",
            SeriesStatus.Processing, "2026-07-18", "", "", "Action", "Comedy", "Supernatural");
        addSeries(Guid.NewGuid(), "Kaiju No. 8", "New science fiction action manga prepared for review, reject annotation and approval scenarios.", "haruto.sato@gmail.com", 
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784621632/Dandadan_Cover_hw9hos.jpg",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784621642/Dandadan_Name_pa12im.pdf",
            "Dandadan_Name_pa12im",
            SeriesStatus.Processing, "2026-07-16", "", "", "Sci-Fi", "Action");

        await dbContext.Series.AddRangeAsync(seriesList);
        await dbContext.CategorySeries.AddRangeAsync(categorySeriesList);
        await dbContext.PublishingSchedules.AddRangeAsync(schedules);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedChaptersAsync(AppDbContext dbContext)
    {
        if (await dbContext.Chapters.AnyAsync())
            return;

        var seriesDb = await dbContext.Series.Include(s => s.PublishingSchedule).ToListAsync();
        var chapters = new List<Chapter>();

        var addChapter = (string seriesTitle,int chapterNumber ,string manuscriptFileUrl, string chapterFileUrl, string title, string summary, int page, ChapterStatus status) => {
            var s = seriesDb.FirstOrDefault(x => x.Title == seriesTitle);
            if(s != null && s.PublishingSchedule != null) {
                var datePublic = s.PublishingSchedule.PublishDate;
                var period = s.PublishingSchedule.PublishPeriod;
                DateTimeOffset publishDateOfChap;
                if(period == "weekly") {
                    publishDateOfChap = datePublic.AddDays(7 * (chapterNumber - 1));
                } else {
                    publishDateOfChap = datePublic.AddMonths(chapterNumber - 1);
                }
                DateTimeOffset deadline = period == "weekly" ? publishDateOfChap.AddDays(-2) : publishDateOfChap.AddMonths(-1);
                DateTimeOffset createdAt = period == "weekly" ? publishDateOfChap.AddDays(-7) : publishDateOfChap.AddMonths(-1);
                
                chapters.Add(new Chapter {
                    Id = Guid.NewGuid(), SeriesId = s.Id, ChapterNumber = chapterNumber, Title = title, Summary = summary, TotalPage = page, Status = status,
                    ManuscriptFileUrl = manuscriptFileUrl, ChapterFileUrl = chapterFileUrl,
                    Deadline = deadline, CreatedAt = createdAt, UpdatedAt = createdAt
                });
            }
        };

        addChapter("Berserk", 1,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784572574/Berserk_Chapter1_Manu_y2wwzd.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571609/Berserk_Chapter1_Final_dzhogs.pdf",
            "Branded by Fate", "A lone warrior walks a blood-soaked path, defying fate with every step.", 15, ChapterStatus.Publishing);
        addChapter("Berserk", 2, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784572578/Berserk_Chapter2_Manu_fqvbaa.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571600/Berserk_Chapter2_Final_dwwsab.pdf",
            "Echoes of the Eclipse", "Haunted by betrayal, he fights against monsters born from darkness.", 15, ChapterStatus.Publishing);
        addChapter("Berserk", 3,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784572579/Berserk_Chapter3_Manu_ahfikb.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571601/Berserk_Chapter3_Final_bf9phi.pdf", 
            "The Black Swordsman's Oath", "In a world ruled by despair, only the strongest survive.", 15, ChapterStatus.Publishing);
        addChapter("Berserk", 4, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784572587/Berserk_Chapter4_Manu_g3rppf.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571600/Berserk_Chapter4_Final_ensg9z.pdf",
            "Beyond the Crimson Horizon", "Every scar tells a story of loss, vengeance, and relentless determination.", 15, ChapterStatus.Publishing);
        addChapter("Berserk", 5, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784572575/Berserk_Chapter5_Manu_cfpotl.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571601/Berserk_Chapter5_Final_sgf4ri.pdf",
            "Into the Abyss", "When destiny becomes a curse, the sword is the only answer.", 15, ChapterStatus.Publishing);
        addChapter("Vinland Saga", 1,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784574976/Vinland_Saga_Chapter1_Manu_typvmx.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574992/Vinland_Saga_Chapter1_Final_ocktah.pdf", 
            "The Call of the Sea", "A young warrior begins a journey driven by vengeance and ambition.", 15, ChapterStatus.Publishing);
        addChapter("Vinland Saga", 2, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574983/Vinland_Saga_Chapter2_Manu_mft6ur.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575237/Vinland_Saga_Chapter2_Final_lb7c7r.pdf",
            "Ashes of Revenge", "Every battle brings him closer to the truth about strength and honor.", 15, ChapterStatus.Publishing);
        addChapter("Vinland Saga", 3,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784574985/Vinland_Saga_Chapter3_Manu_rdf0gg.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575240/Vinland_Saga_Chapter3_Final_aqxh2h.pdf", 
            "A Warrior's Resolve", "The weight of hatred slowly gives way to the search for peace.", 15, ChapterStatus.Publishing);
        addChapter("Vinland Saga", 4,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784577026/Vinland_Saga_Chapter4_Manu_v9u0yi.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784577031/Vinland_Saga_Chapter4_Final_qtfqkx.pdf", 
            "The Land Beyond War", "Beyond the endless wars lies the dream of a land called Vinland.", 13, ChapterStatus.Publishing);
        
        addChapter("Land of the Lustrous", 1,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784579150/Land_of_the_Lustrous_Chapter1_Manu_lgyclv.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784579152/Land_of_the_Lustrous_Chapter1_Final_trjahb.pdf", 
            "Shattered Brilliance", "A fragile gem begins to change after a devastating encounter.", 13, ChapterStatus.Publishing);
        addChapter("Land of the Lustrous", 2, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784579150/Land_of_the_Lustrous_Chapter2_Manu_fxmszm.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784579152/Land_of_the_Lustrous_Chapter2_Final_kloqwf.pdf", 
            "Moonlit Fragments", "Hidden truths emerge beneath the cold light of the moon.", 13, ChapterStatus.Publishing);
        addChapter("Land of the Lustrous", 3,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784579150/Land_of_the_Lustrous_Chapter3_Manu_dihzbz.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784579152/Land_of_the_Lustrous_Chapter3_Final_uz4amn.pdf", 
            "The Weight of Eternity", "Immortality becomes a burden as memories and losses accumulate.", 13, ChapterStatus.Publishing);
        addChapter("Land of the Lustrous", 4, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784579149/Land_of_the_Lustrous_Chapter4_Manu_fbtamq.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784579153/Land_of_the_Lustrous_Chapter4_Final_d03x06.pdf", 
            "A Prayer Beyond Crystal", "The search for purpose leads beyond battle, identity, and even the self.", 13, ChapterStatus.Publishing);
        
        addChapter("Blue Period", 1,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784573493/Blue_Period_Chapter1_manu_zmu2vo.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573490/Blue_Period_Chapter1_final_lipdd3.pdf", 
            "Blue Period", "A talented student discovers a passion that changes his life forever.", 19, ChapterStatus.Publishing);
        addChapter("Blue Period", 2,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784573493/Blue_Period_Chapter2_manu_c8scwx.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573491/Blue_Period_Chapter2_final_blr2ct.pdf", 
            "Colors of Ambition", "Every brushstroke becomes a step toward understanding himself.", 19, ChapterStatus.Publishing);
        addChapter("Blue Period", 3,"https://res.cloudinary.com/dl002mrzl/image/upload/v1784573492/Blue_Period_Chapter3_manu_ulycta.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573494/Blue_Period_Chapter3_final_inx7ts.pdf", 
            "Canvas of Growth", "Through failure and perseverance, art becomes more than a dream.", 19, ChapterStatus.Publishing);
        addChapter("Blue Period", 4, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573493/Blue_Period_Chapter4_manu_glzqre.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784573494/Blue_Period_Chapter4_final_ty0n05.pdf", 
            "Beyond the Blue", "The pursuit of beauty reveals the courage to create without fear.", 13, ChapterStatus.Processing);
        
        addChapter("The Ancient Magus' Bride", 1, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627709/The_Ancient_Magus_Bride_Chapter1_manu_kszbrs.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627708/The_Ancient_Magus_Bride_Chapter1_final_fnhw3t.pdf", 
            "The Thorn's Promise", "A lonely girl finds a new beginning in a world of ancient magic.", 18, ChapterStatus.Publishing);
        addChapter("The Ancient Magus' Bride", 2, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627709/The_Ancient_Magus_Bride_Chapter2_manu_n5hduc.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627708/The_Ancient_Magus_Bride_Chapter2_final_xon7yf.pdf", 
            "Whispers of the Ancient Woods", "Every encounter with the fae reveals another hidden truth.", 18, ChapterStatus.Publishing);
        addChapter("The Ancient Magus' Bride", 3, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627709/The_Ancient_Magus_Bride_Chapter3_manu_a9fsa2.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627708/The_Ancient_Magus_Bride_Chapter3_final_h5mxig.pdf", 
            "Beneath the Dragon's Sky", "Magic offers wonder, but every blessing demands a sacrifice.", 18, ChapterStatus.Publishing);
        addChapter("The Ancient Magus' Bride", 4, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627708/The_Ancient_Magus_Bride_Chapter4_manu_dy62ve.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784627708/The_Ancient_Magus_Bride_Chapter4_final_td9xvo.pdf", 
            "The Price of Magic", "Through loss and healing, two souls slowly learn to understand each other.", 17, ChapterStatus.Publishing);
        
        addChapter("One Piece", 1,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574007/OnePiece_Chapter1_manu_ztlsul.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574005/OnePiece_Chapter1_final_q4mwy0.pdf", 
            "The Dawn of Adventure", "A carefree pirate sets sail to chase the world's greatest treasure.", 15, ChapterStatus.Publishing);
        addChapter("One Piece", 2,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574020/OnePiece_Chapter2_manu_qccxnh.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574017/OnePiece_Chapter2_final_wksvh4.pdf", 
            "Across the Grand Line", "New friends and impossible adventures await beyond every horizon.", 20, ChapterStatus.Publishing);
        addChapter("One Piece", 3,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574029/OnePiece_Chapter3_manu_uxdhde.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574029/OnePiece_Chapter3_final_q7yx0a.pdf", 
            "Inherited Dreams", "Every island holds a story, a dream, and a battle worth fighting.", 20, ChapterStatus.Publishing);
        addChapter("One Piece", 4,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574060/OnePiece_Chapter4_manu_vf8yxy.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574059/OnePiece_Chapter4_final_irqh55.pdf", 
            "Storms of Freedom", "Freedom is earned through courage, loyalty, and unwavering resolve.", 19, ChapterStatus.Publishing);
        addChapter("One Piece", 5,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574069/OnePiece_Chapter5_manu_mxmt3d.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574068/OnePiece_Chapter5_final_nmn1yx.pdf", 
            "The Emperor's Challenge", "Powerful enemies stand between the crew and their destiny.", 20, ChapterStatus.Publishing);
        addChapter("One Piece", 6,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574076/OnePiece_Chapter6_manu_xdx1kd.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784574074/OnePiece_Chapter6_final_izlro8.pdf", 
            "Beyond the Final Sea", "The journey to the One Piece will change the world forever.", 20, ChapterStatus.Processing);
        
        addChapter("Kingdom", 1,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784566428/Kingdom_Chapter1_Manu_dokq27.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784566427/Kingdom_Chapter1_Final_ecmqn8.pdf", 
            "The Path of a General", "An orphan dreams of becoming the greatest general under the heavens.", 15, ChapterStatus.Publishing);
        addChapter("Kingdom", 2,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571293/Kingdom_Chapter2_Manu_kzjmrg.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571303/Kingdom_Chapter2_Final_u3hzjf.pdf", 
            "Flames of the Battlefield", "Every battle forges warriors through blood, sacrifice, and determination.", 15, ChapterStatus.Publishing);
        addChapter("Kingdom", 3,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571293/Kingdom_Chapter3_Manu_bvoznh.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571293/Kingdom_Chapter3_Final_nssxjs.pdf", 
            "The Rise of Qin", "Ambition and strategy shape the fate of kingdoms at war.", 18, ChapterStatus.Publishing);
        addChapter("Kingdom", 4,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571294/Kingdom_Chapter4_Final_dr1ef2.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784571294/Kingdom_Chapter4_Final_dr1ef2.pdf", 
            "Toward Unification", "The road to unifying China demands courage beyond imagination.", 16, ChapterStatus.Publishing);
        
        addChapter("Blue Lock", 1,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613377/Bluelock_Chapter1_Manu_kyqw8j.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613377/Bluelock_Chapter1_Final_ikatfc.pdf", 
            "The Birth of an Ego", "A young forward enters a ruthless program to become the ultimate striker.", 18, ChapterStatus.Publishing);
        addChapter("Blue Lock", 2,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613378/Bluelock_Chapter2_Manu_j1uc8n.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613377/Bluelock_Chapter2_Final_bybmqf.pdf", 
            "Beyond the Goal", "Only those who embrace their ego can survive the competition.", 18, ChapterStatus.Publishing);
        addChapter("Blue Lock", 3,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613381/Bluelock_Chapter3_Manu_vsyveb.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613378/Bluelock_Chapter3_Final_lgqy1k.pdf", 
            "Strikers Never Yield", "Every match is a battle where talent alone is never enough.", 18, ChapterStatus.Publishing);
        addChapter("Blue Lock", 4,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613380/Bluelock_Chapter4_Manu_oeh1g0.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784613378/Bluelock_Chapter4_Final_ne1usc.pdf", 
            "The World's Best", "The dream of becoming the world's greatest striker drives every decision.", 18, ChapterStatus.Scheduled);
        
        addChapter("Hajime no Ippo", 1, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575294/HajimeNoIppo_Chapter1_Manu_wh3dms.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575293/HajimeNoIppo_Chapter1_Final_edesww.pdf", 
            "The First Punch", "A shy teenager discovers his true strength through boxing.", 11, ChapterStatus.Publishing);
        addChapter("Hajime no Ippo", 2, "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575295/HajimeNoIppo_Chapter2_Manu_mtxle8.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575296/HajimeNoIppo_Chapter2_Final_fpgmsq.pdf", 
            "Road to the Ring", "Every match is a step toward confidence, courage, and growth.", 14, ChapterStatus.Publishing);
        addChapter("Hajime no Ippo", 3,  "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575297/HajimeNoIppo_Chapter3_Manu_lbgeeb.pdf",
            "https://res.cloudinary.com/dl002mrzl/image/upload/v1784575296/HajimeNoIppo_Chapter3_Final_s8qekn.pdf", 
            "The Heart of a Champion", "Victory belongs to those who never stop fighting for their dreams.", 7, ChapterStatus.Processing);

        await dbContext.Chapters.AddRangeAsync(chapters);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedMangaTasksAndIncomesAsync(AppDbContext dbContext)
    {
        if (await dbContext.MangaTasks.AnyAsync())
            return;

        var users = await dbContext.Users.ToListAsync();
        var chapters = await dbContext.Chapters.Include(c => c.Series).ToListAsync();
        var assistants = users.Where(u => u.Role == UserRole.Assistant).ToList();
        
        if (!assistants.Any()) return;

        var random = new Random();
        var tasks = new List<MangaTask>();
        var incomes = new List<Income>();

        foreach (var chapter in chapters)
        {
            int taskCount = random.Next(2, 6);
            for (int i = 0; i < taskCount; i++)
            {
                var taskStatus = chapter.Status == ChapterStatus.Publishing ? MangaTaskStatus.Completed :
                    (i % 2 == 0 ? MangaTaskStatus.Completed : MangaTaskStatus.Available);
                
                var taskId = Guid.NewGuid();
                var assistant = assistants[random.Next(assistants.Count)];
                
                tasks.Add(new MangaTask
                {
                    Id = taskId,
                    ChapterId = chapter.Id,
                    TaskTitle = $"Task {i + 1} for Chap {chapter.ChapterNumber}",
                    TaskDescription = $"Assist with drawing/inking/backgrounds for chapter {chapter.ChapterNumber}.",
                    Status = taskStatus,
                    CreatedById = chapter.Series?.CreatedById ?? users.FirstOrDefault(u => u.Role == UserRole.Mangaka)?.Id ?? Guid.Empty,
                    AssignedToId = assistant.Id,
                    Deadline = chapter.Deadline
                });
                
                incomes.Add(new Income { Id = Guid.NewGuid(), MangaTaskId = taskId, Amount = 100.00m + random.Next(100), Status = IncomeStatus.Pending });
            }
        }

        await dbContext.MangaTasks.AddRangeAsync(tasks);
        await dbContext.Incomes.AddRangeAsync(incomes);
        await dbContext.SaveChangesAsync();
    }
}