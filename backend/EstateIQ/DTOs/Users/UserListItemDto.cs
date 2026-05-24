namespace EstateIQ.DTOs.Users;

public class UserListItemDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public DateTime CreatedAt { get; set; }

    public IReadOnlyCollection<UserCompanyListItemDto> Companies { get; set; } = [];

    public IReadOnlyCollection<string> Roles { get; set; } = [];
}

public class UserCompanyListItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RelationshipType { get; set; } = string.Empty;
}
