using DoAnQuanLyKhachSan.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace QLKhachSan.Controllers
{
    public class BookingsController : Controller
    {
        private KhachSanEntities db = new KhachSanEntities();

        // GET: Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .Include(b => b.BookingStatu)
                .Include(b => b.PaymentStatu);
            return View(bookings.ToList());
        }

        // GET: Bookings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Booking booking = db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .Include(b => b.BookingStatu)
                .Include(b => b.PaymentStatu)
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null) return HttpNotFound();
            return View(booking);
        }

        // GET: Bookings/Create
        public ActionResult Create()
        {
            bool isCustomer = (Session["RoleName"] != null && Session["RoleName"].ToString() == "Khách hàng");
            ViewBag.RoomId = new SelectList(db.Rooms.Where(r => r.RoomStatusId == 1), "RoomId", "RoomNumber");

            if (!isCustomer)
            {
                ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "FullName");
                ViewBag.BookingStatusID = new SelectList(db.BookingStatus1, "BookingStatusID", "StatusName");
                ViewBag.PaymentStatusID = new SelectList(db.PaymentStatus1, "PaymentStatusID", "StatusName");
            }
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookingId,CustomerId,RoomId,CheckInDate,CheckOutDate,NumberOfPeople,Note,PaymentStatusID,BookingStatusID")] Booking booking)
        {
            bool isCustomer = (Session["RoleName"] != null && Session["RoleName"].ToString() == "Khách hàng");

            if (isCustomer)
            {
                int? userId = Session["UserId"] as int?;
                if (!userId.HasValue)
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để đặt phòng.";
                    return RedirectToAction("Login", "Auth");
                }

                var customer = db.Customers.FirstOrDefault(c => c.UserId == userId.Value);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng cập nhật thông tin khách hàng.";
                    return RedirectToAction("Create", "Customers");
                }

                booking.CustomerId = customer.CustomerId;
                booking.BookingStatusID = db.BookingStatus1.FirstOrDefault(s => s.StatusName == "Chờ nhận phòng")?.BookingStatusID;
                booking.PaymentStatusID = db.PaymentStatus1.FirstOrDefault(s => s.StatusName == "Chưa thanh toán")?.PaymentStatusID;
            }
            // kiểm tra ngày 
            if (booking.CheckInDate < DateTime.Today)
            {
                ModelState.AddModelError("CheckInDate", "Ngày nhận phòng không được nhỏ hơn ngày hiện tại.");
            }
            if (booking.CheckOutDate <= booking.CheckInDate)
            {
                ModelState.AddModelError("CheckOutDate", "Ngày trả phòng phải lớn hơn ngày nhận phòng.");
            }
            //==============
            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);

                // Cập nhật trạng thái phòng
                var room = db.Rooms.Find(booking.RoomId);
                if (room != null) room.RoomStatusId = 2;

                db.SaveChanges();

                // **Tính tiền và tạo Payment**
                decimal totalAmount = TinhTongTien(booking.BookingId);
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentMethod = "Chưa thanh toán",
                    Note = "Tạo tự động khi đặt phòng"
                };
                db.Payments.Add(payment);
                db.SaveChanges();

                if (isCustomer)
                {
                    TempData["SuccessMessage"] = "Đặt phòng thành công! Tổng tiền: " + totalAmount.ToString("N0") + " VNĐ";
                    return RedirectToAction("BookingSuccess", new { bookingId = booking.BookingId });
                }

                // Nếu không phải khách => Hiển thị lại form Create với thông báo
                ViewBag.SuccessMessage = "Đặt phòng thành công! Tổng tiền: " + totalAmount.ToString("N0") + " VNĐ";

                // Làm mới dropdown để giữ UI ổn định
                ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "FullName");
                ViewBag.BookingStatusID = new SelectList(db.BookingStatus1, "BookingStatusID", "StatusName");
                ViewBag.PaymentStatusID = new SelectList(db.PaymentStatus1, "PaymentStatusID", "StatusName");
                ViewBag.RoomId = new SelectList(db.Rooms.Where(r => r.RoomStatusId == 1), "RoomId", "RoomNumber");

                ModelState.Clear(); // xóa dữ liệu cũ
                return View(new Booking());
            }

            // Nếu có lỗi
            if (!isCustomer)
            {
                ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "FullName", booking.CustomerId);
                ViewBag.BookingStatusID = new SelectList(db.BookingStatus1, "BookingStatusID", "StatusName", booking.BookingStatusID);
                ViewBag.PaymentStatusID = new SelectList(db.PaymentStatus1, "PaymentStatusID", "StatusName", booking.PaymentStatusID);
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "RoomNumber", booking.RoomId);

            return View(booking);
        }


        // Trang cảm ơn
        public ActionResult BookingSuccess(int bookingId)
        {
            var booking = db.Bookings
                            .Include(b => b.Customer)
                            .Include(b => b.Room)
                            .FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return HttpNotFound();

            var totalAmount = db.Payments.FirstOrDefault(p => p.BookingId == bookingId)?.TotalAmount ?? 0;
            ViewBag.TotalAmount = totalAmount;
            return View(booking);
        }

        // Hàm tính tổng tiền
        private decimal TinhTongTien(int bookingId)
        {
            var booking = db.Bookings
                            .Include("Room")
                            .Include("BookingServices.Service")
                            .FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return 0;

            int days = 1;
            if (booking.CheckInDate.HasValue && booking.CheckOutDate.HasValue)
            {
                days = (booking.CheckOutDate.Value - booking.CheckInDate.Value).Days;
                if (days <= 0) days = 1;
            }

            decimal total = (booking.Room.Price ?? 0) * days;

            foreach (var svc in booking.BookingServices)
            {
                total += (svc.Quantity ?? 0) * (svc.Service.Price ?? 0);
            }

            return total;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // GET: Bookings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Booking booking = db.Bookings
                                .Include(b => b.Customer)
                                .Include(b => b.Room)
                                .Include(b => b.BookingStatu)
                                .Include(b => b.PaymentStatu)
                                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null) return HttpNotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            if (booking != null)
            {
                // Xóa Payment liên quan
                var payment = db.Payments.FirstOrDefault(p => p.BookingId == id);
                if (payment != null)
                {
                    db.Payments.Remove(payment);
                }

                // Cập nhật trạng thái phòng
                var room = db.Rooms.Find(booking.RoomId);
                if (room != null)
                {
                    // Kiểm tra còn booking nào khác đang sử dụng phòng này không
                    var cancelledStatusId = db.BookingStatus1.FirstOrDefault(s => s.StatusName == "Đã hủy")?.BookingStatusID;
                    var checkedOutStatusId = db.BookingStatus1.FirstOrDefault(s => s.StatusName == "Đã trả phòng")?.BookingStatusID;

                    bool roomStillUsedByOtherActiveBookings = db.Bookings.Any(b =>
                        b.RoomId == room.RoomId &&
                        b.BookingId != id && // Không tính booking này
                        (b.BookingStatusID != cancelledStatusId && b.BookingStatusID != checkedOutStatusId)
                    );

                    if (!roomStillUsedByOtherActiveBookings)
                    {
                        room.RoomStatusId = 1; // 1 = Trống
                    }
                }

                // Xóa booking
                db.Bookings.Remove(booking);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        // Đặt phòng nhanh 
        // GET: Bookings/QuickBookingForm
        public ActionResult QuickBookingForm(int roomId)
        {
            var room = db.Rooms.Find(roomId);
            if (room == null)
            {
                return HttpNotFound("Không tìm thấy phòng.");
            }

            // Tìm trạng thái "Chờ nhận phòng"
            var bookingStatus = db.BookingStatus1.FirstOrDefault(s => s.StatusName == "Chờ nhận phòng");
            var paymentStatus = db.PaymentStatus1.FirstOrDefault(s => s.StatusName == "Chưa thanh toán");

            if (bookingStatus == null || paymentStatus == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest,
                    "Thiếu trạng thái đặt phòng hoặc trạng thái thanh toán trong cơ sở dữ liệu.");
            }

            // ViewBag cho dropdown
            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "FullName");
            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "RoomNumber", room.RoomId);

            // Trả về partial view
            return PartialView("_QuickBookingForm", new Booking
            {
                RoomId = room.RoomId,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1),
                NumberOfPeople = 1,
                BookingStatusID = bookingStatus.BookingStatusID,
                PaymentStatusID = paymentStatus.PaymentStatusID
            });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuickBookingSubmit(Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);

                var room = db.Rooms.Find(booking.RoomId);
                if (room != null)
                {
                    room.RoomStatusId = 2; // Đang sử dụng
                }

                db.SaveChanges(); // Lưu Booking trước để có BookingId

                decimal totalAmount = TinhTongTien(booking.BookingId);

                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentMethod = "Chưa thanh toán",
                    Note = "Tạo tự động từ Quick Booking"
                };

                db.Payments.Add(payment);
                db.SaveChanges();

                TempData["QuickBookingSuccess"] = "Đặt phòng thành công!";

                // Gửi lại 1 đoạn HTML nhẹ để JS kiểm tra class là success
                return Content("<div class='booking-success'></div>", "text/html");
            }

            // Nếu có lỗi -> load lại dropdown và render lại form
            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "FullName", booking.CustomerId);
            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "RoomNumber", booking.RoomId);

            return PartialView("_QuickBookingForm", booking);
        }


        // GET: Bookings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Booking booking = db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .Include(b => b.BookingStatu)
                .Include(b => b.PaymentStatu)
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null) return HttpNotFound();

            bool isCustomer = (Session["RoleName"] != null && Session["RoleName"].ToString() == "Khách hàng");

            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "RoomNumber", booking.RoomId);

            if (!isCustomer)
            {
                ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "FullName", booking.CustomerId);
                ViewBag.BookingStatusID = new SelectList(db.BookingStatus1, "BookingStatusID", "StatusName", booking.BookingStatusID);
                ViewBag.PaymentStatusID = new SelectList(db.PaymentStatus1, "PaymentStatusID", "StatusName", booking.PaymentStatusID);
            }

            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookingId,CustomerId,RoomId,CheckInDate,CheckOutDate,NumberOfPeople,Note,PaymentStatusID,BookingStatusID")] Booking model)
        {
            if (model == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            bool isCustomer = (Session["RoleName"] != null && Session["RoleName"].ToString() == "Khách hàng");

            // validate ngày giống Create
            if (model.CheckInDate < DateTime.Today)
            {
                ModelState.AddModelError("CheckInDate", "Ngày nhận phòng không được nhỏ hơn ngày hiện tại.");
            }
            if (model.CheckOutDate <= model.CheckInDate)
            {
                ModelState.AddModelError("CheckOutDate", "Ngày trả phòng phải lớn hơn ngày nhận phòng.");
            }

            if (ModelState.IsValid)
            {
                var booking = db.Bookings.Find(model.BookingId);
                if (booking == null) return HttpNotFound();

                // Nếu đổi phòng -> cập nhật trạng thái phòng cũ và phòng mới
                if (booking.RoomId != model.RoomId)
                {
                    var oldRoom = db.Rooms.Find(booking.RoomId);
                    if (oldRoom != null)
                    {
                        var cancelledStatusId = db.BookingStatus1.FirstOrDefault(s => s.StatusName == "Đã hủy")?.BookingStatusID;
                        var checkedOutStatusId = db.BookingStatus1.FirstOrDefault(s => s.StatusName == "Đã trả phòng")?.BookingStatusID;

                        bool roomStillUsedByOtherActiveBookings = db.Bookings.Any(b =>
                            b.RoomId == oldRoom.RoomId &&
                            b.BookingId != booking.BookingId &&
                            (b.BookingStatusID != cancelledStatusId && b.BookingStatusID != checkedOutStatusId)
                        );

                        if (!roomStillUsedByOtherActiveBookings)
                        {
                            oldRoom.RoomStatusId = 1; // Trống
                        }
                    }

                    var newRoom = db.Rooms.Find(model.RoomId);
                    if (newRoom != null)
                    {
                        newRoom.RoomStatusId = 2; // Đang sử dụng
                    }

                    booking.RoomId = model.RoomId;
                }

                // Chỉ admin/nhân viên mới được thay đổi Customer/Status
                if (!isCustomer)
                {
                    booking.CustomerId = model.CustomerId;
                    booking.BookingStatusID = model.BookingStatusID;
                    booking.PaymentStatusID = model.PaymentStatusID;
                }

                // Cập nhật các trường khác
                booking.CheckInDate = model.CheckInDate;
                booking.CheckOutDate = model.CheckOutDate;
                booking.NumberOfPeople = model.NumberOfPeople;
                booking.Note = model.Note;

                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();

                // Tính lại Payment (nếu có) hoặc tạo mới
                decimal totalAmount = TinhTongTien(booking.BookingId);
                var payment = db.Payments.FirstOrDefault(p => p.BookingId == booking.BookingId);
                if (payment != null)
                {
                    payment.TotalAmount = totalAmount;
                    payment.PaymentDate = DateTime.Now;
                    db.Entry(payment).State = EntityState.Modified;
                }
                else
                {
                    db.Payments.Add(new Payment
                    {
                        BookingId = booking.BookingId,
                        PaymentDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        PaymentMethod = "Chưa thanh toán",
                        Note = "Tạo tự động khi sửa booking"
                    });
                }
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật đặt phòng thành công.";
                return RedirectToAction("Index");
            }

            // nếu model invalid -> load lại dropdown và trả view
            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "RoomNumber", model.RoomId);
            if (!isCustomer)
            {
                ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "FullName", model.CustomerId);
                ViewBag.BookingStatusID = new SelectList(db.BookingStatus1, "BookingStatusID", "StatusName", model.BookingStatusID);
                ViewBag.PaymentStatusID = new SelectList(db.PaymentStatus1, "PaymentStatusID", "StatusName", model.PaymentStatusID);
            }
            return View(model);
        }

    }
}
