using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAnQuanLyKhachSan.Models;

namespace DoAnQuanLyKhachSan.Controllers
{
    public class CustomersController : Controller
    {
        private KhachSanEntities db = new KhachSanEntities();

        // GET: Customers
        // GET: Customers
        public ActionResult Index(string search)
        {
            var customers = db.Customers.Include(c => c.User);

            if (!string.IsNullOrEmpty(search))
            {
                customers = customers.Where(c =>
                    c.FullName.Contains(search) ||
                    c.Phone.Contains(search) ||
                    c.User.Username.Contains(search)
                );
            }

            return View(customers.ToList());
        }


        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Find(id);
            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // GET: Customers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerId,FullName,Phone,Email,IdentityNumber,Address")] Customer customer)
        {
            int? userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                ModelState.AddModelError("", "Người dùng chưa đăng nhập.");
                return View(customer);
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra xem user này đã có Customer chưa
                var existingCustomer = db.Customers.FirstOrDefault(c => c.UserId == userId.Value);

                if (existingCustomer == null)
                {
                    // Nếu chưa có thì thêm mới
                    customer.UserId = userId.Value;
                    db.Customers.Add(customer);
                }
                else
                {
                    // Nếu đã có thì cập nhật
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.IdentityNumber = customer.IdentityNumber;
                    existingCustomer.Address = customer.Address;

                    db.Entry(existingCustomer).State = EntityState.Modified;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Thông tin đã được lưu thành công!";
                return RedirectToAction("TrangChuNguoiDung", "Home");
            }

            return View(customer);
        }


        // GET: Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Find(id);
            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerId,FullName,Phone,Email,IdentityNumber,Address")] Customer customer)
        {

            // Lấy dữ liệu cũ từ DB để cập nhật đúng bản ghi
            var existingCustomer = db.Customers.Find(customer.CustomerId);
            if (existingCustomer == null)
                return HttpNotFound();

            existingCustomer.FullName = customer.FullName;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Email = customer.Email;
            existingCustomer.IdentityNumber = customer.IdentityNumber;
            existingCustomer.Address = customer.Address;

            db.Entry(existingCustomer).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Find(id);
            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            db.Customers.Remove(customer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}


