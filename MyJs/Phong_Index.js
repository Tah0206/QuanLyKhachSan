document.addEventListener('DOMContentLoaded', function () {
    // ===== ĐỊNH DẠNG DỮ LIỆU =====
    formatData();

    // ===== XÁC NHẬN XÓA =====
    setupDeleteConfirmation();

    // ===== HIỆU ỨNG HOVER =====
    setupRowHoverEffect();

    // ===== TOOLTIP CHO ACTION =====
    setupActionTooltips();

    // ===== CHỨC NĂNG CHÍNH =====
    function formatData() {
        // Định dạng giá tiền
        document.querySelectorAll('.table td:nth-child(2)').forEach(td => {
            const price = parseFloat(td.textContent.trim());
            if (!isNaN(price)) {
                td.textContent = new Intl.NumberFormat('vi-VN', {
                    style: 'currency',
                    currency: 'VND'
                }).format(price);
            }
        });

        // Định dạng trạng thái
        document.querySelectorAll('.table td:nth-child(3)').forEach(td => {
            const status = td.textContent.trim();
            if (status === 'True') {
                td.textContent = 'ĐÃ THUÊ';
                td.setAttribute('data-status', 'True');
            } else if (status === 'False') {
                td.textContent = 'TRỐNG';
                td.setAttribute('data-status', 'False');
            }
        });
    }

    function setupDeleteConfirmation() {
        document.querySelectorAll('.table a[href*="Delete"]').forEach(link => {
            link.addEventListener('click', function (e) {
                if (!confirm('Bạn có chắc chắn muốn xóa phòng này?')) {
                    e.preventDefault();
                }
            });
        });
    }

    function setupRowHoverEffect() {
        document.querySelectorAll('.table tbody tr').forEach(row => {
            row.addEventListener('mouseenter', function () {
                this.style.transform = 'scale(1.01)';
                this.style.boxShadow = '0 5px 15px rgba(0,0,0,0.1)';
                this.style.transition = 'all 0.3s ease';
            });

            row.addEventListener('mouseleave', function () {
                this.style.transform = '';
                this.style.boxShadow = '';
            });
        });
    }

    function setupActionTooltips() {
        const tooltip = document.createElement('div');
        tooltip.className = 'action-tooltip';
        document.body.appendChild(tooltip);

        document.querySelectorAll('.table a').forEach(link => {
            link.addEventListener('mouseenter', function (e) {
                let text = '';
                if (this.textContent.includes('Edit')) text = 'Sửa thông tin';
                else if (this.textContent.includes('Details')) text = 'Xem chi tiết';
                else if (this.textContent.includes('Delete')) text = 'Xóa phòng';

                tooltip.textContent = text;
                tooltip.style.display = 'block';
                tooltip.style.left = `${e.pageX}px`;
                tooltip.style.top = `${e.pageY + 15}px`;
            });

            link.addEventListener('mouseleave', function () {
                tooltip.style.display = 'none';
            });

            link.addEventListener('mousemove', function (e) {
                tooltip.style.left = `${e.pageX}px`;
                tooltip.style.top = `${e.pageY + 15}px`;
            });
        });
    }
});

// ===== TOOLTIP STYLE =====
const style = document.createElement('style');
style.textContent = `
.action-tooltip {
    position: absolute;
    background-color: #333;
    color: white;
    padding: 5px 10px;
    border-radius: 4px;
    font-size: 12px;
    pointer-events: none;
    z-index: 1000;
    display: none;
    transform: translateX(-50%);
    white-space: nowrap;
}
`;
document.head.appendChild(style);