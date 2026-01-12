using DoAnQuanLyKhachSan.Models;
using DoAnQuanLyKhachSan.Models.ViewModel;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;

public class AuthController : Controller
{
    private KhachSanEntities db = new KhachSanEntities();

    // Đăng nhập 
    public ActionResult Login()
    {
        return View();
    }

    // POST: /Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        string hashedPassword = ComputeSha256Hash(model.Password);
        var user = db.Users.FirstOrDefault(u =>
            u.Username == model.Username && u.PasswordHash == hashedPassword);

        if (user == null)
        {
            ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
            return View(model);
        }

        // Lấy vai trò người dùng
        var roleName = db.Roles.FirstOrDefault(r => r.RoleId == user.RoleId)?.RoleName;

        // Gán session dùng chung cho toàn hệ thống
        Session["UserId"] = user.UserId;
        Session["Username"] = user.Username;
        Session["RoleName"] = roleName;

        // Gán thêm tên hiển thị (FullName) tùy theo vai trò
        if (roleName == "Khách hàng")
        {
            var customer = db.Customers.FirstOrDefault(c => c.UserId == user.UserId);
            if (customer != null)
            {
                Session["DisplayName"] = customer.FullName; 
            }

            return RedirectToAction("TrangChuNguoiDung", "Home");
        }
        else
        {
            var staff = db.Staffs.FirstOrDefault(s => s.UserId == user.UserId);
            if (staff != null)
            {
                Session["DisplayName"] = staff.FullName; 
            }

            return RedirectToAction("Index", "Home");
        }
    }


    // ĐĂNG KÝ 
    public ActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (db.Users.Any(u => u.Username == model.Username))
        {
            ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
            return View(model);
        }

        string hashedPassword = ComputeSha256Hash(model.Password);
        var roleId = db.Roles.FirstOrDefault(r => r.RoleName == "Khách hàng")?.RoleId ?? 4;

        var user = new User
        {
            Username = model.Username,
            PasswordHash = hashedPassword,
            FullName = model.FullName,
            Email = model.Email,
            RoleId = roleId
        };

        db.Users.Add(user);
        db.SaveChanges();

        TempData["Success"] = "Đăng ký thành công!";
        return RedirectToAction("Login");
    }

    //  ĐĂNG XUẤT 
    public ActionResult Logout()
    {
        Session.Clear(); 
        return RedirectToAction("Login"); 
    }

    // MÃ HÓA PASSWORD
    private string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
    }

    //  QUÊN MẬT KHẨU 
    public ActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "Email không tồn tại.");
            return View(model);
        }

        // Tạo token và thời hạn
        string token = Guid.NewGuid().ToString();
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.Now.AddMinutes(15);
        db.SaveChanges();

        string resetLink = Url.Action("ResetPassword", "Auth", new { email = user.Email, token = token }, protocol: Request.Url.Scheme);
        string emailBody = $"Nhấn vào liên kết sau để đặt lại mật khẩu: <a href='{resetLink}'>Đặt lại mật khẩu</a>";

        SendEmail(user.Email, "Khôi phục mật khẩu", emailBody);

        TempData["Success"] = "Liên kết đặt lại mật khẩu đã được gửi đến email.";
        return RedirectToAction("Login");
    }

    //  TRANG ĐẶT LẠI MẬT KHẨU 
    public ActionResult ResetPassword(string email, string token)
    {
        var user = db.Users.FirstOrDefault(u => u.Email == email && u.ResetToken == token);

        if (user == null || user.ResetTokenExpiry < DateTime.Now)
        {
            TempData["Error"] = "Liên kết không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction("Login");
        }

        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = db.Users.FirstOrDefault(u => u.Email == model.Email && u.ResetToken == model.Token);

        if (user == null || user.ResetTokenExpiry < DateTime.Now)
        {
            TempData["Error"] = "Token không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction("Login");
        }

        user.PasswordHash = ComputeSha256Hash(model.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        db.SaveChanges();

        TempData["Success"] = "Mật khẩu đã được đặt lại.";
        return RedirectToAction("Login");
    }

    // GỬI EMAIL 
    private void SendEmail(string toEmail, string subject, string body)
    {
        var fromEmail = "Tah02.06.05@gmail.com"; // thay bằng gmail thật
        var fromPassword = "owlh ygnj gjmb pqxe"; // Mật khẩu ứng dụng 

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromEmail, fromPassword),
            Timeout = 20000,
        };

        var mail = new MailMessage(fromEmail, toEmail)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        smtp.Send(mail);
    }
}
