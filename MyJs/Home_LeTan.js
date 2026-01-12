document.addEventListener('DOMContentLoaded', function () {
    const returnRoomBtn = document.getElementById('returnRoom');
    const confirmPaymentBtn = document.getElementById('confirmPayment');
    const paymentDetails = document.getElementById('paymentDetails');
    const totalAmount = document.getElementById('totalAmount');

    // Khởi tạo dữ liệu phòng
    function initializeRooms() {
        const roomElements = document.querySelectorAll('.box');
        return Array.from(roomElements).map(room => {
            const status = room.classList.contains('empty') ? 'empty' : 'occupied';
            return {
                element: room,
                number: room.dataset.room,
                price: parseInt(room.dataset.price) || 0,
                status: status,
                services: [],
                customer: null,
                rentDays: 0
            };
        });
    }

    let rooms = initializeRooms();

    // Hiển thị thông tin thanh toán khi chọn phòng
    function setupRoomEvents() {
        document.querySelectorAll('.box').forEach(box => {
            box.addEventListener('click', function (e) {
                if (e.target.classList.contains('open-room-btn')) return;

                const roomNumber = this.dataset.room;
                const room = rooms.find(r => r.number === roomNumber);

                showPaymentDetails(room);
                document.querySelectorAll('.box').forEach(b => b.classList.remove('selected'));
                this.classList.add('selected');
            });
        });
    }

    function showPaymentDetails(room) {
        if (room.status === 'occupied') {
            const servicesHTML = `
                <div class="room-info">
                    <h3>${room.number}</h3>
                    <p>Giá phòng: ${room.price.toLocaleString()} VND/ngày</p>
                    <p>Số ngày thuê: ${room.rentDays}</p>
                    <div class="customer-info">
                        <h4>THÔNG TIN KHÁCH HÀNG</h4>
                        <p>Họ tên: ${room.customer?.name || 'Chưa có thông tin'}</p>
                        <p>CCCD: ${room.customer?.id || 'Chưa có thông tin'}</p>
                        <p>SĐT: ${room.customer?.phone || 'Chưa có thông tin'}</p>
                    </div>
                </div>
                <div class="service-list">
                    <h4>DỊCH VỤ</h4>
                    ${room.services.length > 0
                    ? room.services.map(service => `
                            <div class="service-item">
                                <span>${service}</span>
                                <span>${getServicePrice(service).toLocaleString()} VND</span>
                            </div>
                        `).join('')
                    : '<p class="no-services">Không có dịch vụ nào</p>'
                }
                </div>
            `;

            const serviceTotal = room.services.reduce((sum, service) => sum + getServicePrice(service), 0);
            const total = (room.price * room.rentDays) + serviceTotal;
            totalAmount.textContent = total.toLocaleString() + ' VND';
            paymentDetails.innerHTML = servicesHTML;
        } else {
            paymentDetails.innerHTML = `
                <div class="room-info">
                    <h3>${room.number}</h3>
                    <p>Giá phòng: ${room.price.toLocaleString()} VND/ngày</p>
                    <p>Trạng thái: Trống</p>
                    <p class="no-services">Chưa có dịch vụ nào</p>
                </div>
            `;
            totalAmount.textContent = '0 VND';
        }
    }

    function getServicePrice(service) {
        const prices = {
            'Nước uống': 50000,
            'Giặt ủi': 100000,
            'Dọn phòng': 80000
        };
        return prices[service] || 0;
    }

    // Xử lý nút thanh toán
    confirmPaymentBtn.addEventListener('click', function () {
        const selectedRoom = document.querySelector('.box.selected');
        if (selectedRoom) {
            const roomNumber = selectedRoom.dataset.room;
            const room = rooms.find(r => r.number === roomNumber);

            if (room.status === 'occupied') {
                if (confirm(`Xác nhận thanh toán cho ${room.number}?`)) {
                    alert(`Thanh toán thành công cho ${room.number}`);
                    // Có thể thêm logic thanh toán ở đây
                }
            } else {
                alert('Phòng này đang trống, không thể thanh toán');
            }
        } else {
            alert('Vui lòng chọn phòng đang thuê để thanh toán');
        }
    });

    // Xử lý nút trả phòng
    returnRoomBtn.addEventListener('click', function () {
        const selectedRoom = document.querySelector('.box.selected');
        if (selectedRoom) {
            const roomNumber = selectedRoom.dataset.room;
            const room = rooms.find(r => r.number === roomNumber);

            if (room.status === 'occupied') {
                if (confirm(`Xác nhận trả phòng ${room.number}?`)) {
                    room.status = 'empty';
                    room.services = [];
                    room.customer = null;
                    room.rentDays = 0;

                    // Cập nhật giao diện
                    selectedRoom.classList.remove('selected');
                    showPaymentDetails(room);
                    updateRoomUI();

                    alert(`Đã trả phòng ${room.number} thành công`);
                }
            } else {
                alert('Phòng này đang trống, không thể trả phòng');
            }
        } else {
            alert('Vui lòng chọn phòng đang thuê để trả phòng');
        }
    });

    function updateRoomUI() {
        rooms.forEach(room => {
            room.element.className = 'box';
            room.element.classList.add(room.status);

            room.element.innerHTML = `
                <div class="room-info">
                    <div class="room-name">${room.number}</div>
                    <span class="room-status ${room.status}">
                        ${room.status === 'empty' ? 'Trống' : 'Đang thuê'}
                    </span>
                </div>
                ${room.status === 'empty'
                    ? '<a href="/ThongTinKhachHangs/Create" class="open-room-btn">Mở phòng</a>'
                    : ''
                }
            `;
        });

        setupRoomEvents();
    }

    // Khởi tạo
    setupRoomEvents();
});