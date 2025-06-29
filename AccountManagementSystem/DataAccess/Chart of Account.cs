using System.Data;
using System.Data.SqlClient;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.DataAccess;

public class ChartOfAccountsDAL
{
    private readonly string _connectionString;

    public ChartOfAccountsDAL(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<ChartOfAccounts> ManageChartOfAccounts(
        string action,
        int? accountId = null,
        string? accountCode = null,
        string? accountName = null,
        string? accountType = null,
        int? parentAccountId = null,
        bool? isActive = null,
         int? outputAccountId = null)
    {
        outputAccountId = 0;
        List<ChartOfAccounts> accounts = new List<ChartOfAccounts>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", action);
                if (accountId.HasValue) cmd.Parameters.AddWithValue("@AccountId", accountId.Value);
                if (accountCode != null) cmd.Parameters.AddWithValue("@AccountCode", accountCode);
                if (accountName != null) cmd.Parameters.AddWithValue("@AccountName", accountName);
                if (accountType != null) cmd.Parameters.AddWithValue("@AccountType", accountType);
                if (parentAccountId.HasValue) cmd.Parameters.AddWithValue("@ParentAccountId", parentAccountId.Value);
                else cmd.Parameters.AddWithValue("@ParentAccountId", DBNull.Value);
                if (isActive.HasValue) cmd.Parameters.AddWithValue("@IsActive", isActive.Value);

                SqlParameter outputParam = new SqlParameter("@OutputAccountId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputParam);

                try
                {
                    conn.Open();
                    if (action == "Select" || action == "SelectParents")
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                accounts.Add(new ChartOfAccounts
                                {
                                    AccountId = reader.GetInt32("AccountId"),
                                    AccountCode = reader.GetString("AccountCode"),
                                    AccountName = reader.GetString("AccountName"),
                                    AccountType = reader.GetString("AccountType"),
                                    ParentAccountId = reader.IsDBNull("ParentAccountId") ? null : reader.GetInt32("ParentAccountId"),
                                    IsActive = reader.GetBoolean("IsActive"),
                                    CreatedDate = reader.GetDateTime("CreatedDate")
                                });
                            }
                        }
                    }
                    else
                    {
                        cmd.ExecuteNonQuery();
                    }

                    if (outputParam.Value != DBNull.Value)
                    {
                        outputAccountId = Convert.ToInt32(outputParam.Value);
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Database operation failed for action '{action}': {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred during action '{action}': {ex.Message}", ex);
                }
            }
        }
        return accounts;
    }

    public List<ChartOfAccounts> GetAccountsForDropdown()
    {
        List<ChartOfAccounts> accounts = new List<ChartOfAccounts>();
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("sp_GetAccountsForDropdown", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accounts.Add(new ChartOfAccounts
                            {
                                AccountId = reader.GetInt32("AccountId"),
                                AccountCode = reader.GetString("AccountCode"),
                                AccountName = reader.GetString("AccountName"),
                                AccountType = reader.GetString("AccountType")
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Database operation failed while fetching accounts for dropdown: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred while fetching accounts for dropdown: {ex.Message}", ex);
                }
            }
        }
        return accounts;
    }
}