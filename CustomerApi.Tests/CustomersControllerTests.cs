using CustomerApi.Controllers;
using CustomerApi.Data;
using CustomerApi.DTOs;
using CustomerApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace CustomerApi.Tests
{
    public class CustomersControllerTests
    {
        [Fact]
        public async Task GetCustomer_ReturnsCustomer_WhenCustomerExists()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            using var context = new CustomerDbContext(options);

            context.Customers.Add(new Models.Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@example.com",
                IsActive = true
            });

            await context.SaveChangesAsync();

            var controller = new CustomersController(context);

            //Act
            var result = await controller.GetCustomer(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var customer = Assert.IsType<CustomerDto>(okResult.Value);

            Assert.Equal("John", customer.FirstName);
            Assert.Equal("Smith", customer.LastName);
        }

        [Fact]
        public async Task GetCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            using var context = new CustomerDbContext(options);

            var controller = new CustomersController(context);

            //Act
            var result = await controller.GetCustomer(999);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);

        }

        [Fact]
        public async Task CreateCustomer_ReturnsCreatedCustomer()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            using var context = new CustomerDbContext(options);

            var controller = new CustomersController(context);

            //Act
            var newCustomer = new CreateCustomerDto
            {
                FirstName = "Test",
                LastName = "Customer",
                Email = "test.customer@example.com",
                IsActive = true
            };

            var result = await controller.CreateCustomer(newCustomer);

            //Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

            var customer = Assert.IsType<CustomerDto>(createdResult.Value);

            Assert.Equal("Test", customer.FirstName);
            Assert.Equal("Customer", customer.LastName);
            Assert.Equal("test.customer@example.com", customer.Email);
            Assert.True(customer.IsActive);

        }

        [Fact]
        public async Task EditCustomer_ReturnsUpdatedCustomer_WhenCustomerExists()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new CustomerDbContext(options);

            context.Customers.Add(new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "john@example.com",
                IsActive = true
            });

            await context.SaveChangesAsync();

            var controller = new CustomersController(context);

            //"database" holds a customer with id 1

            var updatedCustomer = new UpdateCustomerDto
            {
                FirstName = "FirstChanged",
                LastName = "LastChanged",
                Email = "changed@example.com",
                IsActive = false
            };

            //Act

            var result = await controller.EditCustomer(1, updatedCustomer);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var customer = Assert.IsType<CustomerDto>(okResult.Value);

            Assert.Equal(1, customer.Id);
            Assert.Equal("FirstChanged", customer.FirstName);
            Assert.Equal("LastChanged", customer.LastName);
            Assert.Equal("changed@example.com", customer.Email);
            Assert.False(customer.IsActive);

        }

        [Fact]
        public async Task EditCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new CustomerDbContext(options);

            var controller = new CustomersController(context);

            //"database" holds no customers

            var updatedCustomer = new UpdateCustomerDto
            {
                FirstName = "FirstChanged",
                LastName = "LastChanged",
                Email = "changed@example.com",
                IsActive = false
            };

            await context.SaveChangesAsync();

            //Act

            var result = await controller.EditCustomer(1, updatedCustomer);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);

        }

        [Fact]
        public async Task DeleteCustomer_ReturnsDeletedCustomer_WhenCustomerExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new CustomerDbContext(options);

            context.Customers.Add(new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "john@example.com",
                IsActive = true
            });

            await context.SaveChangesAsync();

            var controller = new CustomersController(context);

            // Act
            var result = await controller.DeleteCustomer(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var customer = Assert.IsType<CustomerDto>(okResult.Value);

            Assert.Equal(1, customer.Id);
            Assert.Equal("John", customer.FirstName);
            Assert.Equal("Smith", customer.LastName);

            var deletedCustomer = await context.Customers.FindAsync(1);

            Assert.Null(deletedCustomer);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CustomerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new CustomerDbContext(options);

            var controller = new CustomersController(context);

            // Act
            var result = await controller.DeleteCustomer(1);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);

        }

    }

}