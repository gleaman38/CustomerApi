using CustomerApi.Controllers;
using CustomerApi.Data;
using CustomerApi.DTOs;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Xunit;


namespace CustomerApi.Tests;

public class AuthorizationTests
{
    [Fact]
    public async Task TestUserAuthorization()
    {
        await using var factory = new WebApplicationFactory<Program>();

        using var client = factory.CreateClient();

        var token = CreateUserTestToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.GetAsync("/api/Customers");

        Assert.Equal(403, (int)response.StatusCode);
    }

    private string CreateUserTestToken()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.Name, "testuser"),
        new Claim(ClaimTypes.Role, "User")
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                "ThisIsADevelopmentOnlySecretKey123456789"));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "CustomerApi",
            audience: "CustomerApiUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task TestAdminAuthorization()
    {
        await using var factory = new WebApplicationFactory<Program>();

        using var client = factory.CreateClient();

        var token = CreateAdminTestToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.GetAsync("/api/Customers");

        var result = await response.Content
            .ReadFromJsonAsync<IEnumerable<CustomerDto>>();

        Assert.Equal(200, (int)response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    private string CreateAdminTestToken()
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.Name, "testuser"),
        new Claim(ClaimTypes.Role, "Admin")
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                "ThisIsADevelopmentOnlySecretKey123456789"));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "CustomerApi",
            audience: "CustomerApiUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task TestAuthenticatedUserCanGetCustomer()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<CustomerDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<CustomerDbContext>(options =>
                        options.UseInMemoryDatabase("AuthorizationTests"));
                });
            });

        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        db.Customers.Add(new Customer
        {
            Id = 2,
            FirstName = "John",
            LastName = "Smith",
            Email = "john@example.com",
            IsActive = true
        });

        db.SaveChanges();

        using var client = factory.CreateClient();

        var token = CreateUserTestToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.GetAsync("/api/Customers/2");

        var result = await response.Content
            .ReadFromJsonAsync<CustomerDto>();

        Assert.Equal(200, (int)response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);

    }


    [Fact]
    public async Task TestNotAuthenticatedUserCannotGetCustomer()
    {
        await using var factory = new WebApplicationFactory<Program>();

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/Customers/2");

        Assert.Equal(401, (int)response.StatusCode);

    }

}