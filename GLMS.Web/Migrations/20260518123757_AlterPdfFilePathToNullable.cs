using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GLMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AlterPdfFilePathToNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only alter the column, don't create tables
            migrationBuilder.Sql("ALTER TABLE Contracts ALTER COLUMN PdfFilePath NVARCHAR(MAX) NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to NOT NULL
            migrationBuilder.Sql("ALTER TABLE Contracts ALTER COLUMN PdfFilePath NVARCHAR(MAX) NOT NULL");
        }
    }
}
