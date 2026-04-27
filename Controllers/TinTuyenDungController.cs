using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            string maDN = Session["MaDN"] as string;
            if (string.IsNullOrEmpty(maDN))
            {
                maDN = "DN001"; // tạm thời để test
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                string message = string.Join("; ", errors);
                return Json(new { success = false, code = "MS_01", message });
            }

            string maDN = Session["MaDN"] as string ?? "DN001";

            string? errorCode = xacThucNghiepVu(vm);
            if (errorCode != null)
            {
                string msg = errorCode switch
                {
                    "MS_01" => "Vui lòng điền đầy đủ các trường bắt buộc.",
                    "MS_02" => "Số lượng tuyển phải lớn hơn 0.",
                    "MS_03" => "Hạn nộp hồ sơ phải sau ngày hiện tại.",
                    _ => "Dữ liệu không hợp lệ."
                };
                return Json(new { success = false, code = errorCode, message = msg });
            }

            TinTuyenDung tin = new TinTuyenDung
            {
                FK_sMaDN = maDN,
                sViTriCV = vm.sViTriCV,
                sMoTaCV = vm.sMoTaCV,
                sYeuCauChuyenMon = vm.sYeuCauChuyenMon,
                iSoLuong = vm.iSoLuong,
                fMucLuong = vm.fMucLuong,
                sDiaDiem = vm.sDiaDiem,
                dHanNop = vm.dHanNop
            };

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
                return Json(new { success = false, code = "MS_05", message = "Lỗi hệ thống, không thể lưu tin." });
            }
        }

        // ==================== XÁC THỰC NGHIỆP VỤ ====================
        private string? xacThucNghiepVu(TinTuyenDungViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.sViTriCV) ||
                string.IsNullOrWhiteSpace(vm.sMoTaCV) ||
                string.IsNullOrWhiteSpace(vm.sDiaDiem) ||
                string.IsNullOrWhiteSpace(vm.sYeuCauChuyenMon))
            {
                return "MS_01";
            }

            if (vm.iSoLuong <= 0)
                return "MS_02";

            if (vm.dHanNop.Date <= DateTime.Today)
                return "MS_03";

            return null;
        }

        // ==================== MỞ FORM CHỈNH SỬA TIN ====================
        public ActionResult vChinhSuaTin(string maTin)
        {
            string maDN = Session["MaDN"] as string ?? "DN001";

            var list = _model.GetTinTuyenDungByMaDN(maDN);
            var tin = list.FirstOrDefault(t => t.PK_sMaTin == maTin);

            if (tin == null)
            {
                return HttpNotFound("Không tìm thấy tin tuyển dụng");
            }

            var vm = new TinTuyenDungViewModel
            {
                sViTriCV = tin.sViTriCV,
                sMoTaCV = tin.sMoTaCV,
                sYeuCauChuyenMon = tin.sYeuCauChuyenMon,
                iSoLuong = tin.iSoLuong,
                fMucLuong = tin.fMucLuong,
                sDiaDiem = tin.sDiaDiem,
                dHanNop = tin.dHanNop.Date   // Chỉ lấy ngày, bỏ giờ
            };

            ViewBag.MaTin = maTin;        // Truyền mã tin để POST
            return View(vm);
        }

        // ==================== XỬ LÝ CHỈNH SỬA TIN (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult yeuCauSuaTin(string maTin, TinTuyenDungViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, code = "MS_01", message = string.Join("; ", errors) });
            }

            string maDN = Session["MaDN"] as string ?? "DN001";

            // Kiểm tra quyền sở hữu
            if (!_model.xacThucQuyenQuanLy(maTin, maDN))
            {
                return Json(new { success = false, code = "MS_Denied", message = "Bạn không có quyền chỉnh sửa tin này." });
            }

            // Xác thực nghiệp vụ
            string? errorCode = xacThucNghiepVu(vm);
            if (errorCode != null)
            {
                string msg = errorCode switch
                {
                    "MS_03" => "Hạn nộp hồ sơ phải sau ngày hiện tại.",
                    _ => "Dữ liệu không hợp lệ."
                };
                return Json(new { success = false, code = errorCode, message = msg });
            }

            int result = _model.capNhatTin(maTin, vm, maDN);

            if (result > 0)
            {
                return Json(new
                {
                    success = true,
                    code = "MS_Success",
                    message = "Cập nhật tin tuyển dụng thành công! Tin đã được đưa về trạng thái Chờ duyệt."
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    code = "MS_05",
                    message = "Không tìm thấy tin hoặc không có dữ liệu thay đổi."
                });
            }
        }

        // ==================== QUẢN LÝ TRẠNG THÁI TIN (F10) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult xuLyThayDoiTrangThai(string maTin, string trangThaiMoi)
        {
            string maDN = Session["MaDN"] as string ?? "DN001";

            if (!_model.xacThucQuyenQuanLy(maTin, maDN))
            {
                return Json(new { success = false, code = "MS_Denied", message = "Bạn không có quyền thực hiện thao tác này." });
            }

            int result = _model.doiTrangThaiTin(maTin, trangThaiMoi);

            if (result > 0)
            {
                return Json(new
                {
                    success = true,
                    code = "MS_Success",
                    message = "Cập nhật trạng thái tin tuyển dụng thành công!"
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    code = "MS_05",
                    message = "Lỗi hệ thống. Không thể cập nhật trạng thái tin."
                });
            }
        }

        // ==================== XÁC THỰC QUYỀN ====================
        private bool xacThucQuyenQuanLy(string maTin, string maDN)
        {
            var listTin = _model.GetTinTuyenDungByMaDN(maDN);
            return listTin.Any(t => t.PK_sMaTin == maTin);
        }

        // GET: Danh sách ứng viên theo tin tuyển dụng
        public ActionResult vDanhSachUngVien(string maTin)
        {
            string maDN = Session["MaDN"] as string ?? "DN001";

            // Kiểm tra quyền sở hữu tin (dùng method có sẵn)
            if (!_model.xacThucQuyenQuanLy(maTin, maDN))
                return HttpNotFound("Bạn không có quyền xem ứng viên của tin này.");

            // Lấy danh sách tất cả tin của doanh nghiệp, sau đó tìm tin cần
            var allTin = _model.GetTinTuyenDungByMaDN(maDN);
            var tinInfo = allTin.FirstOrDefault(t => t.PK_sMaTin == maTin);
            if (tinInfo == null)
                return HttpNotFound("Không tìm thấy tin tuyển dụng.");

            ViewBag.MaTin = maTin;
            ViewBag.TenViTri = tinInfo.sViTriCV;   // Luôn có tên vị trí, kể cả khi không có ứng viên

            var mUng = new mUngTuyen();
            var list = mUng.GetUngVienByMaTin(maTin);

            return View(list);
        }

        // POST: Xét duyệt hồ sơ ứng viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult XuLyXetDuyet(string maUngTuyen, string trangThai, string phanHoi = "")
        {
            try
            {
                string maDN = Session["MaDN"] as string;
                if (string.IsNullOrEmpty(maDN))
                    return Json(new { success = false, message = "Bạn chưa đăng nhập doanh nghiệp." });

                var mUng = new mUngTuyen();

                // 1. Kiểm tra hồ sơ tồn tại
                var ungTuyen = mUng.GetUngTuyenById(maUngTuyen);
                if (ungTuyen == null)
                    return Json(new { success = false, message = "Hồ sơ không tồn tại." });

                // 2. Kiểm tra quyền sở hữu
                if (!mUng.CheckQuyenUngVien(maUngTuyen, maDN))
                    return Json(new { success = false, message = "Bạn không có quyền xử lý hồ sơ này." });

                // 3. Kiểm tra trạng thái hiện tại (chỉ cho phép xử lý nếu đang là "Chờ duyệt")
                string currentStatus = ungTuyen.sTrangThaiUngTuyen;
                bool isPending = (currentStatus == "Đã nộp" || currentStatus == "Đã xem");
                if (!isPending)
                    return Json(new { success = false, message = "Hồ sơ đã được xử lý trước đó." });

                // 4. Xác định trạng thái mới và câu thông báo
                string newStatus = trangThai switch
                {
                    "Đã duyệt" => "Đã duyệt",
                    "Từ chối" => "Từ chối",
                    "Mời phỏng vấn" => "Mời phỏng vấn",
                    _ => null
                };
                if (newStatus == null)
                    return Json(new { success = false, message = "Trạng thái không hợp lệ." });

                // 5. Gọi model cập nhật
                int result = mUng.UpdateTrangThaiUngTuyen(maUngTuyen, newStatus, phanHoi);

                if (result > 0)
                {
                    string msg = trangThai switch
                    {
                        "Đã duyệt" => "Duyệt hồ sơ thành công!",
                        "Từ chối" => "Đã từ chối hồ sơ.",
                        "Mời phỏng vấn" => "Đã gửi lời mời phỏng vấn.",
                        _ => "Cập nhật thành công."
                    };
                    return Json(new { success = true, message = msg });
                }
                else
                {
                    return Json(new { success = false, message = "Lỗi cập nhật cơ sở dữ liệu." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi XuLyXetDuyet: " + ex.Message);
                return Json(new { success = false, message = "Lỗi kết nối cơ sở dữ liệu." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult xoaTinVinhVien(string maTin)
        {
            try
            {
                string maDN = Session["MaDN"] as string ?? "DN001";
                if (string.IsNullOrEmpty(maDN))
                    return Json(new { success = false, message = "Bạn chưa đăng nhập." });

                // Kiểm tra quyền sở hữu tin
                if (!_model.xacThucQuyenQuanLy(maTin, maDN))
                    return Json(new { success = false, message = "Bạn không có quyền xóa tin này." });

                // Xóa ứng viên liên quan trước
                var mUng = new mUngTuyen();
                int deletedUngVien = mUng.XoaUngVienTheoMaTin(maTin);
                System.Diagnostics.Debug.WriteLine($"Đã xóa {deletedUngVien} ứng viên của tin {maTin}");

                // Xóa tin tuyển dụng
                int result = _model.XoaTinTuyenDung(maTin);
                if (result > 0)
                    return Json(new { success = true, message = "Đã xóa tin tuyển dụng thành công!" });
                else
                    return Json(new { success = false, message = "Xóa tin thất bại, không tìm thấy tin." });
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine("SQL Error: " + sqlEx.Message);
                return Json(new { success = false, message = "Lỗi cơ sở dữ liệu: " + sqlEx.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("General Error: " + ex.Message);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Danh sách tất cả ứng viên của doanh nghiệp
        public ActionResult vTatCaUngVien()
        {
            string maDN = Session["MaDN"] as string ?? "DN001";

            // Lấy tất cả tin của doanh nghiệp
            var allTin = _model.GetTinTuyenDungByMaDN(maDN);
            var allMaTin = allTin.Select(t => t.PK_sMaTin).ToList();

            var mUng = new mUngTuyen();
            var allUngVien = new List<UngTuyen>();

            foreach (var maTin in allMaTin)
            {
                var ungVien = mUng.GetUngVienByMaTin(maTin);
                allUngVien.AddRange(ungVien);
            }

            ViewBag.TenViTri = "Tất cả vị trí";
            return View("vDanhSachUngVien", allUngVien);
        }
    }
}