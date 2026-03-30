using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealState_Platform.Migrations
{
    /// <inheritdoc />
    public partial class BuyProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AspNetUsers_AgentId",
                table: "Properties");

            migrationBuilder.AddColumn<string>(
                name: "BuyerId",
                table: "Properties",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SoldAt",
                table: "Properties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_BuyerId",
                table: "Properties",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AspNetUsers_AgentId",
                table: "Properties",
                column: "AgentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AspNetUsers_BuyerId",
                table: "Properties",
                column: "BuyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AspNetUsers_AgentId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AspNetUsers_BuyerId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_BuyerId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "SoldAt",
                table: "Properties");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AspNetUsers_AgentId",
                table: "Properties",
                column: "AgentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
