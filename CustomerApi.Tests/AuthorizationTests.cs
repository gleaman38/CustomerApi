using CustomerApi.Controllers;
using CustomerApi.Data;
using CustomerApi.DTOs;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
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
        var databaseName = Guid.NewGuid().ToString();

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
                        options.UseInMemoryDatabase(databaseName));
                });
            });

        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        db.Customers.AddRange(
            new Customer
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@example.com",
                IsActive = true
            },
            new Customer
            {
                Id = 2,
                FirstName = "John",
                LastName = "Smith",
                Email = "john@example.com",
                IsActive = true
            });

        db.SaveChanges();

        using var client = factory.CreateClient();

        var token = CreateAdminTestToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.GetAsync("/api/Customers");

        var responseBody = await response.Content.ReadAsStringAsync();

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

    [Fact]
    public async Task TestAuthenticatedUserCanCreateCustomer()
    {
        //find and save name of unique database
        var databaseName = Guid.NewGuid().ToString();

        //build test copy of the api
        //and change the database to use in Program.cs in memory database
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
                        options.UseInMemoryDatabase(databaseName));
                });
            });

        //create an Http client to talk to the test api
        using var client = factory.CreateClient();

        //build user token to talk to endpoint
        var token = CreateUserTestToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        //new data to add to in memory db
        var newCustomer = new CreateCustomerDto
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test.customer@example.com",
            IsActive = true
        };

        //call the Post method in the controller
        var response = await client.PostAsJsonAsync(
            "/api/Customers", newCustomer);
        
        //was the Post record created
        Assert.Equal(201, (int)response.StatusCode);

        //read back the results returned
        var result = await response.Content
            .ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(result);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("Customer", result.LastName);
        Assert.Equal("test.customer@example.com", result.Email);
        Assert.True(result.IsActive);

        //look at in memory database
        response = await client.GetAsync($"/api/Customers/{result.Id}");

        //did it find the added record
        Assert.Equal(200, (int)response.StatusCode);

        //read the results of the response
        result = await response.Content.ReadFromJsonAsync<CustomerDto>();

        //are they what we expected
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("Customer", result.LastName);
        Assert.Equal("test.customer@example.com", result.Email);
        Assert.True(result.IsActive);

    }

    [Fact]
    public async Task TestUnauthenticatedUserCannotCreateCustomer()
    {
        //find and save name of unique database
        var databaseName = Guid.NewGuid().ToString();

        //build test copy of the api
        //and change the database to use in Program.cs in memory database
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
                        options.UseInMemoryDatabase(databaseName));
                });
            });

        //create an Http client to talk to the test api
        using var client = factory.CreateClient();

        //new data to add to in memory db
        var newCustomer = new CreateCustomerDto
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test.customer@example.com",
            IsActive = true
        };

        //call the Post method in the controller
        var response = await client.PostAsJsonAsync(
            "/api/Customers", newCustomer);

        //was the Post record created
        Assert.Equal(401, (int)response.StatusCode);

    }

    [Fact]
    public async Task TestAuthenticatedUserCanUpdateCustomer()
    {
        //find and save name of unique database
        var databaseName = Guid.NewGuid().ToString();

        //build test copy of the api
        //and change the database to use in Program.cs in memory database
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
                        options.UseInMemoryDatabase(databaseName));
                });
            });

        //create an Http client to talk to the test api
        using var client = factory.CreateClient();

        //new data to add to in memory db
        var customer = new Customer
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test.customer@example.com",
            IsActive = true
        };

        //add the customer to edit later into in memory database
        using var scope = factory.Services.CreateScope();

        //now db is the variable for dbContext from in memory database above
        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        //add customer to edit in the Put
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        //now in memory db has a record to edit

        //build user token to talk to endpoint
        var token = CreateUserTestToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        var newCustomerData = new Customer {
            FirstName = "AnotherTest",
            LastName = "AnotherCustomer",
            Email = "anothertest.anothercustomer@example.com",
            IsActive = false
        };

        //call the Put method in the controller
        var response = await client.PutAsJsonAsync(
            $"/api/Customers/{customer.Id}", newCustomerData);

        Assert.Equal(200, (int)response.StatusCode);

        //read the results of the response
        var result = await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(result);
        Assert.Equal("AnotherTest", result.FirstName);
        Assert.Equal("AnotherCustomer", result.LastName);
        Assert.Equal("anothertest.anothercustomer@example.com", result.Email);
        Assert.False(result.IsActive);

    }


    [Fact]
    public async Task TestUnauthenticatedUserCannotUpdateCustomer()
    {
        //find and save name of unique database
        var databaseName = Guid.NewGuid().ToString();

        //build test copy of the api
        //and change the database to use in Program.cs in memory database
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
                        options.UseInMemoryDatabase(databaseName));
                });
            });

        //create an Http client to talk to the test api
        using var client = factory.CreateClient();

        //new data to add to in memory db
        var customer = new Customer
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test.customer@example.com",
            IsActive = true
        };

        //add the customer to edit later into in memory database
        using var scope = factory.Services.CreateScope();

        //now db is the variable for dbContext from in memory database above
        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        //add customer to edit in the Put
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        //now in memory db has a record to edit

        var newCustomerData = new Customer
        {
            FirstName = "AnotherTest",
            LastName = "AnotherCustomer",
            Email = "anothertest.anothercustomer@example.com",
            IsActive = false
        };

        //call the Put method in the controller
        var response = await client.PutAsJsonAsync(
            $"/api/Customers/{customer.Id}", newCustomerData);

        Assert.Equal(401, (int)response.StatusCode);

    }

    [Fact]
    public async Task TestAuthenticatedUserCanDeleteCustomer()
    {
        //find and save name of unique database
        var databaseName = Guid.NewGuid().ToString();

        //build test copy of the api
        //and change the database to use in Program.cs in memory database
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
                        options.UseInMemoryDatabase(databaseName));
                });
            });

        //create an Http client to talk to the test api
        using var client = factory.CreateClient();

        //new data to add to in memory db
        var customer = new Customer
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test.customer@example.com",
            IsActive = true
        };

        //add the customer to edit later into in memory database
        using var scope = factory.Services.CreateScope();

        //now db is the variable for dbContext from in memory database above
        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        //add customer to edit in the Put
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        //now in memory db has a record to edit

        //build user token to talk to endpoint
        var token = CreateUserTestToken();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        //call the Delete method in the controller
        var response = await client.DeleteAsync($"/api/Customers/{customer.Id}");

        Assert.Equal(200, (int)response.StatusCode);

        //read the results of the response for the record you deleted
        var result = await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(result);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("Customer", result.LastName);
        Assert.Equal("test.customer@example.com", result.Email);
        Assert.True(result.IsActive);

        //check to see if record removed from in memory database
        var getResponse = await client.GetAsync(
            $"/api/Customers/{customer.Id}");

        Assert.Equal(404, (int)getResponse.StatusCode);

    }

    [Fact]
    public async Task TestUnauthenticatedUserCannotDeleteCustomer()
    {
        //find and save name of unique database
        var databaseName = Guid.NewGuid().ToString();

        //build test copy of the api
        //and change the database to use in Program.cs in memory database
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
                        options.UseInMemoryDatabase(databaseName));
                });
            });

        //create an Http client to talk to the test api
        using var client = factory.CreateClient();

        //new data to add to in memory db
        var customer = new Customer
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test.customer@example.com",
            IsActive = true
        };

        //add the customer to edit later into in memory database
        using var scope = factory.Services.CreateScope();

        //now db is the variable for dbContext from in memory database above
        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        //add customer to edit in the Put
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        //now in memory db has a record to edit

        //call the Delete method in the controller
        var response = await client.DeleteAsync($"/api/Customers/{customer.Id}");

        Assert.Equal(401, (int)response.StatusCode);

    }

}