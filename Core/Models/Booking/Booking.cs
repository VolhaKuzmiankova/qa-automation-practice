using System.Text.Json.Serialization;

namespace Core.Models.Booking;

public class Booking
{
    [JsonPropertyName("firstname")]
    public string Firstname { get; set; } = string.Empty;

    [JsonPropertyName("lastname")]
    public string Lastname { get; set; } = string.Empty;

    [JsonPropertyName("totalprice")]
    public int TotalPrice { get; set; }

    [JsonPropertyName("depositpaid")]
    public bool DepositPaid { get; set; }

    [JsonPropertyName("bookingdates")]
    public BookingDates BookingDates { get; set; } = new();

    [JsonPropertyName("additionalneeds")]
    public string? AdditionalNeeds { get; set; }
}
