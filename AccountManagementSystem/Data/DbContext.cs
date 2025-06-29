using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace AccountManagementSystem.Data
{
    public class DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
