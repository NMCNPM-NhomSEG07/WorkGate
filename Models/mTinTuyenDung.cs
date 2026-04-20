using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace btlWorkGate_SanGiaoDichViecLam.Models
{
    public class mTinTuyenDung
    {
        private readonly string _connectionString;

        public mTinTuyenDung()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        }

        // Hàm 4: truyVanThemTinTuyenDung
        public int truyVanThemTinTuyenDung(TinTuyenDung tin)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            // Tự động sinh mã tin: TIN0001, TIN0002, ...
            string sqlGetMax = @"SELECT ISNULL(MAX(CAST(SUBSTRING(PK_sMaTin, 4, LEN(PK_sMaTin)-3) AS INT)), 0) + 1 
                                 FROM tblTinTuyenDung";

            using SqlCommand cmdMax = new SqlCommand(sqlGetMax, conn);
            int nextId = Convert.ToInt32(cmdMax.ExecuteScalar());
            tin.PK_sMaTin = $"TIN{nextId:D4}";

            string sqlInsert = @"
                INSERT INTO tblTinTuyenDung 
                (PK_sMaTin, FK_sMaDN, sViTriCV, tMoTaCV, sYeuCauChuyenMon, iSoLuong, 
                 fMucLuong, sDiaDiem, dHanNop, dNgayDang, sTrangThaiTin)
                VALUES 
                (@MaTin, @MaDN, @ViTri, @MoTa, @YeuCau, @SoLuong, 
                 @MucLuong, @DiaDiem, @HanNop, @NgayDang, @TrangThai)";

            using SqlCommand cmd = new SqlCommand(sqlInsert, conn);

            cmd.Parameters.AddWithValue("@MaTin", tin.PK_sMaTin);
            cmd.Parameters.AddWithValue("@MaDN", tin.FK_sMaDN);
            cmd.Parameters.AddWithValue("@ViTri", tin.sViTriCV);
            cmd.Parameters.AddWithValue("@MoTa", tin.tMoTaCV);
            cmd.Parameters.AddWithValue("@YeuCau", (object)tin.sYeuCauChuyenMon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SoLuong", tin.iSoLuong);
            cmd.Parameters.AddWithValue("@MucLuong", (object)tin.fMucLuong ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DiaDiem", tin.sDiaDiem);
            cmd.Parameters.AddWithValue("@HanNop", tin.dHanNop);
            cmd.Parameters.AddWithValue("@NgayDang", tin.dNgayDang);
            cmd.Parameters.AddWithValue("@TrangThai", tin.sTrangThaiTin);

            return cmd.ExecuteNonQuery();   // Trả về 1 nếu thành công
        }

        public List<TinTuyenDung> GetTinTuyenDungByMaDN(string maDN)
        {
            List<TinTuyenDung> list = new List<TinTuyenDung>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT * FROM tblTinTuyenDung 
                       WHERE FK_sMaDN = @MaDN 
                       ORDER BY dNgayDang DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaDN", maDN);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TinTuyenDung tin = new TinTuyenDung
                            {
                                PK_sMaTin = reader["PK_sMaTin"].ToString(),
                                sViTriCV = reader["sViTriCV"].ToString(),
                                sDiaDiem = reader["sDiaDiem"].ToString(),
                                fMucLuong = reader["fMucLuong"] as double?,
                                dHanNop = Convert.ToDateTime(reader["dHanNop"]),
                                iSoLuong = Convert.ToInt32(reader["iSoLuong"]),
                                sTrangThaiTin = reader["sTrangThaiTin"].ToString(),
                                // thêm các trường khác nếu cần
                            };
                            list.Add(tin);
                        }
                    }
                }
            }
            return list;
        }
    }
}