using Bogus;
using Core.Models.BookingModel;

namespace Core.Factories;

public static class BookingFactory
{
    private static readonly Faker Faker = new();

    public static Booking Create()
    {
        var checkin = Faker.Date.Soon(365).Date;
        var checkout = checkin.AddDays(Faker.Random.Int(1, 30));

        return new Booking
        {
            Firstname = Faker.Name.FirstName(),
            Lastname = Faker.Name.LastName(),
            TotalPrice = Faker.Random.Int(50, 1000),
            DepositPaid = Faker.Random.Bool(),
            BookingDates = new BookingDates
            {
                Checkin = checkin.ToString("yyyy-MM-dd"),
                Checkout = checkout.ToString("yyyy-MM-dd")
            },
            AdditionalNeeds = Faker.Lorem.Word()
        };
    }
}
