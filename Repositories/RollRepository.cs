using Microsoft.EntityFrameworkCore;
using Nizhegorodcev_Task2_Severstal.Data;
using Nizhegorodcev_Task2_Severstal.DTOs.Requests;
using Nizhegorodcev_Task2_Severstal.DTOs.Responses;
using Nizhegorodcev_Task2_Severstal.Models;

namespace Nizhegorodcev_Task2_Severstal.Repositories
{
    public class RollRepository : IRollRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RollRepository> _logger;

        public RollRepository(
            ApplicationDbContext context,
            ILogger<RollRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MetalRoll> AddAsync(MetalRoll roll)
        {
            await _context.Rolls.AddAsync(roll);
            await _context.SaveChangesAsync();
            return roll;
        }

        public async Task<MetalRoll?> DeleteAsync(int id)
        {
            var roll = await _context.Rolls.FindAsync(id);
            if (roll == null || roll.DeletedDate.HasValue)
                return null;

            roll.MarkAsDeleted();
            await _context.SaveChangesAsync();
            return roll;
        }

        public async Task<MetalRoll?> GetByIdAsync(int id)
        {
            return await _context.Rolls.FindAsync(id);
        }

        public async Task<IEnumerable<MetalRoll>> GetAllAsync(RollFilterRequest? filter = null)
        {
            var query = _context.Rolls.AsQueryable();

            if (!(filter?.IncludeDeleted ?? false))
            {
                query = query.Where(r => r.DeletedDate == null);
            }

            if (filter != null)
            {
                if (filter.MinId.HasValue)
                    query = query.Where(r => r.Id >= filter.MinId.Value);
                if (filter.MaxId.HasValue)
                    query = query.Where(r => r.Id <= filter.MaxId.Value);

                if (filter.MinLength.HasValue)
                    query = query.Where(r => r.Length >= filter.MinLength.Value);
                if (filter.MaxLength.HasValue)
                    query = query.Where(r => r.Length <= filter.MaxLength.Value);

                if (filter.MinWeight.HasValue)
                    query = query.Where(r => r.Weight >= filter.MinWeight.Value);
                if (filter.MaxWeight.HasValue)
                    query = query.Where(r => r.Weight <= filter.MaxWeight.Value);

                if (filter.AddedDateStart.HasValue)
                    query = query.Where(r => r.AddedDate >= filter.AddedDateStart.Value);
                if (filter.AddedDateEnd.HasValue)
                    query = query.Where(r => r.AddedDate <= filter.AddedDateEnd.Value);

                if (filter.DeletedDateStart.HasValue)
                    query = query.Where(r => r.DeletedDate >= filter.DeletedDateStart.Value);
                if (filter.DeletedDateEnd.HasValue)
                    query = query.Where(r => r.DeletedDate <= filter.DeletedDateEnd.Value);
            }

            return await query.OrderBy(r => r.Id).ToListAsync();
        }

        public async Task<StatisticsResponse> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Получаем все рулоны
                var allRolls = await _context.Rolls.ToListAsync();

                // Нормализуем даты для сравнения (убираем время)
                var startDateDate = startDate.Date;
                var endDateDate = endDate.Date.AddDays(1).AddTicks(-1); // Конец дня

                // Рулоны, добавленные в период
                var addedRolls = allRolls
                    .Where(r => r.AddedDate >= startDateDate && r.AddedDate <= endDateDate)
                    .ToList();

                // Рулоны, удаленные в период
                var deletedRolls = allRolls
                    .Where(r => r.DeletedDate.HasValue &&
                               r.DeletedDate.Value >= startDateDate &&
                               r.DeletedDate.Value <= endDateDate)
                    .ToList();

                // Рулоны, которые были на складе в период
                var rollsInPeriod = allRolls
                    .Where(r => r.AddedDate <= endDateDate &&
                               (r.DeletedDate == null || r.DeletedDate >= startDateDate))
                    .ToList();

                // Интервалы между добавлением и удалением
                var intervals = deletedRolls
                    .Select(r => r.DeletedDate.Value - r.AddedDate)
                    .ToList();

                return new StatisticsResponse
                {
                    AddedCount = addedRolls.Count,
                    DeletedCount = deletedRolls.Count,
                    AverageLength = rollsInPeriod.Any() ?
                        Math.Round(rollsInPeriod.Average(r => r.Length), 2) : 0,
                    AverageWeight = rollsInPeriod.Any() ?
                        Math.Round(rollsInPeriod.Average(r => r.Weight), 2) : 0,
                    MaxLength = rollsInPeriod.Any() ?
                        rollsInPeriod.Max(r => r.Length) : 0,
                    MinLength = rollsInPeriod.Any() ?
                        rollsInPeriod.Min(r => r.Length) : 0,
                    MaxWeight = rollsInPeriod.Any() ?
                        rollsInPeriod.Max(r => r.Weight) : 0,
                    MinWeight = rollsInPeriod.Any() ?
                        rollsInPeriod.Min(r => r.Weight) : 0,
                    TotalWeight = rollsInPeriod.Any() ?
                        Math.Round(rollsInPeriod.Sum(r => r.Weight), 2) : 0,
                    MaxInterval = intervals.Any() ?
                        intervals.Max().ToString(@"d\.hh\:mm\:ss") : null,
                    MinInterval = intervals.Any() ?
                        intervals.Min().ToString(@"d\.hh\:mm\:ss") : null,
                    // Остальные поля
                    DayWithMinRolls = null,
                    DayWithMaxRolls = null,
                    DayWithMinWeight = null,
                    DayWithMaxWeight = null,
                    MinRollsCount = 0,
                    MaxRollsCount = 0,
                    MinTotalWeight = 0,
                    MaxTotalWeight = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetStatisticsAsync. StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Rolls.AnyAsync(r => r.Id == id);
        }

        public async Task<MetalRoll> UpdateAsync(MetalRoll roll)
        {
            _context.Rolls.Update(roll);
            await _context.SaveChangesAsync();
            return roll;
        }
    }
}
