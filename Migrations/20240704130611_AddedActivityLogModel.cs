using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cloud.Migrations {
  /// <inheritdoc />
  public partial class AddedActivityLogModel : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
	  migrationBuilder.CreateTable(
		  name: "ActivityLogs",
		  columns: table => new {
			Id = table.Column<Guid>(type: "uuid", nullable: false),
			UserId = table.Column<Guid>(type: "uuid", nullable: false),
			Action = table.Column<string>(type: "text", nullable: false),
			Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
			Details = table.Column<string>(type: "text", nullable: false)
		  },
		  constraints: table => {
			table.PrimaryKey("PK_ActivityLogs", x => x.Id);
		  });
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
	  migrationBuilder.DropTable(
		  name: "ActivityLogs");
	}
  }
}