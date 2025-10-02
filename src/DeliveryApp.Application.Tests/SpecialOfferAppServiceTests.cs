using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryApp.Application.Contracts.Dtos;
using DeliveryApp.Application.Contracts.Services;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Repositories;
using DeliveryApp.EntityFrameworkCore.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Xunit;

namespace DeliveryApp.Application.Tests
{
    public class SpecialOfferAppServiceTests
    {
        private readonly Mock<ISpecialOfferRepository> _mockOfferRepository;
        private readonly Mock<IRepository<Restaurant, Guid>> _mockRestaurantRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<ILogger<SpecialOfferAppService>> _mockLogger;
        private readonly SpecialOfferAppService _service;

        public SpecialOfferAppServiceTests()
        {
            _mockOfferRepository = new Mock<ISpecialOfferRepository>();
            _mockRestaurantRepository = new Mock<IRepository<Restaurant, Guid>>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockLogger = new Mock<ILogger<SpecialOfferAppService>>();

            _service = new SpecialOfferAppService(
                _mockOfferRepository.Object,
                _mockRestaurantRepository.Object,
                _mockCurrentUser.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateAsync_WithValidInput_ShouldCreateOffer()
        {
            // Arrange
            var input = new CreateSpecialOfferDto
            {
                RestaurantId = Guid.NewGuid(),
                Title = "Test Offer",
                Description = "Test Description",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(7),
                DiscountPercentage = 20,
                Priority = 1,
                IsActive = true
            };

            var restaurant = new Restaurant(Guid.NewGuid()) { OwnerId = Guid.NewGuid() };
            _mockRestaurantRepository.Setup(r => r.GetAsync(input.RestaurantId))
                .ReturnsAsync(restaurant);

            _mockCurrentUser.Setup(u => u.GetId()).Returns(restaurant.OwnerId);

            // Act
            var result = await _service.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe(input.Title);
            result.RestaurantId.ShouldBe(input.RestaurantId);
            result.Status.ShouldBe("Active");
        }

        [Fact]
        public async Task CreateAsync_WithInvalidDates_ShouldThrowException()
        {
            // Arrange
            var input = new CreateSpecialOfferDto
            {
                RestaurantId = Guid.NewGuid(),
                Title = "Test Offer",
                StartDate = DateTime.Now.AddDays(7),
                EndDate = DateTime.Now.AddDays(1), // End date before start date
                Priority = 1
            };

            var restaurant = new Restaurant(Guid.NewGuid()) { OwnerId = Guid.NewGuid() };
            _mockRestaurantRepository.Setup(r => r.GetAsync(input.RestaurantId))
                .ReturnsAsync(restaurant);

            _mockCurrentUser.Setup(u => u.GetId()).Returns(restaurant.OwnerId);

            // Act & Assert
            await Assert.ThrowsAsync<UserFriendlyException>(() => _service.CreateAsync(input));
        }

        [Fact]
        public async Task GetListAsync_WithFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            var offers = new List<SpecialOffer>
            {
                new SpecialOffer(Guid.NewGuid())
                {
                    RestaurantId = Guid.NewGuid(),
                    Title = "Active Offer",
                    Status = OfferStatus.Active,
                    IsActive = true,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(7)
                },
                new SpecialOffer(Guid.NewGuid())
                {
                    RestaurantId = Guid.NewGuid(),
                    Title = "Inactive Offer",
                    Status = OfferStatus.Inactive,
                    IsActive = false,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(7)
                }
            };

            _mockOfferRepository.Setup(r => r.GetListAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid?>()))
                .ReturnsAsync(offers);

            var input = new GetSpecialOfferListDto
            {
                OnlyActive = true,
                Status = "Active"
            };

            // Act
            var result = await _service.GetListAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
            result.Items.Count.ShouldBe(1);
            result.Items.First().Title.ShouldBe("Active Offer");
        }

        [Fact]
        public async Task ActivateOfferAsync_ShouldChangeStatusToActive()
        {
            // Arrange
            var offerId = Guid.NewGuid();
            var restaurantId = Guid.NewGuid();
            var offer = new SpecialOffer(offerId)
            {
                RestaurantId = restaurantId,
                Status = OfferStatus.Draft,
                IsActive = false
            };

            var restaurant = new Restaurant(restaurantId) { OwnerId = Guid.NewGuid() };
            
            _mockOfferRepository.Setup(r => r.GetAsync(offerId)).ReturnsAsync(offer);
            _mockRestaurantRepository.Setup(r => r.GetAsync(restaurantId)).ReturnsAsync(restaurant);
            _mockCurrentUser.Setup(u => u.GetId()).Returns(restaurant.OwnerId);

            // Act
            var result = await _service.ActivateOfferAsync(offerId);

            // Assert
            result.Status.ShouldBe("Active");
            _mockOfferRepository.Verify(r => r.UpdateAsync(It.IsAny<SpecialOffer>()), Times.Once);
        }

        [Fact]
        public async Task GetOffersForDateAsync_ShouldReturnValidOffers()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var targetDate = DateTime.Now.Date;
            var offers = new List<SpecialOffer>
            {
                new SpecialOffer(Guid.NewGuid())
                {
                    RestaurantId = restaurantId,
                    StartDate = targetDate.AddDays(-1),
                    EndDate = targetDate.AddDays(1),
                    IsActive = true,
                    Status = OfferStatus.Active
                }
            };

            _mockOfferRepository.Setup(r => r.GetOffersForDateAsync(restaurantId, targetDate))
                .ReturnsAsync(offers);

            // Act
            var result = await _service.GetOffersForDateAsync(restaurantId, targetDate);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
        }

        [Fact]
        public async Task SearchOffersAsync_ShouldReturnMatchingOffers()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var searchTerm = "discount";
            var offers = new List<SpecialOffer>
            {
                new SpecialOffer(Guid.NewGuid())
                {
                    RestaurantId = restaurantId,
                    Title = "20% Discount Offer",
                    Description = "Great discount on all items"
                }
            };

            _mockOfferRepository.Setup(r => r.SearchOffersAsync(restaurantId, searchTerm))
                .ReturnsAsync(offers);

            // Act
            var result = await _service.SearchOffersAsync(restaurantId, searchTerm);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.First().Title.ShouldContain("discount", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetMostUsedOffersAsync_ShouldReturnOrderedByUsage()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var offers = new List<SpecialOffer>
            {
                new SpecialOffer(Guid.NewGuid())
                {
                    RestaurantId = restaurantId,
                    Title = "Most Used Offer",
                    CurrentUses = 100
                },
                new SpecialOffer(Guid.NewGuid())
                {
                    RestaurantId = restaurantId,
                    Title = "Less Used Offer",
                    CurrentUses = 50
                }
            };

            _mockOfferRepository.Setup(r => r.GetMostUsedOffersAsync(restaurantId, 10))
                .ReturnsAsync(offers);

            // Act
            var result = await _service.GetMostUsedOffersAsync(restaurantId, 10);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.First().Title.ShouldBe("Most Used Offer");
        }

        [Fact]
        public async Task ProcessRecurringOffersAsync_ShouldProcessOffersNeedingUpdate()
        {
            // Arrange
            var offersNeedingUpdate = new List<SpecialOffer>
            {
                new SpecialOffer(Guid.NewGuid())
                {
                    IsRecurring = true,
                    RecurrencePattern = "{\"Type\":\"Daily\",\"Interval\":1}",
                    NextOccurrence = DateTime.Now.AddDays(-1)
                }
            };

            _mockOfferRepository.Setup(r => r.GetOffersNeedingRecurrenceUpdateAsync())
                .ReturnsAsync(offersNeedingUpdate);

            // Act
            await _service.ProcessRecurringOffersAsync();

            // Assert
            _mockOfferRepository.Verify(r => r.UpdateNextOccurrenceAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task ValidateOwnershipAsync_WithNonOwner_ShouldThrowException()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var restaurant = new Restaurant(restaurantId) { OwnerId = ownerId };

            _mockRestaurantRepository.Setup(r => r.GetAsync(restaurantId)).ReturnsAsync(restaurant);
            _mockCurrentUser.Setup(u => u.GetId()).Returns(currentUserId);
            _mockCurrentUser.Setup(u => u.IsInRole("admin")).Returns(false);
            _mockCurrentUser.Setup(u => u.IsInRole("manager")).Returns(false);

            var input = new CreateSpecialOfferDto
            {
                RestaurantId = restaurantId,
                Title = "Test Offer",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(7)
            };

            // Act & Assert
            await Assert.ThrowsAsync<Volo.Abp.Authorization.AbpAuthorizationException>(
                () => _service.CreateAsync(input));
        }

        [Fact]
        public async Task ValidateOwnershipAsync_WithAdmin_ShouldSucceed()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var restaurant = new Restaurant(restaurantId) { OwnerId = Guid.NewGuid() };

            _mockRestaurantRepository.Setup(r => r.GetAsync(restaurantId)).ReturnsAsync(restaurant);
            _mockCurrentUser.Setup(u => u.IsInRole("admin")).Returns(true);

            var input = new CreateSpecialOfferDto
            {
                RestaurantId = restaurantId,
                Title = "Test Offer",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(7)
            };

            // Act & Assert
            // Should not throw exception
            await _service.CreateAsync(input);
        }
    }
}
