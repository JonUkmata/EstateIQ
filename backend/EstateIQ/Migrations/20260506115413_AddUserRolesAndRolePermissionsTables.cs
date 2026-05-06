using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EstateIQ.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRolesAndRolePermissionsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e501"), new Guid("d7a9c1e3-4f5b-46a8-90c2-1e3d5f7a9b0c"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e502"), new Guid("4b2a0c8e-6f1d-43b5-9a7c-2e4f6d8b0a1c"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e503"), new Guid("9e1c3a5f-7b2d-40f6-8a9c-3d5e7f1b0c2a"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e504"), new Guid("0f2e4d6c-8b1a-45c7-9e3f-4a6b8d0c2e1f"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e505"), new Guid("5c7a9e1d-3f2b-48d0-86a4-5b7c9e1f3d2a"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e506"), new Guid("8d0b2e4f-6a1c-43e5-9b7d-6c8e0a2f4b1d"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e507"), new Guid("1a3c5e7f-9b0d-42f4-8c6a-7d9e1b3f5a0c"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e508"), new Guid("6b8d0f2e-4a1c-46e8-9d7b-8e0f2a4c6b1d"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e509"), new Guid("3f5a7c9e-1b0d-44f6-8a2c-9f1e3d5b7a0c"), new Guid("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a") },
                    { new Guid("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c601"), new Guid("9e1c3a5f-7b2d-40f6-8a9c-3d5e7f1b0c2a"), new Guid("2a7c9e4f-5b1d-42a8-86f3-1c9d0e7b6a5f") },
                    { new Guid("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c602"), new Guid("0f2e4d6c-8b1a-45c7-9e3f-4a6b8d0c2e1f"), new Guid("2a7c9e4f-5b1d-42a8-86f3-1c9d0e7b6a5f") },
                    { new Guid("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c603"), new Guid("5c7a9e1d-3f2b-48d0-86a4-5b7c9e1f3d2a"), new Guid("2a7c9e4f-5b1d-42a8-86f3-1c9d0e7b6a5f") },
                    { new Guid("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c604"), new Guid("8d0b2e4f-6a1c-43e5-9b7d-6c8e0a2f4b1d"), new Guid("2a7c9e4f-5b1d-42a8-86f3-1c9d0e7b6a5f") },
                    { new Guid("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c605"), new Guid("1a3c5e7f-9b0d-42f4-8c6a-7d9e1b3f5a0c"), new Guid("2a7c9e4f-5b1d-42a8-86f3-1c9d0e7b6a5f") },
                    { new Guid("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c606"), new Guid("6b8d0f2e-4a1c-46e8-9d7b-8e0f2a4c6b1d"), new Guid("2a7c9e4f-5b1d-42a8-86f3-1c9d0e7b6a5f") },
                    { new Guid("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d701"), new Guid("0f2e4d6c-8b1a-45c7-9e3f-4a6b8d0c2e1f"), new Guid("6f2d8a1b-4c3e-49f7-9a0b-5d6e7c8b9a01") },
                    { new Guid("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d702"), new Guid("5c7a9e1d-3f2b-48d0-86a4-5b7c9e1f3d2a"), new Guid("6f2d8a1b-4c3e-49f7-9a0b-5d6e7c8b9a01") },
                    { new Guid("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d703"), new Guid("8d0b2e4f-6a1c-43e5-9b7d-6c8e0a2f4b1d"), new Guid("6f2d8a1b-4c3e-49f7-9a0b-5d6e7c8b9a01") },
                    { new Guid("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d704"), new Guid("1a3c5e7f-9b0d-42f4-8c6a-7d9e1b3f5a0c"), new Guid("6f2d8a1b-4c3e-49f7-9a0b-5d6e7c8b9a01") },
                    { new Guid("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d705"), new Guid("6b8d0f2e-4a1c-46e8-9d7b-8e0f2a4c6b1d"), new Guid("6f2d8a1b-4c3e-49f7-9a0b-5d6e7c8b9a01") },
                    { new Guid("d6c9f4a7-8e01-4c32-9d5f-a7b8c9d0e801"), new Guid("6b8d0f2e-4a1c-46e8-9d7b-8e0f2a4c6b1d"), new Guid("1d3b5f7a-8c9e-4b2a-91d0-6e5f4c3b2a10") },
                    { new Guid("d6c9f4a7-8e01-4c32-9d5f-a7b8c9d0e802"), new Guid("3f5a7c9e-1b0d-44f6-8a2c-9f1e3d5b7a0c"), new Guid("1d3b5f7a-8c9e-4b2a-91d0-6e5f4c3b2a10") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");
        }
    }
}
