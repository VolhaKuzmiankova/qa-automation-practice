using System.Net;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;

namespace ApiTests;

public class Tests
{
    HttpClient _client;
    private readonly string _baseUrl = "https://restful-booker.herokuapp.com";

    [SetUp]
    public void Setup()
    {
        _client = new()
        {
            BaseAddress = new Uri(_baseUrl)
        };
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
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

    [TestCase("/booking?firstname=James&lastname=Brown")]
    [TestCase("/booking?checkin=2027-02-10&checkout=2027-02-15")]
    public void GetBookingByFilterTest(string filter)
    {
        var response = _client.GetAsync(filter)
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
        var response = _client.GetAsync("/booking/1")
            .GetAwaiter()
            .GetResult();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = response.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        var booking = JObject.Parse(json);
        Assert.That(booking, Is.Not.Empty);
        Assert.That(booking["firstname"], Is.Not.Null);
        Assert.That(booking["lastname"], Is.Not.Null);
        Assert.That(booking["totalprice"], Is.Not.Null);
    }

    [Test]
    public void GetBookingByIncorrectIdTest()
    {
        var response = _client.GetAsync("/booking/001")
            .GetAwaiter()
            .GetResult();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}