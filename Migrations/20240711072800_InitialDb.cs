using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cloud.Migrations
{
	/// <inheritdoc />
	public partial class InitialDb : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "ActivityLogs",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<Guid>(type: "uuid", nullable: false),
					Action = table.Column<string>(type: "text", nullable: false),
					Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					Details = table.Column<string>(type: "text", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ActivityLogs", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ApplicationDocuments",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					RentalApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
					FileName = table.Column<string>(type: "text", nullable: false),
					FilePath = table.Column<string>(type: "text", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ApplicationDocuments", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "AspNetRoles",
				columns: table => new
				{
					Id = table.Column<string>(type: "text", nullable: false),
					Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
					NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
					ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetRoles", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUsers",
				columns: table => new
				{
					Id = table.Column<string>(type: "text", nullable: false),
					FirstName = table.Column<string>(type: "text", nullable: false),
					LastName = table.Column<string>(type: "text", nullable: false),
					Role = table.Column<int>(type: "integer", nullable: false),
					IsVerified = table.Column<bool>(type: "boolean", nullable: false),
					IsBanned = table.Column<bool>(type: "boolean", nullable: false),
					BanReason = table.Column<string>(type: "text", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					CreatedBy = table.Column<string>(type: "text", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
					UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
					NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
					Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
					NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
					EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
					PasswordHash = table.Column<string>(type: "text", nullable: true),
					SecurityStamp = table.Column<string>(type: "text", nullable: true),
					ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
					PhoneNumber = table.Column<string>(type: "text", nullable: true),
					PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
					TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
					LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
					LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
					AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUsers", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "AspNetRoleClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					RoleId = table.Column<string>(type: "text", nullable: false),
					ClaimType = table.Column<string>(type: "text", nullable: true),
					ClaimValue = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
						column: x => x.RoleId,
						principalTable: "AspNetRoles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Admins",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<string>(type: "text", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Admins", x => x.Id);
					table.ForeignKey(
						name: "FK_Admins_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					UserId = table.Column<string>(type: "text", nullable: false),
					ClaimType = table.Column<string>(type: "text", nullable: true),
					ClaimValue = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetUserClaims_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserLogins",
				columns: table => new
				{
					LoginProvider = table.Column<string>(type: "text", nullable: false),
					ProviderKey = table.Column<string>(type: "text", nullable: false),
					ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
					UserId = table.Column<string>(type: "text", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
					table.ForeignKey(
						name: "FK_AspNetUserLogins_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserRoles",
				columns: table => new
				{
					UserId = table.Column<string>(type: "text", nullable: false),
					RoleId = table.Column<string>(type: "text", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
					table.ForeignKey(
						name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
						column: x => x.RoleId,
						principalTable: "AspNetRoles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AspNetUserRoles_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserTokens",
				columns: table => new
				{
					UserId = table.Column<string>(type: "text", nullable: false),
					LoginProvider = table.Column<string>(type: "text", nullable: false),
					Name = table.Column<string>(type: "text", nullable: false),
					Value = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
					table.ForeignKey(
						name: "FK_AspNetUserTokens_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Medias",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<string>(type: "text", nullable: false),
					FileName = table.Column<string>(type: "text", nullable: false),
					FilePath = table.Column<string>(type: "text", nullable: false),
					FileType = table.Column<string>(type: "text", nullable: false),
					FileSize = table.Column<long>(type: "bigint", nullable: false),
					UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Medias", x => x.Id);
					table.ForeignKey(
						name: "FK_Medias_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Owners",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<string>(type: "text", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Owners", x => x.Id);
					table.ForeignKey(
						name: "FK_Owners_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "StripeCustomers",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<string>(type: "text", nullable: false),
					StripeCustomerId = table.Column<string>(type: "text", nullable: false),
					IsVerified = table.Column<bool>(type: "boolean", nullable: false),
					DefaultPaymentMethodId = table.Column<string>(type: "text", nullable: true),
					DefaultSourceId = table.Column<string>(type: "text", nullable: true),
					Balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					Currency = table.Column<string>(type: "text", nullable: true),
					Delinquent = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					InvoicePrefix = table.Column<string>(type: "text", nullable: true),
					InvoiceSequence = table.Column<int>(type: "integer", nullable: true),
					BusinessVatId = table.Column<string>(type: "text", nullable: true),
					AccountType = table.Column<string>(type: "text", nullable: true),
					Metadata = table.Column<string>(type: "text", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_StripeCustomers", x => x.Id);
					table.ForeignKey(
						name: "FK_StripeCustomers_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Properties",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
					Address = table.Column<string>(type: "text", nullable: false),
					City = table.Column<string>(type: "text", nullable: false),
					State = table.Column<string>(type: "text", nullable: false),
					ZipCode = table.Column<string>(type: "text", nullable: false),
					PropertyType = table.Column<int>(type: "integer", nullable: false),
					Bedrooms = table.Column<int>(type: "integer", nullable: false),
					Bathrooms = table.Column<int>(type: "integer", nullable: false),
					RentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					Description = table.Column<string>(type: "text", nullable: true),
					Amenities = table.Column<List<string>>(type: "text[]", nullable: true),
					IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
					RoomType = table.Column<int>(type: "integer", nullable: false),
					ImageUrls = table.Column<List<string>>(type: "text[]", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Properties", x => x.Id);
					table.ForeignKey(
						name: "FK_Properties_Owners_OwnerId",
						column: x => x.OwnerId,
						principalTable: "Owners",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Listings",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
					Title = table.Column<string>(type: "text", nullable: false),
					Description = table.Column<string>(type: "text", nullable: true),
					Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsActive = table.Column<bool>(type: "boolean", nullable: false),
					Views = table.Column<int>(type: "integer", nullable: false),
					PropertyModelId = table.Column<Guid>(type: "uuid", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Listings", x => x.Id);
					table.ForeignKey(
						name: "FK_Listings_Properties_PropertyId",
						column: x => x.PropertyId,
						principalTable: "Properties",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Listings_Properties_PropertyModelId",
						column: x => x.PropertyModelId,
						principalTable: "Properties",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "OwnerPayments",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
					PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
					Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					AdminFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					UtilityFees = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					MaintenanceCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					StripePaymentIntentId = table.Column<string>(type: "text", nullable: false),
					Status = table.Column<int>(type: "integer", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
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

			migrationBuilder.CreateTable(
				name: "Tenants",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					UserId = table.Column<string>(type: "text", nullable: false),
					CurrentPropertyId = table.Column<Guid>(type: "uuid", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tenants", x => x.Id);
					table.ForeignKey(
						name: "FK_Tenants_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Tenants_Properties_CurrentPropertyId",
						column: x => x.CurrentPropertyId,
						principalTable: "Properties",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "Leases",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					TenantId = table.Column<Guid>(type: "uuid", nullable: false),
					PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
					StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					RentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					SecurityDeposit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
					IsActive = table.Column<bool>(type: "boolean", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Leases", x => x.Id);
					table.ForeignKey(
						name: "FK_Leases_Properties_PropertyId",
						column: x => x.PropertyId,
						principalTable: "Properties",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Leases_Tenants_TenantId",
						column: x => x.TenantId,
						principalTable: "Tenants",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "MaintenanceRequests",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					TenantId = table.Column<Guid>(type: "uuid", nullable: false),
					PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
					Description = table.Column<string>(type: "text", nullable: false),
					Status = table.Column<int>(type: "integer", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MaintenanceRequests", x => x.Id);
					table.ForeignKey(
						name: "FK_MaintenanceRequests_Properties_PropertyId",
						column: x => x.PropertyId,
						principalTable: "Properties",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_MaintenanceRequests_Tenants_TenantId",
						column: x => x.TenantId,
						principalTable: "Tenants",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "RentalApplications",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					TenantId = table.Column<Guid>(type: "uuid", nullable: false),
					ListingId = table.Column<Guid>(type: "uuid", nullable: false),
					Status = table.Column<int>(type: "integer", nullable: false),
					ApplicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					EmploymentInfo = table.Column<string>(type: "text", nullable: true),
					References = table.Column<string>(type: "text", nullable: true),
					AdditionalNotes = table.Column<string>(type: "text", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RentalApplications", x => x.Id);
					table.ForeignKey(
						name: "FK_RentalApplications_Listings_ListingId",
						column: x => x.ListingId,
						principalTable: "Listings",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_RentalApplications_Tenants_TenantId",
						column: x => x.TenantId,
						principalTable: "Tenants",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "RentPayments",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					TenantId = table.Column<Guid>(type: "uuid", nullable: false),
					Amount = table.Column<int>(type: "integer", nullable: false),
					Currency = table.Column<string>(type: "text", nullable: false),
					PaymentIntentId = table.Column<string>(type: "text", nullable: false),
					PaymentMethodId = table.Column<string>(type: "text", nullable: true),
					Status = table.Column<int>(type: "integer", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RentPayments", x => x.Id);
					table.ForeignKey(
						name: "FK_RentPayments_Tenants_TenantId",
						column: x => x.TenantId,
						principalTable: "Tenants",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "MaintenanceTasks",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					RequestId = table.Column<Guid>(type: "uuid", nullable: false),
					Description = table.Column<string>(type: "text", nullable: false),
					EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
					ActualCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
					StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					Status = table.Column<int>(type: "integer", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
					DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MaintenanceTasks", x => x.Id);
					table.ForeignKey(
						name: "FK_MaintenanceTasks_MaintenanceRequests_RequestId",
						column: x => x.RequestId,
						principalTable: "MaintenanceRequests",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Admins_UserId",
				table: "Admins",
				column: "UserId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_AspNetRoleClaims_RoleId",
				table: "AspNetRoleClaims",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "RoleNameIndex",
				table: "AspNetRoles",
				column: "NormalizedName",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserClaims_UserId",
				table: "AspNetUserClaims",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserLogins_UserId",
				table: "AspNetUserLogins",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserRoles_RoleId",
				table: "AspNetUserRoles",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "EmailIndex",
				table: "AspNetUsers",
				column: "NormalizedEmail");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUsers_Email",
				table: "AspNetUsers",
				column: "Email",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "UserNameIndex",
				table: "AspNetUsers",
				column: "NormalizedUserName",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Leases_PropertyId",
				table: "Leases",
				column: "PropertyId");

			migrationBuilder.CreateIndex(
				name: "IX_Leases_TenantId",
				table: "Leases",
				column: "TenantId");

			migrationBuilder.CreateIndex(
				name: "IX_Listings_PropertyId",
				table: "Listings",
				column: "PropertyId");

			migrationBuilder.CreateIndex(
				name: "IX_Listings_PropertyModelId",
				table: "Listings",
				column: "PropertyModelId");

			migrationBuilder.CreateIndex(
				name: "IX_MaintenanceRequests_PropertyId",
				table: "MaintenanceRequests",
				column: "PropertyId");

			migrationBuilder.CreateIndex(
				name: "IX_MaintenanceRequests_TenantId",
				table: "MaintenanceRequests",
				column: "TenantId");

			migrationBuilder.CreateIndex(
				name: "IX_MaintenanceTasks_RequestId",
				table: "MaintenanceTasks",
				column: "RequestId");

			migrationBuilder.CreateIndex(
				name: "IX_Medias_UserId",
				table: "Medias",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_OwnerPayments_OwnerId",
				table: "OwnerPayments",
				column: "OwnerId");

			migrationBuilder.CreateIndex(
				name: "IX_OwnerPayments_PropertyId",
				table: "OwnerPayments",
				column: "PropertyId");

			migrationBuilder.CreateIndex(
				name: "IX_Owners_UserId",
				table: "Owners",
				column: "UserId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Properties_OwnerId",
				table: "Properties",
				column: "OwnerId");

			migrationBuilder.CreateIndex(
				name: "IX_RentalApplications_ListingId",
				table: "RentalApplications",
				column: "ListingId");

			migrationBuilder.CreateIndex(
				name: "IX_RentalApplications_TenantId",
				table: "RentalApplications",
				column: "TenantId");

			migrationBuilder.CreateIndex(
				name: "IX_RentPayments_TenantId",
				table: "RentPayments",
				column: "TenantId");

			migrationBuilder.CreateIndex(
				name: "IX_StripeCustomers_StripeCustomerId",
				table: "StripeCustomers",
				column: "StripeCustomerId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_StripeCustomers_UserId",
				table: "StripeCustomers",
				column: "UserId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Tenants_CurrentPropertyId",
				table: "Tenants",
				column: "CurrentPropertyId");

			migrationBuilder.CreateIndex(
				name: "IX_Tenants_UserId",
				table: "Tenants",
				column: "UserId",
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "ActivityLogs");

			migrationBuilder.DropTable(
				name: "Admins");

			migrationBuilder.DropTable(
				name: "ApplicationDocuments");

			migrationBuilder.DropTable(
				name: "AspNetRoleClaims");

			migrationBuilder.DropTable(
				name: "AspNetUserClaims");

			migrationBuilder.DropTable(
				name: "AspNetUserLogins");

			migrationBuilder.DropTable(
				name: "AspNetUserRoles");

			migrationBuilder.DropTable(
				name: "AspNetUserTokens");

			migrationBuilder.DropTable(
				name: "Leases");

			migrationBuilder.DropTable(
				name: "MaintenanceTasks");

			migrationBuilder.DropTable(
				name: "Medias");

			migrationBuilder.DropTable(
				name: "OwnerPayments");

			migrationBuilder.DropTable(
				name: "RentalApplications");

			migrationBuilder.DropTable(
				name: "RentPayments");

			migrationBuilder.DropTable(
				name: "StripeCustomers");

			migrationBuilder.DropTable(
				name: "AspNetRoles");

			migrationBuilder.DropTable(
				name: "MaintenanceRequests");

			migrationBuilder.DropTable(
				name: "Listings");

			migrationBuilder.DropTable(
				name: "Tenants");

			migrationBuilder.DropTable(
				name: "Properties");

			migrationBuilder.DropTable(
				name: "Owners");

			migrationBuilder.DropTable(
				name: "AspNetUsers");
		}
	}
}