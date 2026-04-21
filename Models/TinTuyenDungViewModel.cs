using System;
using System.ComponentModel.DataAnnotations;

namespace btlWorkGate_SanGiaoDichViecLam.Models
{
    public class TinTuyenDungViewModel
    {
        [Required(ErrorMessage = "Vị trí công việc không được để trống")]
        [Display(Name = "Vị trí công việc")]
        public string sViTriCV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả công việc không được để trống")]
        [Display(Name = "Mô tả công việc")]
        public string sMoTaCV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yêu cầu ứng viên không được để trống")]
        [Display(Name = "Yêu cầu chuyên môn")]
        public string? sYeuCauChuyenMon { get; set; }

        [Required(ErrorMessage = "Số lượng tuyển không được để trống")]
        [Range(1, 1000, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng tuyển")]
        public int iSoLuong { get; set; }

        [Display(Name = "Mức lương")]
        public double? fMucLuong { get; set; }

        [Required(ErrorMessage = "Địa điểm làm việc không được để trống")]
        [Display(Name = "Địa điểm")]
        public string sDiaDiem { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hạn nộp hồ sơ không được để trống")]
        [Display(Name = "Hạn nộp")]
        [DataType(DataType.Date)]
        public DateTime dHanNop { get; set; }
    }
}