namespace Main.Requests;

public class SearchRequest
{
    public string Destination { get; set; }
    public string? DepartureAirport { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
