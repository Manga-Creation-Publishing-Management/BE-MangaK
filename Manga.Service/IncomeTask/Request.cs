using Manga.Repository.Entity.Enums;

namespace Manga.Service.IncomeTask;

public class Request
{

    public class GetIncomeRequest
    {
        public int? month { set; get; } = 0;
        public int? year { set; get; } = 0;
        public IncomeStatus? Status { get; set; }
    }
    
}