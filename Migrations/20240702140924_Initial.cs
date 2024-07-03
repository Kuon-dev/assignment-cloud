using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cloud.Migrations {
  /// <inheritdoc />
  public partial class Initial : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
	  migrationBuilder.DropForeignKey(
		  name: "FK_Leases_Units_UnitId",
		  table: "Leases");

	  migrationBuilder.DropTable(
		  name: "Units");

	  migrationBuilder.DropIndex(
		  name: "IX_Leases_UnitId",
		  table: "Leases");

	  migrationBuilder.DropColumn(
		  name: "SquareFootage",
		  table: "Properties");

	  migrationBuilder.DropColumn(
		  name: "Amenities",
		  table: "Listings");

	  migrationBuilder.DropColumn(
		  name: "Bedrooms",
		  table: "Listings");

	  migrationBuilder.DropColumn(
		  name: "Location",
		  table: "Listings");

	  migrationBuilder.AddColumn<int>(
		  name: "RoomType",
		  table: "Properties",
		  type: "integer",
		  nullable: false,
		  defaultValue: 0);

	  migrationBuilder.CreateTable(
		  name: "ApplicationDocuments",
		  columns: table => new {
			Id = table.Column<Guid>(type: "uuid", nullable: false),
			RentalApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
			FileName = table.Column<string>(type: "text", nullable: false),
			FilePath = table.Column<string>(type: "text", nullable: false),
			UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
		  },
		  constraints: table => {
			table.PrimaryKey("PK_ApplicationDocuments", x => x.Id);
		  });
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
	  migrationBuilder.DropTable(
		  name: "ApplicationDocuments");

	  migrationBuilder.DropColumn(
		  name: "RoomType",
		  table: "Properties");

	  migrationBuilder.AddColumn<float>(
		  name: "SquareFootage",
		  table: "Properties",
		  type: "real",
		  nullable: false,
		  defaultValue: 0f);

	  migrationBuilder.AddColumn<string>(
		  name: "Amenities",
		  table: "Listings",
		  type: "text",
		  nullable: false,
		  defaultValue: "");

	  migrationBuilder.AddColumn<int>(
		  name: "Bedrooms",
		  table: "Listings",
		  type: "integer",
		  nullable: false,
		  defaultValue: 0);

	  migrationBuilder.AddColumn<string>(
		  name: "Location",
		  table: "Listings",
		  type: "text",
		  nullable: false,
		  defaultValue: "");

	  migrationBuilder.CreateTable(
		  name: "Units",
		  columns: table => new {
			Id = table.Column<Guid>(type: "uuid", nullable: false),
			PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
			IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
			UnitNumber = table.Column<string>(type: "text", nullable: false)
		  },
		  constraints: table => {
			table.PrimaryKey("PK_Units", x => x.Id);
			table.ForeignKey(
					  name: "FK_Units_Properties_PropertyId",
					  column: x => x.PropertyId,
					  principalTable: "Properties",
					  principalColumn: "Id",
					  onDelete: ReferentialAction.Cascade);
		  });

	  migrationBuilder.CreateIndex(
		  name: "IX_Leases_UnitId",
		  table: "Leases",
		  column: "UnitId");

	  migrationBuilder.CreateIndex(
		  name: "IX_Units_PropertyId",
		  table: "Units",
		  column: "PropertyId");

	  migrationBuilder.AddForeignKey(
		  name: "FK_Leases_Units_UnitId",
		  table: "Leases",
		  column: "UnitId",
		  principalTable: "Units",
		  principalColumn: "Id",
		  onDelete: ReferentialAction.Cascade);
	}
  }
}