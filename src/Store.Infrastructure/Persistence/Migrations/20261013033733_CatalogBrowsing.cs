using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogBrowsing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_category_id",
                table: "products");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Rows created before products had a slug get one from their SKU, which is already unique.
            migrationBuilder.Sql("UPDATE products SET slug = lower(sku) WHERE slug = '';");

            migrationBuilder.AddColumn<decimal>(
                name: "final_price_amount",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                computedColumnSql: "round(price_amount * (100 - coalesce(discount_percentage, 0)) / 100, CASE price_currency WHEN 'COP' THEN 0 WHEN 'USD' THEN 2 END)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id_final_price_amount",
                table: "products",
                columns: new[] { "category_id", "final_price_amount" });

            migrationBuilder.CreateIndex(
                name: "ix_products_name",
                table: "products",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_products_slug",
                table: "products",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_category_id_final_price_amount",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_name",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_slug",
                table: "products");

            migrationBuilder.DropColumn(
                name: "final_price_amount",
                table: "products");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "products");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "products");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");
        }
    }
}
