using System.ComponentModel.DataAnnotations;
using QuizApp.Backend.Models;
using QuizApp.Backend.Repositories;
using QuizApp.Backend.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace QuizAppBackend.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;
        private readonly Mock<ILogger<UserService>> _loggerMock;


        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateUser_WhenUserIsNull_ReturnsNull()
        {
            // Act
            var result = await _userService.CreateUser(null);

            // Assert
            Assert.Null(result);
            _userRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateUser_WhenEmailIsEmpty_ReturnsNull()
        {
            // Arrange
            var user = new User { Email = "" };

            // Act
            var result = await _userService.CreateUser(user);

            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task CreateUser_WhenValidUser_ReturnsCreatedUser()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                FullName = "Test User"
            };

            _userRepositoryMock
                .Setup(r => r.AddAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.CreateUser(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);

            _userRepositoryMock.Verify(
                r => r.AddAsync(user, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsUser()
        {
            // Arrange
            var email = "user@test.com";
            var user = new User { Email = email };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result!.Email);
        }



    }
}