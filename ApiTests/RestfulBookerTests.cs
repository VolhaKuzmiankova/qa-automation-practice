using System.Net;
using System.Net.Http.Json;
using Core.Factories;
using Core.Models.BookingModel;
using Newtonsoft.Json.Linq;

namespace ApiTests;

public class RestfulBookerTests
{
    private const string BaseUrl = "https://restful-booker.herokuapp.com";
    private readonly HttpClient _client = new() { BaseAddress = new Uri(BaseUrl) };
    private Booking _booking;
    private int _bookingId;

    [OneTimeSetUp]
    public async Task CreateBooking()
    {
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _booking = BookingFactory.Create();

        var response = await _client.PostAsJsonAsync("/booking", _booking);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateBookingResponse>();

        _bookingId = created!.BookingId;
    }

    [TestCase("admin", "password123", true)]
    [TestCase("admin1", "password123", false)]
    [TestCase("admin", "password121", false)]
    [TestCase("", "", false)]
    public async Task AuthTest(string username, string password, bool isCredentialsValid)
    {
        var response = await _client.PostAsJsonAsync("/auth", new { username, password });

        var json = await response.Content.ReadAsStringAsync();

        var token = JObject.Parse(json)["token"]?.ToString();
        Assert.That(token, isCredentialsValid ? Is.Not.Null : Is.Null);
    }

    [Test]
    public async Task GetPingTest()
    {
        var response = await _client.GetAsync("/ping");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task GetBookingTest()
    {
        var response = await _client.GetAsync("/booking");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await response.Content.ReadAsStringAsync();

        var bookingList = JArray.Parse(json);
        Assert.That(bookingList, Is.Not.Empty);
    }

    [Test]
    public async Task GetBookingByNameFilterTest()
    {
        var response = await _client.GetAsync($"/booking?firstname={Uri.EscapeDataString(_booking.Firstname)}&lastname={Uri.EscapeDataString(_booking.Lastname)}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await response.Content.ReadAsStringAsync();

        var bookingIds = JArray.Parse(json).Select(b => b["bookingid"]?.Value<int>());
        Assert.That(bookingIds, Does.Contain(_bookingId));
    }

    [Test]
    public async Task GetBookingByDateFilterTest()
    {
        // the API treats checkin as strictly greater than the filter value, so filter from the day before (Bug)
        var checkin = DateTime.Parse(_booking.BookingDates.Checkin).AddDays(-1).ToString("yyyy-MM-dd");
        var response = await _client.GetAsync($"/booking?checkin={Uri.EscapeDataString(checkin)}&checkout={Uri.EscapeDataString(_booking.BookingDates.Checkout)}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await response.Content.ReadAsStringAsync();
        var bookingIds = JArray.Parse(json).Select(b => b["bookingid"]?.Value<int>());
        Assert.That(bookingIds, Does.Contain(_bookingId));
    }

    [Test]
    public async Task GetBookingByIdTest()
    {
        var response = await _client.GetAsync($"/booking/{_bookingId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var booking = await response.Content.ReadFromJsonAsync<Booking>();
        Assert.That(booking, Is.Not.Null);
        Assert.That(booking, Is.EqualTo(_booking).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetBookingByIncorrectIdTest()
    {
        var response = await _client.GetAsync("/booking/88888888888");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [OneTimeTearDown]
    public async Task DeleteBooking()
    {
        try
        {
            if (_bookingId == 0) return;

            var authResponse = await _client.PostAsJsonAsync("/auth", new { username = "admin", password = "password123" });
            var token = JObject.Parse(await authResponse.Content.ReadAsStringAsync())["token"]?.ToString();
            Assert.That(token, Is.Not.Null.And.Not.Empty, "Auth did not return a token, cannot delete the booking");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/booking/{_bookingId}");
            request.Headers.Add("Cookie", $"token={token}");

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        finally
        {
            _client.Dispose();
        }
    }
}
