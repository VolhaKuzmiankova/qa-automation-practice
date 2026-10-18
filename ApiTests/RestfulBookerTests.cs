using System.Net;
using System.Net.Http.Json;
using Core.Models;
using Core.Models.Booking;
using Newtonsoft.Json.Linq;

namespace ApiTests;

public class RestfulBookerTests
{
    HttpClient _client;
    private readonly string _baseUrl = "https://restful-booker.herokuapp.com";
    private Booking _booking;
    private int _bookingId;

    [OneTimeSetUp]
    public void CreateBooking()
    {
        _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        _booking = new Booking
        {
            Firstname = "Jim",
            Lastname = "Brown",
            TotalPrice = 111,
            DepositPaid = true,
            BookingDates = new BookingDates { Checkin = "2018-01-01", Checkout = "2019-01-01" },
            AdditionalNeeds = "Breakfast"
        };

        var response = _client.PostAsJsonAsync("/booking", _booking)
            .GetAwaiter()
            .GetResult();
        response.EnsureSuccessStatusCode();

        var created = response.Content.ReadFromJsonAsync<CreateBookingResponse>()
            .GetAwaiter()
            .GetResult();

        _bookingId = created!.BookingId;
    }

    [TestCase("admin", "password123", true)]
    [TestCase("admin1", "password123", false)]
    [TestCase("admin", "password121", false)]
    [TestCase("", "", false)]
    public void AuthTest(string username, string password, bool isCredentialsValid)
    {
        var response = _client.PostAsJsonAsync("/auth", new { username, password })
            .GetAwaiter()
            .GetResult();

        var json = response.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        var token = JObject.Parse(json)["token"]?.ToString();

        Assert.That(token, isCredentialsValid ? Is.Not.Null : Is.Null);
    }

    [Test]
    public void GetPingTest()
    {
        var response = _client.GetAsync("/ping")
            .GetAwaiter()
            .GetResult();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public void GetBookingTest()
    {
        var response = _client.GetAsync("/booking")
            .GetAwaiter()
            .GetResult();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = response.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        var bookingList = JArray.Parse(json);
        Assert.That(bookingList, Is.Not.Empty);
    }

    [Test]
    public void GetBookingByNameFilterTest()
    {
        var response = _client.GetAsync($"/booking?firstname={_booking.Firstname}&lastname={_booking.Lastname}")
            .GetAwaiter()
            .GetResult();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = response.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        var bookingList = JArray.Parse(json);
        Assert.That(bookingList, Is.Not.Empty);
    }

    [Test]
    public void GetBookingByDateFilterTest()
    {
        // the API treats checkin as strictly greater than the filter value, so filter from the day before (Bug)
        var checkin = DateTime.Parse(_booking.BookingDates.Checkin).AddDays(-1).ToString("yyyy-MM-dd");
        var response = _client.GetAsync($"/booking?checkin={checkin}&checkout={_booking.BookingDates.Checkout}")
            .GetAwaiter()
            .GetResult();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = response.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        var bookingList = JArray.Parse(json);
        Assert.That(bookingList, Is.Not.Empty);
    }

    [Test]
    public void GetBookingByIdTest()
    {
        var response = _client.GetAsync($"/booking/{_bookingId}")
            .GetAwaiter()
            .GetResult();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var booking = response.Content.ReadFromJsonAsync<Booking>()
            .GetAwaiter()
            .GetResult();

        Assert.That(booking, Is.Not.Null);
        Assert.That(booking, Is.EqualTo(_booking).UsingPropertiesComparer());
    }

    [Test]
    public void GetBookingByIncorrectIdTest()
    {
        var response = _client.GetAsync("/booking/88888888888")
            .GetAwaiter()
            .GetResult();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [OneTimeTearDown]
    public void DeleteBooking()
    {
        var authResponse = _client.PostAsJsonAsync("/auth", new { username = "admin", password = "password123" })
            .GetAwaiter()
            .GetResult();
        var token = JObject.Parse(authResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult())["token"]?.ToString();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/booking/{_bookingId}");
        request.Headers.Add("Cookie", $"token={token}");

        var response = _client.SendAsync(request)
            .GetAwaiter()
            .GetResult();
        response.EnsureSuccessStatusCode();

        _client.Dispose();
    }
}