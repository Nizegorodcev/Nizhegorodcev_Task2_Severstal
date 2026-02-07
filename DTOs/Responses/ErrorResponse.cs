namespace Nizhegorodcev_Task2_Severstal.DTOs.Responses
{
    public record ErrorResponse
    {
        public string Type { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public int Status { get; init; }
        public string Detail { get; init; } = string.Empty;
        public string Instance { get; init; } = string.Empty;
        public Dictionary<string, string[]> Errors { get; init; } = new();
    }
}
