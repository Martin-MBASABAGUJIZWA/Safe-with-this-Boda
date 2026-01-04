using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeBoda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Trips",
                columns: new[] { "Id", "DriverId", "Fare", "RequestTime", "RiderId", "End_Latitude", "End_Longitude", "Start_Latitude", "Start_Longitude" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("33333333-3333-3333-3333-333333333333"), 1500m, new DateTime(2025, 10, 22, 18, 19, 43, 191, DateTimeKind.Utc).AddTicks(3705), new Guid("22222222-2222-2222-2222-222222222222"), -1.9576499999999999, 30.091229999999999, -1.9499500000000001, 30.05885 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
