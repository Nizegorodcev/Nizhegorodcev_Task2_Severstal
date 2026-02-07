using Nizhegorodcev_Task2_Severstal.DTOs.Requests;
using Nizhegorodcev_Task2_Severstal.DTOs.Responses;
using Nizhegorodcev_Task2_Severstal.Models;
namespace Nizhegorodcev_Task2_Severstal.Repositories
{
    public interface IRollRepository
    {
        Task<MetalRoll> AddAsync(MetalRoll roll);
        Task<MetalRoll?> DeleteAsync(int id);
        Task<MetalRoll?> GetByIdAsync(int id);
        Task<IEnumerable<MetalRoll>> GetAllAsync(RollFilterRequest? filter = null);
        Task<StatisticsResponse> GetStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<bool> ExistsAsync(int id);
        Task<MetalRoll> UpdateAsync(MetalRoll roll);
    }
}
