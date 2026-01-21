using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using AuthService.Models;
using System.Collections.Generic;

[TestFixture]
public class UserModelTests
{
    [Test]
    public void ValidateModel_WithValidData_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            SteamID = "12345678901234567",
            Name = "Test User",
            Email = "test@example.com",
            Phone = "+79875580000"
        };
        var context = new ValidationContext(user, null, null);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(user, context, results, true);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void ValidateModel_WithInvalidSteamID_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            SteamID = "invalidSteamID",
            Name = "",
            Email = "1111",
            Phone = "eww22211123"
        };
        var context = new ValidationContext(user, null, null);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(user, context, results, true);

        // Assert
        Assert.IsFalse(isValid);
        Assert.That(results, Has.Some.Matches<ValidationResult>(r => r.ErrorMessage == "Wrong Steam ID number"));
    }
}
