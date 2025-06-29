using AccountManagementSystem.Data;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace AccountManagementSystem.Controllers
{
    public class Account
    {
        private readonly DbContext _context;

        public Account(DbContext context)
        {
            _context = context;
        }

        public DataTable GetAccounts()
        {
            using (var connection = _context.CreateConnection())
            {
                var command = new System.Data.SqlClient.SqlCommand("sp_ManageChartOfAccounts", (System.Data.SqlClient.SqlConnection)connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Action", "SELECTALL");

                var adapter = new System.Data.SqlClient.SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                return dataTable;
            }
        }

        public int AddAccount(string accountCode, string accountName, int? parentAccountId, string accountType, bool isActive)
        {
            using (var connection = _context.CreateConnection())
            {
                var command = new System.Data.SqlClient.SqlCommand("sp_ManageChartOfAccounts", (System.Data.SqlClient.SqlConnection)connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Action", "INSERT");
                command.Parameters.AddWithValue("@AccountCode", accountCode);
                command.Parameters.AddWithValue("@AccountName", accountName);
                command.Parameters.AddWithValue("@ParentAccountId", parentAccountId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AccountType", accountType);
                command.Parameters.AddWithValue("@IsActive", isActive);

                connection.Open();
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }
    }
}