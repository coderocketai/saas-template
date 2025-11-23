using CodeRocket.Common.Dto;
using CodeRocket.Common.Enums;
using CodeRocket.Common.Models.Users;
using CodeRocket.DataAccess.Interfaces;
using CodeRocket.Services.Users;
using Moq;

namespace CodeRocket.Services.UnitTests;

[TestClass]
public sealed class UserServiceTests
{
    private Mock<IUserRepository> _mockRepository = null!;
    private UserService _userService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IUserRepository>();
        _userService = new UserService(_mockRepository.Object);
    }

    #region GetAllUsersAsync Tests

    [TestMethod]
    public async Task GetAllUsersAsync_WithValidRequest_ReturnsPaginatedUsers()
    {
        // Arrange
        var request = new PaginationRequest { Page = 1, PageSize = 10 };
        var expectedResponse = new PaginationResponse<User>
        {
            Items = new List<User>
            {
                new User { Id = 1, Email = "user1@example.com", DisplayName = "User1" },
                new User { Id = 2, Email = "user2@example.com", DisplayName = "User2" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetAllAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _userService.GetAllUsersAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Items.Count());
        Assert.AreEqual(2, result.TotalCount);
        Assert.AreEqual(1, result.Page);
        _mockRepository.Verify(r => r.GetAllAsync(request), Times.Once);
    }

    [TestMethod]
    public async Task GetAllUsersAsync_WithEmptyResult_ReturnsEmptyPaginatedResponse()
    {
        // Arrange
        var request = new PaginationRequest { Page = 1, PageSize = 10 };
        var expectedResponse = new PaginationResponse<User>
        {
            Items = new List<User>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _mockRepository.Setup(r => r.GetAllAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _userService.GetAllUsersAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Items.Count());
        Assert.AreEqual(0, result.TotalCount);
        _mockRepository.Verify(r => r.GetAllAsync(request), Times.Once);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [TestMethod]
    public async Task GetUserByIdAsync_WithExistingId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "JohnD",
            Role = UserRole.User
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.Id);
        Assert.AreEqual("test@example.com", result.Email);
        Assert.AreEqual("JohnD", result.DisplayName);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task GetUserByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.IsNull(result);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    #endregion

    #region CreateUserAsync Tests

    [TestMethod]
    public async Task CreateUserAsync_WithValidUser_SetsTimestampsAndCreatesUser()
    {
        // Arrange
        var newUser = new User
        {
            Email = "newuser@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            DisplayName = "JaneS",
            Role = UserRole.User
        };

        var createdUser = new User
        {
            Id = 1,
            Email = newUser.Email,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            DisplayName = newUser.DisplayName,
            Role = newUser.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(newUser.IsDeleted);
        Assert.IsTrue((DateTime.UtcNow - newUser.CreatedAt).TotalSeconds < 2);
        _mockRepository.Verify(r => r.CreateAsync(It.Is<User>(u => 
            u.Email == newUser.Email && 
            u.IsDeleted == false &&
            u.CreatedAt != default)), Times.Once);
    }

    [TestMethod]
    public async Task CreateUserAsync_SetsIsDeletedToFalse()
    {
        // Arrange
        var newUser = new User
        {
            Email = "test@example.com",
            DisplayName = "TestUser",
            IsDeleted = true // Explicitly setting to true to test override
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
        await _userService.CreateUserAsync(newUser);

        // Assert
        Assert.IsFalse(newUser.IsDeleted);
        _mockRepository.Verify(r => r.CreateAsync(It.Is<User>(u => u.IsDeleted == false)), Times.Once);
    }

    #endregion

    #region UpdateUserAsync Tests

    [TestMethod]
    public async Task UpdateUserAsync_WithValidUser_UpdatesTimestampAndSaves()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Email = "old@example.com",
            DisplayName = "OldName",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedUser = new User
        {
            Id = existingUser.Id,
            Email = "new@example.com",
            DisplayName = "NewName",
            CreatedAt = existingUser.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _userService.UpdateUserAsync(existingUser);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue((DateTime.UtcNow - existingUser.UpdatedAt).TotalSeconds < 2);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => 
            u.Id == existingUser.Id && 
            u.UpdatedAt > existingUser.CreatedAt)), Times.Once);
    }

    [TestMethod]
    public async Task UpdateUserAsync_UpdatesOnlyUpdatedAtTimestamp()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            CreatedAt = createdAt,
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        await _userService.UpdateUserAsync(user);

        // Assert
        Assert.AreEqual(createdAt, user.CreatedAt); // CreatedAt should not change
        Assert.IsTrue((DateTime.UtcNow - user.UpdatedAt).TotalSeconds < 2); // UpdatedAt should be updated
        _mockRepository.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    #endregion

    #region DeleteUserAsync Tests

    [TestMethod]
    public async Task DeleteUserAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        _mockRepository.Setup(r => r.DeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.IsTrue(result);
        _mockRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteUserAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        _mockRepository.Setup(r => r.DeleteAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.IsFalse(result);
        _mockRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    #endregion
}