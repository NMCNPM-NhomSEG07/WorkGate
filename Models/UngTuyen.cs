using System;

namespace btlWorkGate_SanGiaoDichViecLam.Models
{
    public class UngTuyen
    {
        public string PK_sMaUngTuyen { get; set; } = string.Empty;
        public string FK_sMaTin { get; set; } = string.Empty;
        public string FK_sMaHoSo { get; set; } = string.Empty;
        public DateTime dNgayUngTuyen { get; set; }
        public string sTrangThaiUngTuyen { get; set; } = "Chờ duyệt";
        public string? sPhanHoi { get; set; }

        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SDT { get; set; } = string.Empty;
        public string NganhHoc { get; set; } = string.Empty;
        public string TenViTri { get; set; } = string.Empty;

        public string TrangThaiHienThi
        {
            get
            {
                if (sTrangThaiUngTuyen == "Đã nộp" || sTrangThaiUngTuyen == "Đã xem")
                    return "Chờ duyệt";
                if (sTrangThaiUngTuyen == "Mời phỏng vấn")
                    return "Mời phỏng vấn";
                if (sTrangThaiUngTuyen == "Trúng tuyển" || sTrangThaiUngTuyen == "Đã duyệt")
                    return "Đã duyệt";
                if (sTrangThaiUngTuyen == "Từ chối")
                    return "Từ chối";
                return "Chờ duyệt";
            }
        }
    }
}