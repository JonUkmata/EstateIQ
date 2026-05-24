namespace EstateIQ.DTOs.Dashboard;

public class AdminDashboardDto
{
    public string Role { get; set; } = "Admin";
    public int TotalProperties { get; set; }
    public int ForSaleProperties { get; set; }
    public int ForRentProperties { get; set; }
    public int SoldProperties { get; set; }
    public int RentedProperties { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalAgents { get; set; }
    public IReadOnlyList<DashboardPropertyDto> RecentProperties { get; set; } = [];
}
