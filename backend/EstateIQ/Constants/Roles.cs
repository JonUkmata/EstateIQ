namespace EstateIQ.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string CompanyAdmin = "CompanyAdmin";
    public const string Agent = "Agent";
    public const string User = "User";

    public const string PropertyManagers = $"{Admin},{CompanyAdmin},{Agent}";
}
