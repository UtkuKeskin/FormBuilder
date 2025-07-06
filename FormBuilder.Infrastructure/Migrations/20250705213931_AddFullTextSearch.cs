using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL Full-Text Search extensions
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            
            // Templates için search vector column
            migrationBuilder.AddColumn<string>(
                name: "SearchVector",
                table: "Templates",
                type: "tsvector",
                nullable: true);

            // Search vector'ü güncelleyen function
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_template_search_vector()
                RETURNS trigger AS $$
                BEGIN
                    NEW.""SearchVector"" = 
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""Title"", ''))), 'A') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""Description"", ''))), 'B') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomString1Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomString2Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomString3Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomString4Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomText1Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomText2Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomText3Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomText4Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomInt1Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomInt2Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomInt3Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomInt4Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomCheckbox1Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomCheckbox2Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomCheckbox3Question"", ''))), 'C') ||
                        setweight(to_tsvector('english', unaccent(COALESCE(NEW.""CustomCheckbox4Question"", ''))), 'C');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Trigger oluştur
            migrationBuilder.Sql(@"
                CREATE TRIGGER update_template_search_trigger
                BEFORE INSERT OR UPDATE ON ""Templates""
                FOR EACH ROW EXECUTE FUNCTION update_template_search_vector();
            ");

            // GIN index oluştur
            migrationBuilder.Sql(@"
                CREATE INDEX idx_template_search_vector 
                ON ""Templates"" USING GIN (""SearchVector"");
            ");

            // Mevcut kayıtları güncelle
            migrationBuilder.Sql(@"UPDATE ""Templates"" SET ""UpdatedAt"" = ""UpdatedAt"";");

            // Tags için de search index
            migrationBuilder.Sql(@"
                CREATE INDEX idx_tag_name_trgm 
                ON ""Tags"" USING gin (""Name"" gin_trgm_ops);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            try
            {
                migrationBuilder.Sql("DROP INDEX IF EXISTS idx_tag_name_trgm;");
                migrationBuilder.Sql("DROP INDEX IF EXISTS idx_template_search_vector;");
                migrationBuilder.Sql("DROP TRIGGER IF EXISTS update_template_search_trigger ON \"Templates\";");
                migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_template_search_vector();");
                migrationBuilder.DropColumn(name: "SearchVector", table: "Templates");
                
                // Don't drop extensions as they might be used elsewhere
                // migrationBuilder.Sql("DROP EXTENSION IF EXISTS unaccent;");
                // migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
            }
            catch (Exception ex)
            {
                // Log but don't fail - extensions might be used by other apps
                Console.WriteLine($"Migration rollback warning: {ex.Message}");
            }
        }
    }
}