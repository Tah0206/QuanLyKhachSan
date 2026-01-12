
    document.addEventListener('DOMContentLoaded', function() {
    // Kiểm tra nếu vừa thêm tầng mới
    const urlParams = new URLSearchParams(window.location.search);
    const newFloorAdded = urlParams.get('newFloor');

    if (newFloorAdded) {
        const newFloor = document.querySelector(`.floor[data-floor-id="${newFloorAdded}"]`);
    if (newFloor) {
        newFloor.classList.add('new-floor');

            // Cuộn đến tầng mới
            setTimeout(() => {
        newFloor.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }, 100);
        }
    }
});
