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

        // ==================== LƯU TIN MỚI ====================
        public int luuTinMoi(TinTuyenDung tin)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            // Tự động sinh mã tin mới: TIN0001, TIN0002, ...
            string sqlGetMax = @"SELECT ISNULL(MAX(CAST(SUBSTRING(PK_sMaTin, 4, LEN(PK_sMaTin)-3) AS INT)), 0) + 1 
                                 FROM tblTinTuyenDung";
            using SqlCommand cmdMax = new SqlCommand(sqlGetMax, conn);
            int nextId = Convert.ToInt32(cmdMax.ExecuteScalar());
            tin.PK_sMaTin = $"TIN{nextId:D4}";

            //  Thiết lập ngày đăng và trạng thái theo đúng thiết kế
            tin.dNgayDang = DateTime.Now;
            tin.sTrangThaiTin = "Chờ duyệt";

            // Gọi hàm truy vấn thêm
            return truyVanThemTinTuyenDung(tin);
        }

        // ====================  TRUY VẤN THÊM TIN ====================
        public int truyVanThemTinTuyenDung(TinTuyenDung tin)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string sqlInsert = @"
                    INSERT INTO tblTinTuyenDung 
                    (PK_sMaTin, FK_sMaDN, sViTriCV, sMoTaCV, sYeuCauChuyenMon, iSoLuong, 
                     fMucLuong, sDiaDiem, dHanNop, dNgayDang, sTrangThaiTin)
                    VALUES 
                    (@MaTin, @MaDN, @ViTri, @MoTa, @YeuCau, @SoLuong, 
                     @MucLuong, @DiaDiem, @HanNop, @NgayDang, @TrangThai)";

                using SqlCommand cmd = new SqlCommand(sqlInsert, conn);

                cmd.Parameters.AddWithValue("@MaTin", tin.PK_sMaTin);
                cmd.Parameters.AddWithValue("@MaDN", tin.FK_sMaDN);
                cmd.Parameters.AddWithValue("@ViTri", tin.sViTriCV);
                cmd.Parameters.AddWithValue("@MoTa", tin.sMoTaCV);
                cmd.Parameters.AddWithValue("@YeuCau", (object)tin.sYeuCauChuyenMon ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SoLuong", tin.iSoLuong);
                cmd.Parameters.AddWithValue("@MucLuong", (object)tin.fMucLuong ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DiaDiem", tin.sDiaDiem);
                cmd.Parameters.AddWithValue("@HanNop", tin.dHanNop);
                cmd.Parameters.AddWithValue("@NgayDang", tin.dNgayDang);
                cmd.Parameters.AddWithValue("@TrangThai", tin.sTrangThaiTin);

                return cmd.ExecuteNonQuery();   
            }
            catch (Exception)
            {
                // Ghi log lỗi nếu cần
                return 0;   // Trả về 0 để controller báo MS_05
            }
        }

        // ==================== LẤY DANH SÁCH TIN THEO MÃ DN ====================
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
                                FK_sMaDN = reader["FK_sMaDN"].ToString(),
                                sViTriCV = reader["sViTriCV"].ToString(),
                                sMoTaCV = reader["sMoTaCV"].ToString(),
                                sYeuCauChuyenMon = reader["sYeuCauChuyenMon"]?.ToString(),
                                iSoLuong = Convert.ToInt32(reader["iSoLuong"]),
                                fMucLuong = reader["fMucLuong"] as double?,
                                sDiaDiem = reader["sDiaDiem"].ToString(),
                                dHanNop = Convert.ToDateTime(reader["dHanNop"]),
                                dNgayDang = Convert.ToDateTime(reader["dNgayDang"]),
                                sTrangThaiTin = reader["sTrangThaiTin"].ToString()
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