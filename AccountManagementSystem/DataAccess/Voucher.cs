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
                cmd.Parameters.AddWithValue("@CreatedDate", voucher.CreatedDate);

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
                        Console.WriteLine("Stored procedure did not return a VoucherId. This indicates a potential issue in the SP.");
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
                    throw new Exception($"An unexpected error occurred while saving voucher: {ex.Message}", ex);
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
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Dictionary<int, Voucher> voucherMap = new Dictionary<int, Voucher>();

                    while (reader.Read())
                    {
                        int currentVoucherId = reader.GetInt32(reader.GetOrdinal("VoucherId"));

                        if (!voucherMap.ContainsKey(currentVoucherId))
                        {
                            voucherMap[currentVoucherId] = new Voucher
                            {
                                VoucherId = currentVoucherId,
                                VoucherType = reader.GetString(reader.GetOrdinal("VoucherType")),
                                VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
                                ReferenceNo = reader.GetString(reader.GetOrdinal("ReferenceNo")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                Details = new List<VoucherDetail>()
                            };
                            vouchers.Add(voucherMap[currentVoucherId]);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("VoucherDetailId")))
                        {
                            voucherMap[currentVoucherId].Details.Add(new VoucherDetail
                            {
                                VoucherDetailId = reader.GetInt32(reader.GetOrdinal("VoucherDetailId")),
                                VoucherId = currentVoucherId,
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                                DebitAmount = reader.GetDecimal(reader.GetOrdinal("DebitAmount")),
                                CreditAmount = reader.GetDecimal(reader.GetOrdinal("CreditAmount")),
                                Narration = reader.IsDBNull(reader.GetOrdinal("Narration")) ? null : reader.GetString(reader.GetOrdinal("Narration"))
                            });
                        }
                    }
                }

            }
        }
        return vouchers;
    }
}