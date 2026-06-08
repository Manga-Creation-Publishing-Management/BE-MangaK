using Manga.Repository.Entity.Enums;

namespace Manga.Service.IncomeTask;

public class Response
{
    public class GetIncomeResponse
    {
        public Guid IncomeId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public IncomeStatus Status { get; set; }
        public string TaskTitle { get; set; }
    }
    public class GetIncomeHistoryResponse
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalIncome { get; set; }
    }
}