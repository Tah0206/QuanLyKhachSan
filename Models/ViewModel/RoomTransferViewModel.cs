using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DoAnQuanLyKhachSan.Models.ViewModel
{
    public class RoomTransferViewModel
    {
        public int BookingId { get; set; }
        public int FromRoomId { get; set; }
        public int ToRoomId { get; set; }

        public string FromRoomName { get; set; } // Tên phòng hiện tại (hiển thị trong form)
      
        public List<SelectListItem> AvailableRooms { get; set; } // Danh sách phòng trống có thể chuyển đến
    }
}
