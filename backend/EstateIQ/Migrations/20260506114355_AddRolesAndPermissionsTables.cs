using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EstateIQ.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissionsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("0f2e4d6c-8b1a-45c7-9e3f-4a6b8d0c2e1f"), "Create properties", "CreateProperty" },
                    { new Guid("1a3c5e7f-9b0d-42f4-8c6a-7d9e1b3f5a0c"), "Upload property images", "UploadPropertyImages" },
                    { new Guid("3f5a7c9e-1b0d-44f6-8a2c-9f1e3d5b7a0c"), "Book property viewings", "BookViewing" },
                    { new Guid("4b2a0c8e-6f1d-43b5-9a7c-2e4f6d8b0a1c"), "Manage companies", "ManageCompanies" },
                    { new Guid("5c7a9e1d-3f2b-48d0-86a4-5b7c9e1f3d2a"), "Edit properties", "EditProperty" },
                    { new Guid("6b8d0f2e-4a1c-46e8-9d7b-8e0f2a4c6b1d"), "View properties", "ViewProperties" },
                    { new Guid("8d0b2e4f-6a1c-43e5-9b7d-6c8e0a2f4b1d"), "Delete properties", "DeleteProperty" },
                    { new Guid("9e1c3a5f-7b2d-40f6-8a9c-3d5e7f1b0c2a"), "Manage agents", "ManageAgents" },
                    { new Guid("d7a9c1e3-4f5b-46a8-90c2-1e3d5f7a9b0c"), "Manage users", "ManageUsers" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("1d3b5f7a-8c9e-4b2a-91d0-6e5f4c3b2a10"), new DateTime(2026, 4, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Public user", "User" },
                    { new Guid("2a7c9e4f-5b1d-42a8-86f3-1c9d0e7b6a5f"), new DateTime(2026, 4, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Company administrator", "CompanyAdmin" },
                    { new Guid("6f2d8a1b-4c3e-49f7-9a0b-5d6e7c8b9a01"), new DateTime(2026, 4, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Real estate agent", "Agent" },
                    { new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a"), new DateTime(2026, 4, 22, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator", "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
