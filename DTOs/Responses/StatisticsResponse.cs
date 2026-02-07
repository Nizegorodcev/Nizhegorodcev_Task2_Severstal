namespace Nizhegorodcev_Task2_Severstal.DTOs.Responses
{
    public record StatisticsResponse
    {
        public int AddedCount { get; init; }
        public int DeletedCount { get; init; }
        public decimal AverageLength { get; init; }
        public decimal AverageWeight { get; init; }
        public decimal MaxLength { get; init; }
        public decimal MinLength { get; init; }
        public decimal MaxWeight { get; init; }
        public decimal MinWeight { get; init; }
        public decimal TotalWeight { get; init; }

        // Используем строковое представление TimeSpan
        public string? MaxInterval { get; init; }
        public string? MinInterval { get; init; }

        // Остальные поля (опционально)
        public DateTime? DayWithMinRolls { get; init; }
        public DateTime? DayWithMaxRolls { get; init; }
        public DateTime? DayWithMinWeight { get; init; }
        public DateTime? DayWithMaxWeight { get; init; }
        public int MinRollsCount { get; init; }
        public int MaxRollsCount { get; init; }
        public decimal MinTotalWeight { get; init; }
        public decimal MaxTotalWeight { get; init; }
    }
}
