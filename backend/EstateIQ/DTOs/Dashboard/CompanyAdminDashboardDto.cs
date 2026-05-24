namespace EstateIQ.DTOs.Dashboard;

public class CompanyAdminDashboardDto
{
    public string Role { get; set; } = "CompanyAdmin";
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int CompanyProperties { get; set; }
    public int CompanyAgents { get; set; }
    public int ForSaleProperties { get; set; }
    public int ForRentProperties { get; set; }
    public int SoldProperties { get; set; }
    public int RentedProperties { get; set; }
    public IReadOnlyList<DashboardPropertyDto> RecentCompanyProperties { get; set; } = [];
}
