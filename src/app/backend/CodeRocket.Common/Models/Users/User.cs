using CodeRocket.Common.Enums;

namespace CodeRocket.Common.Models.Users;

public class User : ModelBase
{
    public UserRole Role { get; set; } = UserRole.Guest;

    public string? Email { get; set; }
    public string? TelegramId { get; set; }
    public string? DiscordId { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
}