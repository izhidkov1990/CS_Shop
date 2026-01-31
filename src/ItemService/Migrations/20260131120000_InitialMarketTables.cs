using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ItemService.Migrations
{
    public partial class InitialMarketTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerSteamId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AppId = table.Column<int>(type: "integer", nullable: false),
                    ContextId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssetId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClassId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InstanceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MarketHashName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IconUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BuyerSteamId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SoldAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketListings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerSteamId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_MarketListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "MarketListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerSteamId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SellerSteamId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MarketHashName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IconUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<int>(type: "integer", nullable: false),
                    ContextId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssetId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClassId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InstanceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_MarketListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "MarketListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_BuyerSteamId_ListingId",
                table: "CartItems",
                columns: new[] { "BuyerSteamId", "ListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ListingId",
                table: "CartItems",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_SellerSteamId_AssetId_Status",
                table: "MarketListings",
                columns: new[] { "SellerSteamId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_SellerSteamId_Status",
                table: "MarketListings",
                columns: new[] { "SellerSteamId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_Status_CreatedAt",
                table: "MarketListings",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_BuyerSteamId_PurchasedAt",
                table: "Purchases",
                columns: new[] { "BuyerSteamId", "PurchasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ListingId",
                table: "Purchases",
                column: "ListingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "MarketListings");
        }
    }
}
