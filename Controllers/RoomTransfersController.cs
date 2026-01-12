using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DoAnQuanLyKhachSan.Models;
using DoAnQuanLyKhachSan.Models.ViewModel;

namespace DoAnQuanLyKhachSan.Controllers
{
    public class RoomTransfersController : Controller
    {
        private KhachSanEntities db = new KhachSanEntities();

        // GET: RoomTransfers/GetTransferForm
        public ActionResult GetTransferForm(int bookingId, int fromRoomId)
        {
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            var currentRoom = db.Rooms.Find(fromRoomId);

            // Lấy các phòng có trạng thái là "Trống"
            var availableRooms = db.Rooms
                .Where(r => r.RoomId != fromRoomId && r.RoomStatu.StatusName == "Trống")
                .ToList();

            var viewModel = new RoomTransferViewModel
            {
                BookingId = bookingId,
                FromRoomId = fromRoomId,
                FromRoomName = currentRoom?.RoomNumber,
                AvailableRooms = availableRooms.Select(r => new SelectListItem
                {
                    Value = r.RoomId.ToString(),
                    Text = r.RoomNumber
                }).ToList()
            };

            return PartialView("_RoomTransferPartial", viewModel);
        }

        // POST: RoomTransfers/ChuyenPhong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChuyenPhong(RoomTransferViewModel model)
        {
            if (ModelState.IsValid)
            {
                var booking = db.Bookings.Find(model.BookingId);
                if (booking == null)
                {
                    return HttpNotFound();
                }

                var oldRoom = db.Rooms.FirstOrDefault(r => r.RoomId == model.FromRoomId);
                var newRoom = db.Rooms.FirstOrDefault(r => r.RoomId == model.ToRoomId);

                if (oldRoom == null || newRoom == null)
                {
                    return HttpNotFound("Không tìm thấy thông tin phòng.");
                }

                // Ghi nhận lịch sử chuyển phòng
                var transfer = new RoomTransfer
                {
                    BookingId = model.BookingId,
                    FromRoomId = model.FromRoomId,
                    ToRoomId = model.ToRoomId,
                    TransferDate = DateTime.Now,
                    Note = "Chuyển phòng theo yêu cầu khách"
                };

                // Cập nhật mã phòng mới cho booking
                booking.RoomId = model.ToRoomId;

                // Đảm bảo cập nhật đúng trạng thái phòng
                var statusTrong = db.RoomStatus.FirstOrDefault(s => s.StatusName == "Trống");
                var statusDangSuDung = db.RoomStatus.FirstOrDefault(s => s.StatusName == "Đang sử dụng");

                if (statusTrong == null || statusDangSuDung == null)
                {
                    return HttpNotFound("Không tìm thấy trạng thái phòng.");
                }

                oldRoom.RoomStatu = statusTrong;
                newRoom.RoomStatu = statusDangSuDung;

                db.RoomTransfers.Add(transfer);
                db.SaveChanges();

                return RedirectToAction("Index", "TongQuan");
            }

            // Nếu dữ liệu không hợp lệ, nạp lại view model
            var currentRoom = db.Rooms.Find(model.FromRoomId);
            var availableRooms = db.Rooms
                .Where(r => r.RoomId != model.FromRoomId && r.RoomStatu.StatusName == "Trống")
                .ToList();

            model.FromRoomName = currentRoom?.RoomNumber;
            model.AvailableRooms = availableRooms.Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = r.RoomNumber
            }).ToList();

            return PartialView("_RoomTransferPartial", model);
        }
    }
}
