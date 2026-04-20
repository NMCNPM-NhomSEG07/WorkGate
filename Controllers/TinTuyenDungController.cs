using System;
using System.Web.Mvc;
using btlWorkGate_SanGiaoDichViecLam.Models;

namespace btlWorkGate_SanGiaoDichViecLam.Controllers
{
    public class TinTuyenDungController : Controller
    {
        private readonly mTinTuyenDung _model = new mTinTuyenDung();

        // ==================== DANH SÁCH TIN TUYỂN DỤNG CỦA TÔI ====================
        // Thay vì Index(), đổi thành DanhSachTin()
        public ActionResult DanhSachTin()
        {
            string maDN = "DN0001";

            if (string.IsNullOrEmpty(maDN))
            {
                return RedirectToAction("Login", "Account");
            }

            var listTin = _model.GetTinTuyenDungByMaDN(maDN);
            return View(listTin);           // Sẽ tìm file DanhSachTin.cshtml
        }

        // ==================== MỞ FORM ĐĂNG TIN MỚI ====================
        public ActionResult DangTin()
        {
            // Trả về View rỗng để đăng tin mới
            return View(new TinTuyenDungViewModel());   // Truyền model rỗng
        }

        // ==================== XỬ LÝ ĐĂNG TIN (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult yeuCauDangTin(TinTuyenDungViewModel vm)
        {
            string maDN = "DN0001";   // Sau này lấy từ Session

            if (string.IsNullOrEmpty(maDN))
            {
                return Json(new { success = false, message = "MS_05", error = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại." });
            }

            // Xác thực nghiệp vụ
            string? errorCode = xacThucNghiepVu(vm);
            if (!string.IsNullOrEmpty(errorCode))
            {
                return Json(new { success = false, message = errorCode });
            }

            // Chuẩn bị dữ liệu
            TinTuyenDung tin = new TinTuyenDung
            {
                FK_sMaDN = maDN,
                sViTriCV = vm.sViTriCV,
                tMoTaCV = vm.tMoTaCV,
                sYeuCauChuyenMon = vm.sYeuCauChuyenMon,
                iSoLuong = vm.iSoLuong,
                fMucLuong = vm.fMucLuong,
                sDiaDiem = vm.sDiaDiem,
                dHanNop = vm.dHanNop,
                dNgayDang = DateTime.Now,
                sTrangThaiTin = "Chờ duyệt"
            };

            int result = _model.truyVanThemTinTuyenDung(tin);

            if (result > 0)
            {
                return Json(new { success = true, message = "MS_Success", maTin = tin.PK_sMaTin });
            }
            else
            {
                return Json(new { success = false, message = "MS_05", error = "Không thể lưu tin tuyển dụng vào cơ sở dữ liệu." });
            }
        }

        // ==================== XÁC THỰC NGHIỆP VỤ ====================
        private string xacThucNghiepVu(TinTuyenDungViewModel vm)
        {
            if (vm.dHanNop <= DateTime.Now)
                return "MS_03";   // Hạn nộp không được trong quá khứ

            if (vm.iSoLuong <= 0)
                return "MS_02";   // Số lượng phải > 0

            // Có thể thêm kiểm tra khác: vị trí không rỗng, mức lương > 0...

            return null;
        }
    }
}