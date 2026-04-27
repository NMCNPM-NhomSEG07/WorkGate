using System;
using System.Linq;
using System.Web.Mvc;
using btlWorkGate_SanGiaoDichViecLam.Models;

namespace btlWorkGate_SanGiaoDichViecLam.Controllers
{
    public class HomeController : Controller
    {
        private readonly mTinTuyenDung _tinModel = new mTinTuyenDung();
        private readonly mUngTuyen _ungModel = new mUngTuyen();

        public ActionResult Index()
        {
            // Lấy mã doanh nghiệp
            string maDN = Session["MaDN"] as string;
            if (string.IsNullOrEmpty(maDN))
            {
                maDN = "DN001";
            }

            // Lấy danh sách tin
            var allTin = _tinModel.GetTinTuyenDungByMaDN(maDN);

            // CHỈ LẤY TIN CHƯA HẾT HẠN (dHanNop >= hôm nay)
            var tinChuaHetHan = allTin.Where(t => t.dHanNop.Date >= DateTime.Today).ToList();

            // Lấy tên doanh nghiệp
            string tenDN = "Doanh nghiệp";
            if (allTin.Any())
            {
                tenDN = GetTenDoanhNghiep(maDN);
            }

            // Tính thống kê
            int tinDangTuyen = tinChuaHetHan.Count;
            int tinMoiTrongTuan = tinChuaHetHan.Count(t => t.dNgayDang >= DateTime.Now.AddDays(-7));

            int ungVienMoi = 0;
            int daTuyen = 0;
            int hoSoHomNay = 0;

            foreach (var tin in allTin)
            {
                var ungTuyens = _ungModel.GetUngVienByMaTin(tin.PK_sMaTin);
                ungVienMoi += ungTuyens.Count(u => u.dNgayUngTuyen >= DateTime.Now.AddDays(-30));
                daTuyen += ungTuyens.Count(u => u.sTrangThaiUngTuyen == "Đã duyệt");
                hoSoHomNay += ungTuyens.Count(u => u.dNgayUngTuyen.Date == DateTime.Today);
            }

            Random rnd = new Random();
            int luotXem = allTin.Sum(t => rnd.Next(50, 500));
            int luotXemTang = rnd.Next(50, 200);

            // Gán dữ liệu
            ViewBag.TinTuyenDungs = tinChuaHetHan;  // CHỈ TIN CHƯA HẾT HẠN
            ViewBag.TenDoanhNghiep = tenDN;
            ViewBag.TinDangTuyen = tinDangTuyen;
            ViewBag.TinMoiTrongTuan = tinMoiTrongTuan;
            ViewBag.UngVienMoi = ungVienMoi;
            ViewBag.HoSoHomNay = hoSoHomNay;
            ViewBag.DaTuyen = daTuyen;
            ViewBag.LuotXem = luotXem;
            ViewBag.LuotXemTang = luotXemTang;

            return View();
        }

        public ActionResult Enterprise()
        {
            return RedirectToAction("Index");
        }

        private string GetTenDoanhNghiep(string maDN)
        {
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString))
                {
                    string sql = "SELECT sTenDN FROM tblDoanhNghiep WHERE PK_sMaDN = @MaDN";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaDN", maDN);
                        conn.Open();
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "Doanh nghiệp";
                    }
                }
            }
            catch
            {
                return "Doanh nghiệp";
            }
        }
    }
}