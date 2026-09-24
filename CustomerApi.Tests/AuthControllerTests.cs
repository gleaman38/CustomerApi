using CustomerApi.Controllers;
using CustomerApi.Data;
using CustomerApi.DTOs;
using CustomerApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CustomerApi.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsAreValid()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new CustomerDbContext(options);

        var user = new User
        {
            Username = "testuser",
            Role = "User"
        };

        var hasher = new PasswordHasher<User>();

        user.PasswordHash = hasher.HashPassword(
            user,
            "TestPassword123!");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsATestSecretKeyForUnitTesting123456",
            ["Jwt:Issuer"] = "CustomerApi",
            ["Jwt:Audience"] = "CustomerApiUsers"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var controller = new AuthController(configuration, context);

        var loginRequest = new LoginRequestDto
        {
            Username = "testuser",
            Password = "TestPassword123!"
        };

        var result = await controller.Login(loginRequest);

        var response = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(
            result.Result);

        var loginResponse = Assert.IsType<LoginResponseDto>(
            response.Value);

        Assert.False(string.IsNullOrEmpty(loginResponse.Token));
        Assert.Equal("testuser", loginResponse.Username);
        Assert.Equal("User", loginResponse.Role);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalidPassword()
    {
        //simulate db
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new CustomerDbContext(options);

        var user = new User
        {
            Username = "testuser",
            Role = "User"
        };

        var hasher = new PasswordHasher<User>();

        user.PasswordHash = hasher.HashPassword(
            user,
            "TestPassword123!");

        //add record for good user, role and password to simulated db
        context.Users.Add(user);
        await context.SaveChangesAsync();

        //simulate token creation to run controller method
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsATestSecretKeyForUnitTesting123456",
            ["Jwt:Issuer"] = "CustomerApi",
            ["Jwt:Audience"] = "CustomerApiUsers"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        //simulate controller
        var controller = new AuthController(configuration, context);

        //simulate login request with bad password
        var loginRequest = new LoginRequestDto
        {
            Username = "testuser",
            Password = "BadTestPassword123!"
        };

        //testing Login method with bad password
        var result = await controller.Login(loginRequest);

        // Verify unauthorized response
        Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(
            result.Result);

    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
    {

        //simulate db
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new CustomerDbContext(options);

        //simulate token creation to run controller method
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsATestSecretKeyForUnitTesting123456",
            ["Jwt:Issuer"] = "CustomerApi",
            ["Jwt:Audience"] = "CustomerApiUsers"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        //simulate controller
        var controller = new AuthController(configuration, context);

        //simulate login request with non existent username
        var loginRequest = new LoginRequestDto
        {
            Username = "doesnotexist",
            Password = "TestPassword123!"
        };

        //testing Login method with non existent username
        var result = await controller.Login(loginRequest);

        // Verify unauthorized response
        Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>(
            result.Result);
    }

}