namespace Nizhegorodcev_Task2_Severstal.DTOs.Requests
{
    public record StatisticsRequest
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
