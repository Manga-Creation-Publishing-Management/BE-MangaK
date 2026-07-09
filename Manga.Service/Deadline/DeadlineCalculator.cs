namespace Manga.Service.Deadline;

public static class DeadlineCalculator
{
    private const int WeeklyBufferDays = 2;
    private const int MonthlyBufferDays = 7;

    private const int WeeklyPeriodDays = 7;
    private const int MonthlyPeriodDays = 30;
    
    public static int GetBufferDays(string? publishPeriod) // Trả về số ngày trừ hao theo chu kỳ
    {
        return publishPeriod?.Trim().ToLower() switch
        {
            "weekly"  => WeeklyBufferDays,
            "monthly" => MonthlyBufferDays,
            _         => WeeklyBufferDays, // mặc định 
        };
    }
    
    public static int GetPeriodDays(string? publishPeriod)// Trả về số ngày của 1 chu kỳ
    {
        return publishPeriod?.Trim().ToLower() switch
        {
            "weekly"  => WeeklyPeriodDays,
            "monthly" => MonthlyPeriodDays,
            _         => WeeklyPeriodDays,
        };
    }
    
    public static DateTimeOffset CalculateDeadline(
        DateTimeOffset publishDate,
        string? publishPeriod,
        int chapterNumber)
    {
        var periodDays = GetPeriodDays(publishPeriod);
        var bufferDays = GetBufferDays(publishPeriod);

        return publishDate
            .AddDays((chapterNumber - 1) * periodDays)
            .AddDays(-bufferDays);
    }

    // Validate PublishDate tối thiểu khi tạo lịch xuất bản
    // Weekly  -> PublishDate >= hôm nay + 7 ngày
    // Monthly -> PublishDate >= hôm nay + 30 ngày
    public static void ValidatePublishDate(DateTimeOffset publishDate, string? publishPeriod)
    {
        var periodDays  = GetPeriodDays(publishPeriod);
        var minimumDate = DateTimeOffset.UtcNow.AddDays(periodDays);

        if (publishDate < minimumDate)
            throw new ArgumentException(
                $"PublishDate must be at least {periodDays} days from now for {publishPeriod ?? "Weekly"} period.");
    }
}