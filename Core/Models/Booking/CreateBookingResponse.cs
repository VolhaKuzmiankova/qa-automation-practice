using System.Text.Json.Serialization;

namespace Core.Models.Booking;

public class CreateBookingResponse
{
    [JsonPropertyName("bookingid")]
    public int BookingId { get; set; }

    [JsonPropertyName("booking")]
    public Models.Booking.Booking Booking { get; set; } = new();
}
