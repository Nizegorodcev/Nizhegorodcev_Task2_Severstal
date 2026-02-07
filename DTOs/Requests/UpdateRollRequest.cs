using System.ComponentModel.DataAnnotations;

namespace Nizhegorodcev_Task2_Severstal.DTOs.Requests
{
    /// <summary>
    /// Запрос на обновление рулона
    /// </summary>
    public record UpdateRollRequest
    {
        /// <summary>
        /// Новая длина рулона
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Длина должна быть больше 0")]
        public decimal? Length { get; init; }

        /// <summary>
        /// Новый вес рулона
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Вес должен быть больше 0")]
        public decimal? Weight { get; init; }
    }
}
