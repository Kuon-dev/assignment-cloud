using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cloud.Migrations {
  /// <inheritdoc />
  public partial class AddedOwnerPaymentModel : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
	  migrationBuilder.CreateTable(
		  name: "OwnerPayments",
		  columns: table => new {
			Id = table.Column<Guid>(type: "uuid", nullable: false),
			OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
			PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
			Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
			PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
			AdminFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
			UtilityFees = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
			MaintenanceCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
			StripePaymentIntentId = table.Column<string>(type: "text", nullable: false),
			Status = table.Column<int>(type: "integer", nullable: false)
		  },
		  constraints: table => {
			table.PrimaryKey("PK_OwnerPayments", x => x.Id);
			table.ForeignKey(
					  name: "FK_OwnerPayments_Owners_OwnerId",
					  column: x => x.OwnerId,
					  principalTable: "Owners",
					  principalColumn: "Id",
					  onDelete: ReferentialAction.Cascade);
			table.ForeignKey(
					  name: "FK_OwnerPayments_Properties_PropertyId",
					  column: x => x.PropertyId,
					  principalTable: "Properties",
					  principalColumn: "Id",
					  onDelete: ReferentialAction.Cascade);
		  });

	  migrationBuilder.CreateIndex(
		  name: "IX_OwnerPayments_OwnerId",
		  table: "OwnerPayments",
		  column: "OwnerId");

	  migrationBuilder.CreateIndex(
		  name: "IX_OwnerPayments_PropertyId",
		  table: "OwnerPayments",
		  column: "PropertyId");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
	  migrationBuilder.DropTable(
		  name: "OwnerPayments");
	}
  }
}