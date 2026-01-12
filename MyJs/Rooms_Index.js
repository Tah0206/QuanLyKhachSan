// Hàm xử lý khi click vào 1 ô phòng
function toggleRoomDetails(element) {
    // Bỏ class 'active' khỏi tất cả các phòng khác
    document.querySelectorAll('.room-box').forEach(box => {
        if (box !== element) {
            box.classList.remove('active');
        }
    });

    // Thêm hoặc bỏ class 'active' cho phòng được click
    element.classList.toggle('active');
}

// Modal DOM elements
const modal = document.getElementById("roomModal");
const span = document.getElementsByClassName("close")[0];

// Khi click vào nút (x) để đóng modal
span.onclick = function () {
    modal.style.display = "none";
}

// Khi click bên ngoài modal thì cũng đóng modal
window.onclick = function (event) {
    if (event.target === modal) {
        modal.style.display = "none";
    }
}

// Hàm mở modal và hiển thị thông tin phòng
function openRoomDetails(roomId) {
    // Tạm thời hiển thị placeholder. Sau này có thể thay bằng fetch/AJAX
    document.getElementById("modalContent").innerHTML = `
        <p>Loading details for room <strong>${roomId}</strong>...</p>
    `;
    modal.style.display = "block";
}
