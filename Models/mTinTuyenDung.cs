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

            tin.dNgayDang = DateTime.Now;
            tin.sTrangThaiTin = "Chờ duyệt";

            return truyVanThemTinTuyenDung(tin);
        }

        // ==================== TRUY VẤN THÊM TIN ====================
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
                return 0;
            }
        }

        // ==================== LẤY DANH SÁCH TIN THEO MÃ DN + SỐ LƯỢNG ỨNG VIÊN ====================
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
                                iSoLuong = Convert.ToInt32(reader["iSoLuong"]),        // Giữ nguyên số lượng tuyển
                                fMucLuong = reader["fMucLuong"] as double?,
                                sDiaDiem = reader["sDiaDiem"].ToString(),
                                dHanNop = Convert.ToDateTime(reader["dHanNop"]),
                                dNgayDang = Convert.ToDateTime(reader["dNgayDang"]),
                                sTrangThaiTin = reader["sTrangThaiTin"].ToString()
                            };

                            // Lấy số lượng ứng viên thực tế
                            tin.iSoUngVien = GetSoLuongUngVien(tin.PK_sMaTin);

                            list.Add(tin);
                        }
                    }
                }
            }
            return list;
        }

        // ==================== CHỈNH SỬA TIN ====================
        public int capNhatTin(string maTin, TinTuyenDungViewModel vm, string maDN)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string sql = @"
            UPDATE tblTinTuyenDung 
            SET sViTriCV = @ViTri, 
                sMoTaCV = @MoTa, 
                sYeuCauChuyenMon = @YeuCau,
                iSoLuong = @SoLuong, 
                fMucLuong = @MucLuong, 
                sDiaDiem = @DiaDiem,
                dHanNop = @HanNop, 
                sTrangThaiTin = N'Chờ duyệt'
            WHERE PK_sMaTin = @MaTin AND FK_sMaDN = @MaDN";

                using SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@MaTin", maTin);
                cmd.Parameters.AddWithValue("@MaDN", maDN);
                cmd.Parameters.AddWithValue("@ViTri", vm.sViTriCV);
                cmd.Parameters.AddWithValue("@MoTa", vm.sMoTaCV);
                cmd.Parameters.AddWithValue("@YeuCau", (object)vm.sYeuCauChuyenMon ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SoLuong", vm.iSoLuong);
                cmd.Parameters.AddWithValue("@MucLuong", (object)vm.fMucLuong ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DiaDiem", vm.sDiaDiem);
                cmd.Parameters.AddWithValue("@HanNop", vm.dHanNop);

                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Để debug: bạn có thể ghi log ex.Message
                Console.WriteLine("Lỗi cập nhật tin: " + ex.Message);
                return 0;
            }
        }

        // ==================== ĐỔI TRẠNG THÁI TIN ====================
        public int doiTrangThaiTin(string maTin, string trangThaiMoi)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                string sql = @"
                    UPDATE tblTinTuyenDung 
                    SET sTrangThaiTin = @TrangThaiMoi,
                        dNgayCapNhat = GETDATE()
                    WHERE PK_sMaTin = @MaTin";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaTin", maTin);
                cmd.Parameters.AddWithValue("@TrangThaiMoi", trangThaiMoi);

                return cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // ==================== XÁC THỰC QUYỀN QUẢN LÝ TIN ====================
        public bool xacThucQuyenQuanLy(string maTin, string maDN)
        {
            if (string.IsNullOrEmpty(maTin) || string.IsNullOrEmpty(maDN))
                return false;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string sql = @"SELECT COUNT(*) 
                           FROM tblTinTuyenDung 
                           WHERE PK_sMaTin = @MaTin 
                             AND FK_sMaDN = @MaDN";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaTin", maTin);
                        cmd.Parameters.AddWithValue("@MaDN", maDN);

                        conn.Open();
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ==================== LẤY SỐ LƯỢNG ỨNG VIÊN THỰC TẾ ====================
        public int GetSoLuongUngVien(string maTin)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM tblUngTuyen WHERE FK_sMaTin = @MaTin";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaTin", maTin);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int XoaTinTuyenDung(string maTin)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM tblTinTuyenDung WHERE PK_sMaTin = @MaTin";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaTin", maTin);
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}