using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Genius.Atom.Data.Ef.Tests.TestData;

/// <summary>
/// Hand-written migrations for <see cref="TestItemsDbContext"/>: the initial schema (Id + Name)
/// and a follow-up migration adding the Description column, mirroring the real-world scenario
/// of a database created before the follow-up migration existed.
/// </summary>
[DbContext(typeof(TestItemsDbContext))]
[Migration("20240101000000_InitialCreate")]
public sealed class TestItemsInitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Items",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Items", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Items");
    }
}

[DbContext(typeof(TestItemsDbContext))]
[Migration("20240201000000_AddItemDescription")]
public sealed class TestItemsAddItemDescription : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Items",
            type: "TEXT",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Description", table: "Items");
    }
}
