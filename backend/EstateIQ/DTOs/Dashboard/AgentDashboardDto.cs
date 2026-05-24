namespace EstateIQ.DTOs.Dashboard;

public class AgentDashboardDto
{
    public string Role { get; set; } = "Agent";
    public int AgentId { get; set; }
    public int MyProperties { get; set; }
    public int MyForSaleProperties { get; set; }
    public int MyForRentProperties { get; set; }
    public int MySoldProperties { get; set; }
    public int MyRentedProperties { get; set; }
    public IReadOnlyList<DashboardPropertyDto> RecentMyProperties { get; set; } = [];
}
