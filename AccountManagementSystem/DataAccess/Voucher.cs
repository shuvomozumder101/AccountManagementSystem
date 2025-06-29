using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.DataAccess;
public class VoucherDAL
{
    private readonly string _connectionString;

    public VoucherDAL(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int SaveVoucher(Voucher voucher)
    {
        int outputVoucherId = 0;
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("sp_SaveVoucher", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@VoucherType", voucher.VoucherType);
                cmd.Parameters.AddWithValue("@VoucherDate", voucher.VoucherDate);
                cmd.Parameters.AddWithValue("@ReferenceNo", voucher.ReferenceNo);
                cmd.Parameters.AddWithValue("@Description", (object?)voucher.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", voucher.CreatedBy);

                XElement voucherDetailsXml = new XElement("VoucherDetails",
                    voucher.Details.Select(d => new XElement("Detail",
                        new XElement("AccountId", d.AccountId),
                        new XElement("DebitAmount", d.DebitAmount),
                        new XElement("CreditAmount", d.CreditAmount),
                        new XElement("Narration", (object?)d.Narration ?? DBNull.Value)
                    ))
                );
                cmd.Parameters.AddWithValue("@VoucherDetailsXml", voucherDetailsXml.ToString());

                SqlParameter outputParam = new SqlParameter("@OutputVoucherId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputParam);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    if (outputParam.Value != DBNull.Value)
                    {
                        outputVoucherId = Convert.ToInt32(outputParam.Value);
                    }
                    else
                    {
                        Console.WriteLine("Stored procedure did not return a VoucherId.");
                        outputVoucherId = -1;
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number >= 50000)
                    {
                        throw new InvalidOperationException(ex.Message, ex);
                    }
                    else
                    {
                        throw new Exception($"Database operation failed while saving voucher: {ex.Message}", ex);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred while saving voucher: {ex.Message}", ex);
                }
            }
        }
        return outputVoucherId;
    }

    public List<Voucher> GetVouchers(int? voucherId = null, string? voucherType = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        List<Voucher> vouchers = new List<Voucher>();
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("sp_GetVouchers", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (voucherId.HasValue) cmd.Parameters.AddWithValue("@VoucherId", voucherId.Value);
                else cmd.Parameters.AddWithValue("@VoucherId", DBNull.Value);

                if (voucherType != null) cmd.Parameters.AddWithValue("@VoucherType", voucherType);
                else cmd.Parameters.AddWithValue("@VoucherType", DBNull.Value);

                if (startDate.HasValue) cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                else cmd.Parameters.AddWithValue("@StartDate", DBNull.Value);

                if (endDate.HasValue) cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
                else cmd.Parameters.AddWithValue("@EndDate", DBNull.Value);

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Dictionary<int, Voucher> voucherMap = new Dictionary<int, Voucher>();

                        while (reader.Read())
                        {
                            int currentVoucherId = reader.GetInt32("VoucherId");
                            if (!voucherMap.ContainsKey(currentVoucherId))
                            {
                                voucherMap[currentVoucherId] = new Voucher
                                {
                                    VoucherId = currentVoucherId,
                                    VoucherType = reader.GetString("VoucherType"),
                                    VoucherDate = reader.GetDateTime("VoucherDate"),
                                    ReferenceNo = reader.GetString("ReferenceNo"),
                                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                    CreatedBy = reader.GetString("CreatedBy"),
                                    CreatedDate = reader.GetDateTime("CreatedDate"),
                                    Details = new List<VoucherDetail>()
                                };
                                vouchers.Add(voucherMap[currentVoucherId]);
                            }

                            voucherMap[currentVoucherId].Details.Add(new VoucherDetail
                            {
                                VoucherDetailId = reader.GetInt32("VoucherDetailId"),
                                VoucherId = currentVoucherId,
                                AccountId = reader.GetInt32("AccountId"),
                                AccountCode = reader.GetString("AccountCode"),
                                AccountName = reader.GetString("AccountName"),
                                DebitAmount = reader.GetDecimal("DebitAmount"),
                                CreditAmount = reader.GetDecimal("CreditAmount"),
                                Narration = reader.IsDBNull("Narration") ? null : reader.GetString("Narration")
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Database operation failed while fetching vouchers: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred while fetching vouchers: {ex.Message}", ex);
                }
            }
        }
        return vouchers;
    }
}