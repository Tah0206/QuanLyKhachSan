using DoAnQuanLyKhachSan.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace DoAnQuanLyKhachSan.Controllers
{
    public class HomeController : Controller
    {
        private KhachSanEntities db = new KhachSanEntities();

        public ActionResult TrangChu()
        {
            return View();
        }

        public ActionResult Dining()
        {
            return View();
        }

        public ActionResult RoomsAndSuite()
        {
            return View();
        }

        public ActionResult TrangChuNguoiDung()
        {
            ViewBag.Title = "Trang chủ người dùng";
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.RoomId = new SelectList(db.Rooms.Where(r => r.RoomStatusId == 1), "RoomId", "RoomNumber");
            return View();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["UserId"] == null)  // Chưa đăng nhập
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Auth", action = "Login" }
                    ));
            }
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            // Tổng số phòng
            var tongPhong = db.Rooms.Count();

            // Phòng còn trống (giả định RoomStatusId = 1 là "Trống")
            var phongTrong = db.Rooms.Count(r => r.RoomStatusId == 1);

            // Khách đang thuê (giả định RoomStatusId = 2 là "Đang thuê")
            var khachDangThue = db.Rooms.Count(r => r.RoomStatusId == 2);

            // Doanh thu hôm nay
            var doanhThuHomNay = db.Payments
                .Where(p => DbFunctions.TruncateTime(p.PaymentDate) == DbFunctions.TruncateTime(DateTime.Now))
                .Sum(p => (decimal?)p.TotalAmount) ?? 0;

            // Top 5 khách hàng đặt phòng nhiều nhất
            var topKhach = db.Bookings
                .GroupBy(b => b.Customer.FullName)
                .Select(g => new
                {
                    Khach = g.Key,
                    SoLan = g.Count()
                })
                .OrderByDescending(x => x.SoLan)
                .Take(5)
                .ToList();

            // Truyền dữ liệu sang View
            ViewBag.TongPhong = tongPhong;
            ViewBag.PhongTrong = phongTrong;
            ViewBag.KhachDangThue = khachDangThue;
            ViewBag.DoanhThuHomNay = doanhThuHomNay;
            ViewBag.TopKhach = topKhach;

            return View();
        }
    }
}
