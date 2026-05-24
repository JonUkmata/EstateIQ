namespace EstateIQ.DTOs.Dashboard;

public class UserDashboardDto
{
    public string Role { get; set; } = "User";
    public int AvailableProperties { get; set; }
    public IReadOnlyList<DashboardPropertyDto> LatestProperties { get; set; } = [];
    public IReadOnlyList<string> PopularCities { get; set; } = [];
}
