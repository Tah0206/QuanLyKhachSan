document.addEventListener('DOMContentLoaded', function () {
    // Định dạng giá tiền khi nhập
    const giaInput = document.querySelector('#Gia');
    if (giaInput) {
        giaInput.addEventListener('input', function (e) {
            let value = this.value.replace(/\D/g, '');
            if (value) {
                value = parseInt(value, 10);
                this.value = value.toLocaleString('vi-VN');
            }
        });
    }

    // Thay đổi EditorFor TrangThai thành radio button
    const trangThaiContainer = document.querySelector('#TrangThai').parentNode;
    if (trangThaiContainer) {
        trangThaiContainer.innerHTML = `
            <div class="radio-group">
                <label class="radio-option">
                    <input type="radio" name="TrangThai" value="true" checked /> Đã thuê
                </label>
                <label class="radio-option">
                    <input type="radio" name="TrangThai" value="false" /> Trống
                </label>
            </div>
        `;
    }

    // Validate form trước khi submit
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function (e) {
            let isValid = true;

            // Validate Mã Phòng
            const maPhong = document.querySelector('#MaPhong').value.trim();
            if (!maPhong) {
                isValid = false;
                alert('Vui lòng nhập mã phòng');
                return false;
            }

            // Validate Tên Phòng
            const tenPhong = document.querySelector('#TenPhong').value.trim();
            if (!tenPhong) {
                isValid = false;
                alert('Vui lòng nhập tên phòng');
                return false;
            }

            // Validate Giá
            const gia = document.querySelector('#Gia').value.trim();
            if (!gia || isNaN(gia.replace(/\./g, ''))) {
                isValid = false;
                alert('Vui lòng nhập giá hợp lệ');
                return false;
            }

            if (!isValid) {
                e.preventDefault();
            }
        });
    }
});