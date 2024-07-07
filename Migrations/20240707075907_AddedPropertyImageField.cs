using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cloud.Migrations
{
	/// <inheritdoc />
	public partial class AddedPropertyImageField : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<List<string>>(
				name: "ImageUrls",
				table: "Properties",
				type: "text[]",
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ImageUrls",
				table: "Properties");
		}
	}
}