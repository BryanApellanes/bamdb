namespace BamDb
{
    /// <summary>
    /// Provides default Docker container image tags for supported database engines.
    /// </summary>
    public class DefaultImageTags
    {
        /// <summary>
        /// The default Docker image tag for Microsoft SQL Server (2022-latest).
        /// </summary>
        public const string MsSql = "mcr.microsoft.com/mssql/server:2022-latest";

        /// <summary>
        /// The default Docker image tag for MySQL (8.2).
        /// </summary>
        public const string MySql = "mysql:8.2";

        /// <summary>
        /// The default Docker image tag for PostgreSQL (latest).
        /// </summary>
        public const string Postgres = "postgres";


    }
}
