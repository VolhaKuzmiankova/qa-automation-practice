# QaPractice

QA automation practice project in C# (.NET 10, NUnit). It contains API tests for the
[Restful-booker](https://restful-booker.herokuapp.com/apidoc/index.html) playground API.

## Structure

- `Core` - shared code, including the booking models (`Core.Models`)
- `ApiTests` - API tests for Restful-booker
- `UiTests` - UI tests

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Internet access (the API tests call the live Restful-booker service)

## Running tests

Run all tests from the repository root:

```bash
dotnet test
```

Run only the API tests:

```bash
dotnet test ApiTests
```

Run a single test by name:

```bash
dotnet test ApiTests --filter "Name=GetBookingByIdTest"
```
