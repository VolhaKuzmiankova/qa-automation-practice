using System.Text.Json.Serialization;

namespace Core.Models.BookingModel;

public class CreateBookingResponse
{
    [JsonPropertyName("bookingid")]
    public int BookingId { get; set; }
}
