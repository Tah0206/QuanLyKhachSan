

# 📌 QuanLyKhachSan

Website quản lý khách sạn viết bằng **ASP.NET MVC + C# + SQL Server**.

Ứng dụng cho phép quản lý phòng, khách hàng, đặt phòng, thanh toán, thống kê… dựa trên mô hình **MVC** được xây dựng trong Visual Studio.

---

## 🧰 Yêu cầu môi trường

Trước khi chạy project bạn cần cài các công cụ sau:

1. **Visual Studio** (phiên bản 2019/2022)
2. **SQL Server** (Express hoặc bản đầy đủ)
3. **SQL Server Management Studio (SSMS)** để phục hồi database
4. .NET Framework phù hợp với project (kiểm tra trong `.csproj`)

---

## 🗄️ Thiết lập Database

### Phục hồi file `.BAK` vào SQL Server

1. Mở **SQL Server Management Studio (SSMS)**
2. Kết nối tới server của bạn
3. Chuột phải vào **Databases > Restore Database…**
4. Chọn **Device** và trỏ đến file `QLKhachSan.BAK` có trong project
5. Nhấn **OK** để phục hồi database
6. Sau khi hoàn tất bạn sẽ có database sẵn sàng để sử dụng

> Việc restore file .BAK sẽ tạo ra database với đầy đủ bảng dữ liệu cần thiết cho website hoạt động bình thường.

---

## 🔧 Cấu hình kết nối

Sau khi restore database, bạn cần chỉnh lại chuỗi kết nối trong project:

📍 Mở file **Web.config**

Tìm phần `<connectionStrings>` và cập nhật:

```
<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=TEN_DATABASE;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Lưu ý**

* `YOUR_SERVER_NAME`: tên SQL Server của máy bạn
* `TEN_DATABASE`: tên database vừa restore từ file `.BAK`

---

## 🚀 Chạy project

1. Mở **Visual Studio**
2. Chọn **Open > Project/Solution** và mở file `.sln`
3. Chọn build solution
4. Chỉnh startup project nếu cần
5. Nhấn **F5** để chạy web trên trình duyệt
6. Website sẽ chạy tại `http://localhost:XXXX/`

---

## 📌 Cấu trúc chính

Project hiện có các phần chính:

* `Models` chứa các lớp dữ liệu tương ứng bảng SQL
* `Controllers` xử lý logic và điều hướng
* `Views` chứa các trang giao diện (Razor)
* File database .BAK dùng để phục hồi dữ liệu

---

## 🛠 Công nghệ sử dụng

* C#
* ASP.NET MVC
* SQL Server
* Entity Framework / ADO.NET
* Bootstrap / jQuery cho UI

---

## 🧪 Thử nghiệm chức năng

Sau khi chạy bạn có thể thử các chức năng:

🎯 Đăng nhập/đăng ký
🎯 Xem danh sách phòng, khách
🎯 Thêm/sửa/xóa thông tin
🎯 Quản lý đặt phòng
🎯 Thống kê báo cáo

---

## 📌 Ghi chú

* Đảm bảo **SQL Server đã chạy và có quyền truy cập database**
* Nếu gặp lỗi kết nối, kiểm tra lại chuỗi connection và tên server
* Project xây dựng theo kiến trúc MVC chuẩn, dễ mở rộng ✨

---
Chúc các bạn sử dụng code vui vẻ
## 📌Lưu ý: code vẫn trong giai đoạn hoàn thiện, nếu có lỗi vui lòng báo cáo với nhóm chúng tôi qua email Tah02.06.05@gmail.com hoặc bạn cũng có thể tự hoàn thiện cũng được



