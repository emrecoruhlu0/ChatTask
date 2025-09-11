using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatTask.TaskService.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(day, 7, GETUTCDATE())"),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaskGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignments_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_TaskId_UserId",
                table: "TaskAssignments",
                columns: new[] { "TaskId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_UserId_Status",
                table: "TaskAssignments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status_Priority",
                table: "Tasks",
                columns: new[] { "Status", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskAssignments");

            migrationBuilder.DropTable(
                name: "Tasks");
        }
    }
}
