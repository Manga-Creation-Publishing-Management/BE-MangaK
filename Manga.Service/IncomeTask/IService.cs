namespace Manga.Service.IncomeTask;

public interface IService
{
    // Task<Response.CreateIncomeResponse> CreateIncome(Request.CreateIncomeRequest request);
    Task<Response.GetIncomeResponse> GetIncome(Request.GetIncomeRequest request);
    Task<Response.GetIncomeHistoryResponse> GetIncomeHistory(Request.GetIncomeHistoryRequest request);
}