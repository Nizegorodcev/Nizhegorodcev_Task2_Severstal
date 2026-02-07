using System.ComponentModel.DataAnnotations;

namespace Nizhegorodcev_Task2_Severstal.DTOs.Requests
{
    /// <summary>
    /// Фильтр для поиска рулонов
    /// </summary>
    public record RollFilterRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Минимальный ID должен быть больше 0")]
        public int? MinId { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = "Максимальный ID должен быть больше 0")]
        public int? MaxId { get; init; }

        [Range(0.01, 10000.00, ErrorMessage = "Минимальная длина должна быть больше 0")]
        public decimal? MinLength { get; init; }

        [Range(0.01, 10000.00, ErrorMessage = "Максимальная длина должна быть больше 0")]
        public decimal? MaxLength { get; init; }

        [Range(0.01, 10000.00, ErrorMessage = "Минимальный вес должен быть больше 0")]
        public decimal? MinWeight { get; init; }

        [Range(0.01, 10000.00, ErrorMessage = "Максимальный вес должен быть больше 0")]
        public decimal? MaxWeight { get; init; }

        [DataType(DataType.DateTime)]
        public DateTime? AddedDateStart { get; init; }

        [DataType(DataType.DateTime)]
        public DateTime? AddedDateEnd { get; init; }

        [DataType(DataType.DateTime)]
        public DateTime? DeletedDateStart { get; init; }

        [DataType(DataType.DateTime)]
        public DateTime? DeletedDateEnd { get; init; }

        public bool IncludeDeleted { get; init; } = false;
    }
}