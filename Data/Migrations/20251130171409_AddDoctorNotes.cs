using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorNotes",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorNotes",
                table: "Appointments");
        }
    }
}
