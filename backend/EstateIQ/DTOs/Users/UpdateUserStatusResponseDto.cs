namespace EstateIQ.DTOs.Users;

public class UpdateUserStatusResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }
}
