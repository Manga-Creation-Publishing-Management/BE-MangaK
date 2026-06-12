using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Microsoft.EntityFrameworkCore;


namespace Manga.Repository.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Series> Series { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<MangaTask> MangaTasks { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<ChapterVoting> ChapterVotings { get; set; }
    public DbSet<Leaderboard> Leaderboards { get; set; }
    public DbSet<PublishingSchedule> PublishingSchedules { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<Reader> Readers { get; set; }

    public DbSet<Category> Categories { get; set; }
    public DbSet<CategorySeries> CategorySeries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(128);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(128);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(128);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(128);
            builder.Property(u => u.Phone).HasMaxLength(15);
            builder.Property(u => u.AvatarUrl).HasMaxLength(500);
            builder.Property(u => u.AuthorName).HasMaxLength(150);
            builder.Property(u => u.Bio).HasMaxLength(2000);
            builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(50)
                .HasDefaultValue(UserRole.Reader);
            builder.Property(u => u.Status).IsRequired().HasConversion<string>().HasMaxLength(50)
                .HasDefaultValue(UserStatus.Active);
            builder.Property(u => u.Verified).IsRequired().HasDefaultValue(false);
            builder.Property(u => u.VerifiedCode).HasMaxLength(20);
            builder.Property(u => u.ResetPasswordCode).HasMaxLength(50);
            builder.HasMany(u => u.CreatedSeries).WithOne(s => s.CreatedBy).HasForeignKey(s => s.CreatedById).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.ApprovedSeries).WithOne(s => s.ApprovedBy).HasForeignKey(s => s.ApprovedById).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.CreatedTasks).WithOne(t => t.CreatedBy).HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.AssignedTasks).WithOne(t => t.AssignedTo).HasForeignKey(t => t.AssignedToId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.SendFeedbacks).WithOne(f => f.Sender).HasForeignKey(f => f.SenderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.ReceivedFeedbacks).WithOne(f => f.Receiver).HasForeignKey(f => f.ReceiverId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.DecidedSchedules).WithOne(p => p.DecidedBy).HasForeignKey(p => p.DecidedById).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.ReviewedSeries).WithOne(s => s.ReviewedBy).HasForeignKey(s => s.ReviewedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserSession>(builder =>
        {
            builder.Property(us => us.DeviceFingerprint).IsRequired().HasMaxLength(300);
            builder.Property(s => s.RefreshToken).IsRequired().HasMaxLength(512);
            builder.Property(s => s.ExpiresAt).IsRequired();
            builder.Property(s => s.IsRevoked).HasDefaultValue(false);

            builder.ToTable(t => t.HasCheckConstraint("CK_UserSession_Exclusive_User_Or_Reader",
                "(\"UserId\" IS NULL AND \"ReaderId\" IS NOT NULL) OR (\"UserId\" IS NOT NULL AND \"ReaderId\" IS NULL)"));

            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.RefreshToken).IsUnique();

            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(s => s.ReaderId);

            builder.HasOne(s => s.Reader)
                .WithMany()
                .HasForeignKey(s => s.ReaderId)
                .OnDelete(DeleteBehavior.Cascade); 
        });

        modelBuilder.Entity<Reader>(builder =>
        {
            builder.Property(r => r.Email).IsRequired().HasMaxLength(128);
            builder.HasIndex(r => r.Email).IsUnique();
            builder.Property(r => r.Name).HasMaxLength(128);
            builder.Property(r => r.AvatarUrl).HasMaxLength(500);
            builder.Property(r => r.GoogleAccountId).HasMaxLength(255);
            
            builder.HasMany(r => r.ChapterVotings).WithOne(v => v.Reader).HasForeignKey(v => v.ReaderId).OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Series>(builder =>
        {
            builder.Property(s => s.Title).IsRequired().HasMaxLength(255);
            builder.Property(s => s.Description).IsRequired().HasMaxLength(3000);
            builder.Property(s => s.CoverFile).HasMaxLength(500);
            builder.Property(s => s.NameFile).HasMaxLength(255);
            builder.Property(s => s.NameFilePublicId).HasMaxLength(500);
            builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(50).HasDefaultValue(SeriesStatus.Processing);            
            builder.HasMany(s => s.Chapters).WithOne(c => c.Series).HasForeignKey(c => c.SeriesId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(s => s.Feedbacks).WithOne(f => f.Series).HasForeignKey(f => f.SeriesId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(s => s.Leaderboards).WithOne(l => l.Series).HasForeignKey(l => l.SeriesId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.PublishingSchedule).WithOne(p => p.Series).HasForeignKey<PublishingSchedule>(p => p.SeriesId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Chapter>(builder =>
        {
            builder.Property(c => c.ChapterNumber).IsRequired();
            builder.Property(c => c.Title).IsRequired().HasMaxLength(255);
            builder.Property(c => c.Summary).HasMaxLength(3000);
            builder.Property(c => c.ManuscriptFileUrl).HasMaxLength(500);
            builder.Property(c => c.ChapterFileUrl).HasMaxLength(500);
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(50)
                .HasDefaultValue(ChapterStatus.Created);
            builder.HasIndex(c => new { c.SeriesId, c.ChapterNumber }).IsUnique();

            builder.HasMany(c => c.MangaTasks).WithOne(t => t.Chapter).HasForeignKey(t => t.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Feedbacks).WithOne(f => f.Chapter).HasForeignKey(f => f.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.ChapterVotes).WithOne(v => v.Chapter).HasForeignKey(v => v.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MangaTask>(builder =>
        {
            builder.Property(t => t.TaskTitle).IsRequired().HasMaxLength(255);
            builder.Property(t => t.TaskDescription).IsRequired().HasMaxLength(3000);
            builder.Property(t => t.submittedFileUrl).HasMaxLength(500);
            builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(50)
                .HasDefaultValue(MangaTaskStatus.Available);

            builder.HasOne(t => t.Income).WithOne(i => i.MangaTask).HasForeignKey<Income>(i => i.MangaTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(t => t.Feedbacks).WithOne(f => f.MangaTask).HasForeignKey(f => f.MangaTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Income>(builder =>
        {
            builder.Property(i => i.Amount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(50)
                .HasDefaultValue(IncomeStatus.Pending);

            builder.HasIndex(i => i.MangaTaskId).IsUnique();
        });

        modelBuilder.Entity<Feedback>(builder => { builder.Property(f => f.Content).IsRequired().HasMaxLength(3000); });

        modelBuilder.Entity<ChapterVoting>(builder =>
        {
            builder.Property(v => v.Rate).IsRequired();
            builder.Property(v => v.VoteAt).IsRequired();
            builder.Property(v => v.Status).IsRequired().HasConversion<string>().HasMaxLength(50)
                .HasDefaultValue(VoteStatus.Active);

            builder.HasIndex(v => new { v.ReaderId, v.ChapterId }).IsUnique();
        });

        modelBuilder.Entity<Leaderboard>(builder =>
        {
            builder.Property(l => l.RankingPeriod).IsRequired().HasMaxLength(50);
            builder.Property(l => l.RankPosition).IsRequired();
            builder.Property(l => l.TotalVotes).IsRequired().HasColumnType("decimal(5,2)");

            builder.HasIndex(l => new { l.SeriesId, l.RankingPeriod }).IsUnique();
        });

        modelBuilder.Entity<PublishingSchedule>(builder =>
        {
            builder.Property(p => p.PublishDate).IsRequired();
            builder.Property(p => p.PublishPeriod).HasMaxLength(50);
            builder.HasIndex(p => p.SeriesId).IsUnique();
        });
        
        modelBuilder.Entity<CategorySeries>(builder =>
        {
            builder.HasKey(cs => new { cs.CategoryId, cs.SeriesId });
            builder.HasOne(cs => cs.Category).WithMany(c => c.CategorySeries).HasForeignKey(cs => cs.CategoryId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(cs => cs.Series).WithMany(s => s.CategorySeries).HasForeignKey(cs => cs.SeriesId).OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Category>(builder =>
        {
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });
    }
}