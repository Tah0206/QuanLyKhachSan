using DoAnQuanLyKhachSan.Models;
using DoAnQuanLyKhachSan.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DoAnQuanLyKhachSan.Controllers
{
    public class TongQuanController : Controller
    {
        private KhachSanEntities db = new KhachSanEntities();

        public ActionResult Index()
        {
            var tangList = db.Floors.OrderBy(t => t.FloorName).ToList();

            var result = tangList.Select(t => new RoomOverviewViewModel
            {
                TenTang = t.FloorName,
                Phong = t.Rooms.Select(p => new RoomOverviewViewModel.RoomInfo
                {
                    RoomId = p.RoomId,
                    SoPhong = p.RoomNumber,
                    TrangThai = p.RoomStatu.StatusName,
                    IsBooked = p.RoomStatu.StatusName == "Đang sử dụng",

                    //  Lấy BookingId nếu phòng đang được sử dụng
                    BookingId = db.Bookings
                        .FirstOrDefault(b => b.RoomId == p.RoomId && p.RoomStatu.StatusName == "Đang sử dụng")
                        ?.BookingId
                }).ToList()
            }).ToList();

            return View(result);
        }
        public ActionResult LoadPaymentForm(int bookingId)
        {
            var booking = db.Bookings
                            .Include("Customer") // nếu tên navigation là "Customer"
                            .Include("Room")
                            .FirstOrDefault(b => b.BookingId == bookingId);

            if (booking == null)
                return HttpNotFound();

            return PartialView("~/Views/Payment/_PaymentForm.cshtml", booking);
        }

    }
}
