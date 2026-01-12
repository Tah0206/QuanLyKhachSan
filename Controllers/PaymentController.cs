using DoAnQuanLyKhachSan.Models;
using DoAnQuanLyKhachSan.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

public class PaymentController : Controller
{
    private readonly KhachSanEntities db = new KhachSanEntities();
    private readonly MomoService _momoService = new MomoService();

    // Hiển thị thông tin thanh toán
    public ActionResult Pay(int bookingId)
    {
        var payment = db.Payments
                        .Include(p => p.Booking.Customer)
                        .Include(p => p.Booking.Room)
                        .FirstOrDefault(p => p.BookingId == bookingId);

        if (payment == null)
            return HttpNotFound();

        return View(payment);
    }

    // Thanh toán tại quầy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ConfirmPay(int paymentId)
    {
        var payment = db.Payments.Include(p => p.Booking)
                                 .FirstOrDefault(p => p.PaymentId == paymentId);
        if (payment == null)
            return HttpNotFound();

        payment.PaymentDate = DateTime.Now;
        payment.PaymentMethod = "Tại quầy";
        payment.Note = "Đã thanh toán";

        var status = db.PaymentStatus1.FirstOrDefault(s => s.StatusName == "Đã thanh toán");
        if (status != null)
            payment.Booking.PaymentStatusID = status.PaymentStatusID;

        db.SaveChanges();

        TempData["SuccessMessage"] = "Thanh toán tại quầy thành công!";
        return RedirectToAction("TrangChuNguoiDung", "Home");
    }

    // Tạo yêu cầu thanh toán MoMo
    public async Task<ActionResult> CreateMomoPayment(int bookingId)
    {
        var payment = db.Payments.Include(p => p.Booking)
                                 .FirstOrDefault(p => p.BookingId == bookingId);
        if (payment == null)
            return HttpNotFound();

        if (payment.TotalAmount <= 0)
        {
            TempData["ErrorMessage"] = "Số tiền không hợp lệ!";
            return RedirectToAction("Pay", new { bookingId });
        }

        // Gọi MoMo API
        var momoResponse = await _momoService.CreatePaymentAsync(
            bookingId,
            payment.TotalAmount,
            $"Thanh toán đặt phòng #{bookingId}"
        );

        // Ghi log dữ liệu trả về để debug
        System.Diagnostics.Debug.WriteLine("MoMo Response: " + Newtonsoft.Json.JsonConvert.SerializeObject(momoResponse));

        // Luôn vào view để xem dữ liệu (kể cả khi lỗi)
        ViewBag.BookingId = bookingId;
        return View("CreateMomoPayment", momoResponse ?? new DoAnQuanLyKhachSan.Models.MoMo.MomoCreatePaymentResponseModel
        {
            Message = "Không nhận được phản hồi từ MoMo"
        });
    }
    // Người dùng quay lại sau khi thanh toán
    public ActionResult MomoCallback(string orderId, string resultCode, string message)
    {
        if (string.IsNullOrEmpty(orderId))
        {
            TempData["ErrorMessage"] = "Dữ liệu trả về không hợp lệ!";
            return RedirectToAction("TrangChuNguoiDung", "Home");
        }

        if (resultCode == "0" && int.TryParse(orderId, out int bookingId))
        {
            var payment = db.Payments.Include(p => p.Booking)
                                     .FirstOrDefault(p => p.BookingId == bookingId);
            if (payment != null)
            {
                payment.PaymentDate = DateTime.Now;
                payment.PaymentMethod = "MoMo";
                payment.Note = "Thanh toán MoMo thành công";

                var status = db.PaymentStatus1.FirstOrDefault(s => s.StatusName == "Đã thanh toán");
                if (status != null)
                    payment.Booking.PaymentStatusID = status.PaymentStatusID;

                db.SaveChanges();
            }

            TempData["SuccessMessage"] = "Thanh toán MoMo thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Thanh toán thất bại: " + (message ?? "Không xác định");
        }

        return RedirectToAction("TrangChuNguoiDung", "Home");
    }
    //vietqr
    public ActionResult GenerateBankQr(int bookingId)
    {
        var payment = db.Payments
                        .Include(p => p.Booking)
                        .FirstOrDefault(p => p.BookingId == bookingId);
        if (payment == null || payment.TotalAmount <= 0)
        {
            TempData["ErrorMessage"] = "Không tìm thấy thanh toán hoặc số tiền không hợp lệ.";
            return RedirectToAction("Pay", new { bookingId });
        }

        // Cấu hình thông tin ngân hàng nhận
        string bankCode = "MB"; // Ví dụ: MB Bank
        string accountNumber = "0898870437";
        string accountName = "NGUYEN THANH HO";
        string info = $"Thanh toan booking {bookingId}";
        decimal amount = payment.TotalAmount;

        // Encode nội dung chuyển khoản
        string addInfo = Uri.EscapeDataString(info);
        string encodedAccountName = Uri.EscapeDataString(accountName);

        // Thêm tham số ngẫu nhiên để tránh cache (bypass cache VietQR)
        string cacheBypass = DateTime.Now.Ticks.ToString();

        // Tạo URL QR theo chuẩn VietQR
        string qrUrl = $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png" +
                       $"?amount={(int)amount}&addInfo={addInfo}&accountName={encodedAccountName}&t={cacheBypass}";

        ViewBag.BookingId = bookingId;
        ViewBag.QrUrl = qrUrl;
        ViewBag.Amount = amount;
        ViewBag.Info = info;

        return View();
    }


    // MoMo gọi server (xác nhận backend)
    [HttpPost]
    public ActionResult MomoNotify()
    {
        // Có thể xử lý thêm xác nhận giao dịch
        return new HttpStatusCodeResult(200);
    }
}
