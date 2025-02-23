using Main.Enums;

namespace Main.DTOs;

public class BookingDTO
{
    public string BookingCode { get; set; }
    public int SleepTime { get; set; }
    public DateTime BookingTime { get; set; }
    public BookingStatusEnum Status { get; set; }
    public string SearchType { get; set; } = string.Empty;
}
