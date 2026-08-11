using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Garage.Web.Migrations
{
    /// <inheritdoc />
    public partial class applicationuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingAllocation_ParkingSession_ParkingSessionId",
                table: "ParkingAllocation");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingAllocation_ParkingSpot_ParkingSpotId",
                table: "ParkingAllocation");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSession_Vehicles_VehicleId",
                table: "ParkingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSpot_VehicleTypes_VehicleTypeId",
                table: "ParkingSpot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSpot",
                table: "ParkingSpot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSession",
                table: "ParkingSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingAllocation",
                table: "ParkingAllocation");

            migrationBuilder.RenameTable(
                name: "ParkingSpot",
                newName: "ParkingSpots");

            migrationBuilder.RenameTable(
                name: "ParkingSession",
                newName: "ParkingSessions");

            migrationBuilder.RenameTable(
                name: "ParkingAllocation",
                newName: "ParkingAllocations");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSpot_VehicleTypeId",
                table: "ParkingSpots",
                newName: "IX_ParkingSpots_VehicleTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSpot_SpotNumber",
                table: "ParkingSpots",
                newName: "IX_ParkingSpots_SpotNumber");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSession_VehicleId",
                table: "ParkingSessions",
                newName: "IX_ParkingSessions_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingAllocation_ParkingSpotId",
                table: "ParkingAllocations",
                newName: "IX_ParkingAllocations_ParkingSpotId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSpots",
                table: "ParkingSpots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSessions",
                table: "ParkingSessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingAllocations",
                table: "ParkingAllocations",
                columns: new[] { "ParkingSessionId", "ParkingSpotId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingAllocations_ParkingSessions_ParkingSessionId",
                table: "ParkingAllocations",
                column: "ParkingSessionId",
                principalTable: "ParkingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingAllocations_ParkingSpots_ParkingSpotId",
                table: "ParkingAllocations",
                column: "ParkingSpotId",
                principalTable: "ParkingSpots",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSessions_Vehicles_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSpots_VehicleTypes_VehicleTypeId",
                table: "ParkingSpots",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingAllocations_ParkingSessions_ParkingSessionId",
                table: "ParkingAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingAllocations_ParkingSpots_ParkingSpotId",
                table: "ParkingAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSessions_Vehicles_VehicleId",
                table: "ParkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSpots_VehicleTypes_VehicleTypeId",
                table: "ParkingSpots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSpots",
                table: "ParkingSpots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSessions",
                table: "ParkingSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingAllocations",
                table: "ParkingAllocations");

            migrationBuilder.RenameTable(
                name: "ParkingSpots",
                newName: "ParkingSpot");

            migrationBuilder.RenameTable(
                name: "ParkingSessions",
                newName: "ParkingSession");

            migrationBuilder.RenameTable(
                name: "ParkingAllocations",
                newName: "ParkingAllocation");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSpots_VehicleTypeId",
                table: "ParkingSpot",
                newName: "IX_ParkingSpot_VehicleTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSpots_SpotNumber",
                table: "ParkingSpot",
                newName: "IX_ParkingSpot_SpotNumber");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSessions_VehicleId",
                table: "ParkingSession",
                newName: "IX_ParkingSession_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingAllocations_ParkingSpotId",
                table: "ParkingAllocation",
                newName: "IX_ParkingAllocation_ParkingSpotId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSpot",
                table: "ParkingSpot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSession",
                table: "ParkingSession",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingAllocation",
                table: "ParkingAllocation",
                columns: new[] { "ParkingSessionId", "ParkingSpotId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingAllocation_ParkingSession_ParkingSessionId",
                table: "ParkingAllocation",
                column: "ParkingSessionId",
                principalTable: "ParkingSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingAllocation_ParkingSpot_ParkingSpotId",
                table: "ParkingAllocation",
                column: "ParkingSpotId",
                principalTable: "ParkingSpot",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSession_Vehicles_VehicleId",
                table: "ParkingSession",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSpot_VehicleTypes_VehicleTypeId",
                table: "ParkingSpot",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
