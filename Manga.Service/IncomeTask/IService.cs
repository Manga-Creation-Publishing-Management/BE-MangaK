namespace Manga.Service.IncomeTask;

public interface IService
{
    public Task<(List<Response.GetIncomeResponse>, decimal IncomeMonth)> GetIncome(Request.GetIncomeRequest request);
    public Task<List<Response.GetIncomeHistoryResponse>> GetIncomeHistory();
}