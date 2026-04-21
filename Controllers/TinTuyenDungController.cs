using System;
using System.Linq;
using System.Web.Mvc;
using btlWorkGate_SanGiaoDichViecLam.Models;

namespace btlWorkGate_SanGiaoDichViecLam.Controllers
{
    public class TinTuyenDungController : Controller
    {
        private readonly mTinTuyenDung _model = new mTinTuyenDung();

        // ==================== DANH SÁCH TIN TUYỂN DỤNG CỦA TÔI ====================
        public ActionResult vDanhSachTin()
        {
            //string maDN = Session["MaDN"]?.ToString();
            //if (string.IsNullOrEmpty(maDN))
            //{
            //    return Json(new { success = false, code = "MS_05", message = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại." });
            //}
            string maDN = Session["MaDN"] as string;
            if (string.IsNullOrEmpty(maDN))
            {
                // Tạm thời dùng cứng để test, sau này lấy từ session
                maDN = "DN001";
                // return RedirectToAction("Login", "Account");
            }

            var listTin = _model.GetTinTuyenDungByMaDN(maDN);
            return View(listTin);
        }

        // ==================== MỞ FORM ĐĂNG TIN MỚI ====================
        public ActionResult vDangTin()
        {
            return View(new TinTuyenDungViewModel());
        }

        // ==================== XỬ LÝ ĐĂNG TIN (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult yeuCauDangTin(TinTuyenDungViewModel vm)
        {
            // Kiểm tra ModelState từ DataAnnotation (các [Required], [Range])
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                string message = string.Join("; ", errors);
                return Json(new { success = false, code = "MS_01", message });
            }

            // Lấy mã doanh nghiệp từ session (sau này thay bằng session thật)
            string maDN = Session["MaDN"] as string;
            if (string.IsNullOrEmpty(maDN))
            {
                // Tạm gán cứng để test
                maDN = "DN001";
                // return Json(new { success = false, code = "MS_05", message = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại." });
            }

            // Xác thực nghiệp vụ (business validation)
            string? errorCode = xacThucNghiepVu(vm);
            if (errorCode != null)
            {
                string msg = errorCode switch
                {
                    "MS_01" => "Vui lòng điền đầy đủ các trường bắt buộc (vị trí, mô tả, địa điểm, yêu cầu).",
                    "MS_02" => "Số lượng tuyển phải lớn hơn 0.",
                    "MS_03" => "Hạn nộp hồ sơ phải sau ngày hiện tại.",
                    _ => "Dữ liệu không hợp lệ."
                };
                return Json(new { success = false, code = errorCode, message = msg });
            }

            // Tạo đối tượng TinTuyenDung (chưa có mã tin, ngày đăng, trạng thái)
            TinTuyenDung tin = new TinTuyenDung
            {
                FK_sMaDN = maDN,
                sViTriCV = vm.sViTriCV,
                tMoTaCV = vm.tMoTaCV,
                sYeuCauChuyenMon = vm.sYeuCauChuyenMon,
                iSoLuong = vm.iSoLuong,
                fMucLuong = vm.fMucLuong,
                sDiaDiem = vm.sDiaDiem,
                dHanNop = vm.dHanNop
                
            };

            // Gọi model lưu tin mới
            int result = _model.luuTinMoi(tin);

            if (result > 0)
            {
                return Json(new
                {
                    success = true,
                    code = "MS_Success",
                    message = "Đăng tin tuyển dụng thành công!",
                    maTin = tin.PK_sMaTin
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    code = "MS_05",
                    message = "Lỗi hệ thống, không thể lưu tin. Vui lòng thử lại sau."
                });
            }
        }

        // ==================== XÁC THỰC NGHIỆP VỤ ====================
        private string? xacThucNghiepVu(TinTuyenDungViewModel vm)
        {
            // MS_01: Kiểm tra các trường bắt buộc 
            if (string.IsNullOrWhiteSpace(vm.sViTriCV) ||
                string.IsNullOrWhiteSpace(vm.tMoTaCV) ||
                string.IsNullOrWhiteSpace(vm.sDiaDiem) ||
                string.IsNullOrWhiteSpace(vm.sYeuCauChuyenMon))
            {
                return "MS_01";
            }

            // MS_02: Số lượng tuyển phải > 0
            if (vm.iSoLuong <= 0)
                return "MS_02";

            // MS_03: Hạn nộp phải lớn hơn ngày hiện tại (không được bằng)
            if (vm.dHanNop.Date <= DateTime.Today)
                return "MS_03";

            return null;
        }
    }
}