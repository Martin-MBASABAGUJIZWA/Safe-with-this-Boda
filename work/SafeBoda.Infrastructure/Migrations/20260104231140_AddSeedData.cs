using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeBoda.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "Id", "MotoPlateNumber", "Name", "PhoneNumber" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), "RAA123A", "Test Driver", "0987654321" });

            migrationBuilder.InsertData(
                table: "Riders",
                columns: new[] { "Id", "Name", "PhoneNumber" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "Test Rider", "1234567890" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Riders",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));
        }
    }
}
