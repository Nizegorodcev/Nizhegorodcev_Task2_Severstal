using System.ComponentModel.DataAnnotations;

namespace Nizhegorodcev_Task2_Severstal.DTOs.Requests
{
    /// <summary>
    /// Запрос на создание нового рулона
    /// </summary>
    /// <example>
    /// {
    ///   "length": 10.5,
    ///   "weight": 2.3
    /// }
    /// </example>
    public record CreateRollRequest
    {
        /// <summary>
        /// Длина рулона в метрах
        /// </summary>
        /// <example>10.5</example>
        [Required(ErrorMessage = "Длина обязательна")]
        [Range(0.01, 10000.00, ErrorMessage = "Длина должна быть от 0.01 до 10000")]
        public decimal Length { get; init; }

        /// <summary>
        /// Вес рулона в тоннах
        /// </summary>
        /// <example>2.3</example>
        [Required(ErrorMessage = "Вес обязателен")]
        [Range(0.01, 10000.00, ErrorMessage = "Вес должен быть от 0.01 до 10000")]
        public decimal Weight { get; init; }

    }
}
