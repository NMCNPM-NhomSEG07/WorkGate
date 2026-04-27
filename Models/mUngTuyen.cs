using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace btlWorkGate_SanGiaoDichViecLam.Models
{
    public class mUngTuyen
    {
        private readonly string _connectionString;

        public mUngTuyen()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        }

        public List<UngTuyen> GetUngVienByMaTin(string maTin)
        {
            List<UngTuyen> list = new List<UngTuyen>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
            SELECT 
                u.PK_sMaUngTuyen,
                u.FK_sMaTin,
                u.FK_sMaHoSo,
                u.dNgayUngTuyen,
                u.sTrangThaiUngTuyen,
                u.sPhanHoi,
                sv.sHoTen AS HoTen,
                usr.sEmail AS Email,
                sv.sSDT,
                sv.sNganhHoc,
                t.sViTriCV AS TenViTri
            FROM tblUngTuyen u
            INNER JOIN tblHoSoSinhVien hs ON u.FK_sMaHoSo = hs.PK_sMaHoSo
            INNER JOIN tblSinhVien sv ON hs.FK_sMaSV = sv.PK_sMaSV
            INNER JOIN tblUser usr ON sv.FK_sUserID = usr.PK_sUserID
            INNER JOIN tblTinTuyenDung t ON u.FK_sMaTin = t.PK_sMaTin
            WHERE u.FK_sMaTin = @MaTin
            ORDER BY u.dNgayUngTuyen DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaTin", maTin);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new UngTuyen
                            {
                                PK_sMaUngTuyen = reader["PK_sMaUngTuyen"].ToString(),
                                FK_sMaTin = reader["FK_sMaTin"].ToString(),
                                FK_sMaHoSo = reader["FK_sMaHoSo"].ToString(),
                                dNgayUngTuyen = Convert.ToDateTime(reader["dNgayUngTuyen"]),
                                sTrangThaiUngTuyen = reader["sTrangThaiUngTuyen"].ToString(),
                                sPhanHoi = reader["sPhanHoi"]?.ToString(),
                                HoTen = reader["HoTen"].ToString(),
                                Email = reader["Email"].ToString(),
                                SDT = reader["sSDT"]?.ToString() ?? "",
                                NganhHoc = reader["sNganhHoc"]?.ToString() ?? "",
                                TenViTri = reader["TenViTri"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public int UpdateTrangThaiUngTuyen(string maUngTuyen, string trangThaiMoi, string phanHoi = null)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();
                string sql = @"
            UPDATE tblUngTuyen 
            SET sTrangThaiUngTuyen = @TrangThai,
                sPhanHoi = @PhanHoi,
                dNgayCapNhat = GETDATE()
            WHERE PK_sMaUngTuyen = @MaUngTuyen";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                cmd.Parameters.AddWithValue("@TrangThai", trangThaiMoi);
                cmd.Parameters.AddWithValue("@PhanHoi", string.IsNullOrEmpty(phanHoi) ? (object)DBNull.Value : phanHoi);
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0) System.Diagnostics.Debug.WriteLine($"Không tìm thấy mã ứng tuyển: {maUngTuyen}");
                return rows;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi UpdateTrangThaiUngTuyen: {ex.Message}");
                return 0;
            }
        }

        // Thêm vào class mUngTuyen

        public UngTuyen GetUngTuyenById(string maUngTuyen)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT * FROM tblUngTuyen WHERE PK_sMaUngTuyen = @MaUngTuyen";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UngTuyen
                            {
                                PK_sMaUngTuyen = reader["PK_sMaUngTuyen"].ToString(),
                                FK_sMaTin = reader["FK_sMaTin"].ToString(),
                                FK_sMaHoSo = reader["FK_sMaHoSo"].ToString(),
                                dNgayUngTuyen = Convert.ToDateTime(reader["dNgayUngTuyen"]),
                                sTrangThaiUngTuyen = reader["sTrangThaiUngTuyen"].ToString(),
                                sPhanHoi = reader["sPhanHoi"]?.ToString()
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public bool CheckQuyenUngVien(string maUngTuyen, string maDN)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
            SELECT COUNT(*)
            FROM tblUngTuyen u
            INNER JOIN tblTinTuyenDung t ON u.FK_sMaTin = t.PK_sMaTin
            WHERE u.PK_sMaUngTuyen = @MaUngTuyen AND t.FK_sMaDN = @MaDN";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                    cmd.Parameters.AddWithValue("@MaDN", maDN);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public int XoaUngVienTheoMaTin(string maTin)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM tblUngTuyen WHERE FK_sMaTin = @MaTin";
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