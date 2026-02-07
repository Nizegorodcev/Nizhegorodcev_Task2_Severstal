namespace Nizhegorodcev_Task2_Severstal.DTOs.Responses
{
    /// <summary>
    /// Ответ с данными рулона
    /// </summary>
    public record RollResponse
    {
        public int Id { get; init; }
        public decimal Length { get; init; }
        public decimal Weight { get; init; }
        public DateTime AddedDate { get; init; }
        public DateTime? DeletedDate { get; init; }
        public bool IsDeleted { get; init; }
    }
}
