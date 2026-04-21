$(document).ready(function () {
    // ===== FIX LỖI NGÀY THÁNG =====
    var hanNopInput = $("#hanNop");
    if (hanNopInput.val() && hanNopInput.val().indexOf("0001") > -1) {
        hanNopInput.val("");
    }

    var today = new Date().toISOString().split('T')[0];
    hanNopInput.attr('min', today);

    // ĐÃ XÓA: không còn set giá trị mặc định +7 ngày nữa

    // Biến lưu trạng thái đã tương tác với từng field chưa
    var touchedFields = {};

    // Hàm đánh dấu field đã được tương tác
    function markTouched(fieldId) {
        touchedFields[fieldId] = true;
    }

    // Hàm kiểm tra field đã được tương tác chưa
    function isTouched(fieldId) {
        return touchedFields[fieldId] === true;
    }

    // Hàm hiển thị lỗi
    function setError(id, msg) {
        $("#" + id).addClass("error-input");
        $("#err" + id.charAt(0).toUpperCase() + id.slice(1)).text(msg);
    }

    // Hàm ẩn lỗi
    function setSuccess(id) {
        $("#" + id).removeClass("error-input");
        $("#err" + id.charAt(0).toUpperCase() + id.slice(1)).text("");
    }

    // Validate một trường cụ thể (chỉ hiển thị lỗi nếu field đã được tương tác)
    function validateField(fieldId, showError = true) {
        let value = $("#" + fieldId).val().trim();
        let todayDate = new Date().toISOString().split('T')[0];
        let isValid = true;
        let errorMsg = "";

        switch (fieldId) {
            case "viTri":
                if (value === "") {
                    isValid = false;
                    errorMsg = "Vui lòng nhập vị trí tuyển dụng";
                }
                break;

            case "diaDiem":
                if (value === "") {
                    isValid = false;
                    errorMsg = "Vui lòng nhập địa điểm làm việc";
                }
                break;

            case "luong":
                if (value === "") {
                    isValid = false;
                    errorMsg = "Vui lòng nhập mức lương";
                } else {
                    let luongNum = parseFloat(value.replace(/[^0-9.-]/g, ''));
                    if (isNaN(luongNum) || luongNum <= 0) {
                        isValid = false;
                        errorMsg = "Mức lương phải là số lớn hơn 0";
                    }
                }
                break;

            case "soLuong":
                if (value === "") {
                    isValid = false;
                    errorMsg = "Vui lòng nhập số lượng cần tuyển";
                } else {
                    let soLuongNum = parseInt(value);
                    if (isNaN(soLuongNum) || soLuongNum <= 0) {
                        isValid = false;
                        errorMsg = "Số lượng phải là số nguyên lớn hơn 0";
                    }
                }
                break;

            case "hanNop":
                if (value === "") {
                    isValid = false;
                    errorMsg = "Vui lòng chọn hạn nộp hồ sơ";
                } else if (value < todayDate) {
                    isValid = false;
                    errorMsg = "Hạn nộp phải từ hôm nay trở đi";
                }
                break;

            case "moTa":
                if (value === "") {
                    isValid = false;
                    errorMsg = "Vui lòng nhập mô tả công việc";
                }
                break;

            case "yeuCau":
                if (value === "") {
                    isValid = false;
                    errorMsg = "Vui lòng nhập yêu cầu ứng viên";
                }
                break;
        }

        // Chỉ hiển thị lỗi nếu showError = true VÀ field đã được tương tác
        if (showError && isTouched(fieldId)) {
            if (!isValid) {
                setError(fieldId, errorMsg);
            } else {
                setSuccess(fieldId);
            }
        } else if (!showError && !isValid) {
            // Nếu chưa tương tác thì không hiển thị lỗi, nhưng vẫn clear class cũ
            $("#" + fieldId).removeClass("error-input");
        }

        return isValid;
    }

    // Validate toàn bộ form (dùng để kiểm tra trước khi submit)
    function validateFormForSubmit() {
        let isValid = true;
        const fields = ["viTri", "diaDiem", "luong", "soLuong", "hanNop", "moTa", "yeuCau"];

        fields.forEach(field => {
            // Đánh dấu tất cả field đã được tương tác khi submit
            markTouched(field);
            if (!validateField(field, true)) {
                isValid = false;
            }
        });

        return isValid;
    }

    // Cập nhật trạng thái nút Đăng tin (chỉ dựa vào validation không hiển thị lỗi)
    function updateButtonState() {
        let isValid = true;
        const fields = ["viTri", "diaDiem", "luong", "soLuong", "hanNop", "moTa", "yeuCau"];

        fields.forEach(field => {
            if (!validateField(field, false)) {
                isValid = false;
            }
        });

        $("#btnDangTin").prop("disabled", !isValid);
    }

    // ====================== SỰ KIỆN ======================

    // Khi focus vào field → đánh dấu đã tương tác
    $("input, textarea").on("focus", function () {
        const fieldId = $(this).attr("id");
        if (fieldId && fieldId !== "") {
            markTouched(fieldId);
        }
    });

    // Khi blur (click ra ngoài ô input) → kiểm tra và hiển thị lỗi
    $("input, textarea").on("blur", function () {
        const fieldId = $(this).attr("id");
        if (fieldId && fieldId !== "") {
            markTouched(fieldId);
            validateField(fieldId, true);
            updateButtonState();
        }
    });

    // Khi người dùng bắt đầu nhập (keyup) → ẩn lỗi ngay nếu đang hiển thị
    $("input, textarea").on("keyup", function () {
        const fieldId = $(this).attr("id");
        if (fieldId && fieldId !== "") {
            // Xóa lỗi ngay lập tức khi người dùng gõ
            setSuccess(fieldId);
            $("#" + fieldId).removeClass("error-input");
            updateButtonState();
        }
    });

    // Khi thay đổi giá trị (change) → cập nhật button state
    $("input, textarea").on("change", function () {
        updateButtonState();
    });

    // Xử lý click nút Đăng tin
    $("#btnDangTin").click(function () {
        // Đánh dấu tất cả field đã được tương tác để hiển thị lỗi đầy đủ
        const fields = ["viTri", "diaDiem", "luong", "soLuong", "hanNop", "moTa", "yeuCau"];
        fields.forEach(field => markTouched(field));

        if (!validateFormForSubmit()) {
            // Cuộn lên lỗi đầu tiên
            $('html, body').animate({
                scrollTop: $(".error:visible").first().offset().top - 100
            }, 500);
            return;
        }

        $(this).prop("disabled", true).text("Đang đăng...");

        $.ajax({
            url: '/TinTuyenDung/yeuCauDangTin',
            type: 'POST',
            data: $("#formDangTin").serialize(),
            success: function (res) {
                if (res.success) {
                    alert("Đăng tin tuyển dụng thành công!");
                    window.location.href = "/TinTuyenDung/vDanhSachTin";
                } else {
                    alert(res.message || "Có lỗi xảy ra khi đăng tin!");
                    $("#btnDangTin").prop("disabled", false).text("Đăng tin");
                }
            },
            error: function (xhr, status, error) {
                console.log("Lỗi:", error);
                alert("Có lỗi xảy ra, vui lòng thử lại sau!");
                $("#btnDangTin").prop("disabled", false).text("Đăng tin");
            }
        });
    });

    // Thêm CSS cho error-input
    $("<style>")
        .prop("type", "text/css")
        .html(`
            .error-input {
                border-color: #ef4444 !important;
                background-color: #fef2f2 !important;
            }
            .error {
                color: #ef4444;
                font-size: 13.5px;
                margin-top: 5px;
            }
        `)
        .appendTo("head");

    // Chỉ cập nhật trạng thái nút khi load trang (KHÔNG hiển thị lỗi)
    updateButtonState();

    // Debug
    console.log("dangtin.js đã được load thành công!");
});