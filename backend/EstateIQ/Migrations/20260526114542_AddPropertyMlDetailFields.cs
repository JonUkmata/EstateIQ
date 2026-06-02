using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstateIQ.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyMlDetailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Floors",
                table: "Properties",
                type: "decimal(4,1)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Bathrooms",
                table: "Properties",
                type: "decimal(4,1)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasementArea",
                table: "Properties",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BasementAreaUnit",
                table: "Properties",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBasement",
                table: "Properties",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LotArea",
                table: "Properties",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LotAreaUnit",
                table: "Properties",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NearbyLivingArea",
                table: "Properties",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NearbyLivingAreaUnit",
                table: "Properties",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NearbyLotArea",
                table: "Properties",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NearbyLotAreaUnit",
                table: "Properties",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Renovated",
                table: "Properties",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewQuality",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Waterfront",
                table: "Properties",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearRenovated",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Zipcode",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_BasementArea",
                table: "Properties",
                sql: "[BasementArea] IS NULL OR [BasementArea] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_Condition",
                table: "Properties",
                sql: "[Condition] IS NULL OR ([Condition] >= 1 AND [Condition] <= 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_Grade",
                table: "Properties",
                sql: "[Grade] IS NULL OR ([Grade] >= 1 AND [Grade] <= 13)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_LotArea",
                table: "Properties",
                sql: "[LotArea] IS NULL OR [LotArea] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_NearbyLivingArea",
                table: "Properties",
                sql: "[NearbyLivingArea] IS NULL OR [NearbyLivingArea] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_NearbyLotArea",
                table: "Properties",
                sql: "[NearbyLotArea] IS NULL OR [NearbyLotArea] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_ViewQuality",
                table: "Properties",
                sql: "[ViewQuality] IS NULL OR ([ViewQuality] >= 0 AND [ViewQuality] <= 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_YearRenovated",
                table: "Properties",
                sql: "[YearRenovated] IS NULL OR ([YearRenovated] >= 1800 AND [YearRenovated] <= YEAR(GETDATE()))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_Zipcode",
                table: "Properties",
                sql: "[Zipcode] IS NULL OR ([Zipcode] >= 1 AND [Zipcode] <= 99999)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_BasementArea",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_Condition",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_Grade",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_LotArea",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_NearbyLivingArea",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_NearbyLotArea",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_ViewQuality",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_YearRenovated",
                table: "Properties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_Zipcode",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BasementArea",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BasementAreaUnit",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "HasBasement",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "LotArea",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "LotAreaUnit",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "NearbyLivingArea",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "NearbyLivingAreaUnit",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "NearbyLotArea",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "NearbyLotAreaUnit",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Renovated",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ViewQuality",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Waterfront",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "YearRenovated",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Zipcode",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "Floors",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Bathrooms",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)",
                oldNullable: true);
        }
    }
}
