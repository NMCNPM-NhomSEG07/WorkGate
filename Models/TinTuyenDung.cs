using System;

namespace btlWorkGate_SanGiaoDichViecLam.Models
{
    public class TinTuyenDung
    {
        public string PK_sMaTin { get; set; } = string.Empty;
        public string FK_sMaDN { get; set; } = string.Empty;
        public string sViTriCV { get; set; } = string.Empty;
        public string tMoTaCV { get; set; } = string.Empty;
        public string? sYeuCauChuyenMon { get; set; }
        public int iSoLuong { get; set; } = 1;
        public double? fMucLuong { get; set; }
        public string sDiaDiem { get; set; } = string.Empty;
        public DateTime dHanNop { get; set; }
        public DateTime dNgayDang { get; set; }
        public string sTrangThaiTin { get; set; } = "Chờ duyệt";
        public string? sGhiChuTuChoi { get; set; }
    }
}