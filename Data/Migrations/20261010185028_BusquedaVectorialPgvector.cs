using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaOnline.Data.Migrations
{
    /// <summary>
    /// Búsqueda semántica en PostgreSQL: extensión pgvector, tabla de embeddings (384 dimensiones,
    /// modelo paraphrase-multilingual-MiniLM-L12-v2) e índice HNSW por distancia coseno (operador &lt;=&gt;).
    /// En RDS la extensión la crea el usuario maestro (rol rds_superuser).
    /// </summary>
    public partial class BusquedaVectorialPgvector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "ProductEmbeddings" (
                    "ProductId"  integer PRIMARY KEY REFERENCES "Products"("Id") ON DELETE CASCADE,
                    "Embedding"  vector(384) NOT NULL,
                    "SourceHash" varchar(64) NOT NULL,
                    "UpdatedUtc" timestamptz NOT NULL DEFAULT now());
                """);
            // HNSW: índice aproximado de vecinos más cercanos; vector_cosine_ops acelera ORDER BY "Embedding" <=> consulta.
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ProductEmbeddings_Embedding_hnsw"
                ON "ProductEmbeddings" USING hnsw ("Embedding" vector_cosine_ops);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ProductEmbeddings_Embedding_hnsw";""");
            migrationBuilder.Sql("""DROP TABLE IF EXISTS "ProductEmbeddings";""");
            // La extensión vector se deja instalada: otras bases o esquemas podrían usarla.
        }
    }
}
