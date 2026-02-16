namespace BamDb
{
    /// <summary>
    /// Specifies the default network ports for supported database engines.
    /// </summary>
    public enum DefaultPorts
    {
        /// <summary>
        /// Default port for MySQL (3306).
        /// </summary>
        MySql = 3306,

        /// <summary>
        /// Default port for PostgreSQL (5432).
        /// </summary>
        Postgres = 5432,

        /// <summary>
        /// Default port for Microsoft SQL Server (1433).
        /// </summary>
        MsSql = 1433,

        /// <summary>
        /// Default port for Oracle (1521).
        /// </summary>
        Oracle = 1521
    }
}
