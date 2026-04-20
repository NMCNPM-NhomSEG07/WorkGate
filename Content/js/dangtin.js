// dangtin.js - ĐÃ SỬA THEO YÊU CẦU MỚI

$(document).ready(function () {

    // Hàm hiển thị lỗi
    function setError(id, msg) {
        $("#" + id).addClass("error-input");
        $("#err" + capitalizeFirstLetter(id)).text(msg);
    }

    // Hàm ẩn lỗi + thành công
    function setSuccess(id) {
        $("#" + id).removeClass("error-input");
        $("#err" + capitalizeFirstLetter(id)).text("");
    }

    // Hàm hỗ trợ viết hoa chữ cái đầu (ví dụ: viTri → errViTri)
    function capitalizeFirstLetter(string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    // Validate một trường cụ thể
    function validateField(fieldId) {
        let value = $("#" + fieldId).val().trim();
        let today = new Date().toISOString().split("T")[0];

        switch (fieldId) {
            case "viTri":
                if (value === "") {
                    setError("viTri", "Không được để trống");
                    return false;
                }
                break;

            case "diaDiem":
                if (value === "") {
                    setError("diaDiem", "Không được để trống");
                    return false;
                }
                break;

            case "luong":
                if (value === "" || parseFloat(value) <= 0) {
                    setError("luong", "Mức lương phải lớn hơn 0");
                    return false;
                }
                break;

            case "soLuong":
                if (value === "" || parseInt(value) <= 0) {
                    setError("soLuong", "Số lượng phải lớn hơn 0");
                    return false;
                }
                break;

            case "hanNop":
                if (value === "" || value < today) {
                    setError("hanNop", "Hạn nộp phải từ ngày mai trở đi");
                    return false;
                }
                break;

            case "moTa":
                if (value === "") {
                    setError("moTa", "Không được để trống");
                    return false;
                }
                break;

            case "yeuCau":
                if (value === "") {
                    setError("yeuCau", "Không được để trống");
                    return false;
                }
                break;
        }

        setSuccess(fieldId);
        return true;
    }

    // Validate toàn bộ form
    function validateForm() {
        let isValid = true;

        const fields = ["viTri", "diaDiem", "luong", "soLuong", "hanNop", "moTa", "yeuCau"];

        fields.forEach(field => {
            if (!validateField(field)) {
                isValid = false;
            }
        });

        $("#btnDangTin").prop("disabled", !isValid);
        return isValid;
    }

    // ====================== SỰ KIỆN ======================

    // Khi blur (click ra ngoài ô input) → kiểm tra lỗi
    $("input, textarea").on("blur", function () {
        const fieldId = $(this).attr("id");
        if (fieldId) {
            validateField(fieldId);
        }
    });

    // Khi người dùng bắt đầu nhập (keyup) → ẩn lỗi ngay
    $("input, textarea").on("keyup", function () {
        const fieldId = $(this).attr("id");
        if (fieldId) {
            setSuccess(fieldId);   // Ẩn lỗi khi đang gõ
        }
    });

    // Khi thay đổi giá trị (change) → validate lại
    $("input, textarea").on("change", function () {
        validateForm();
    });

    // Xử lý click nút Đăng tin
    $("#btnDangTin").click(function () {
        if (!validateForm()) {
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
                    location.href = "/TinTuyenDung/DanhSachTin";
                } else {
                    alert(res.message || "Có lỗi xảy ra khi đăng tin!");
                }
            },
            error: function () {
                alert("Lỗi kết nối với server!");
            },
            complete: function () {
                $("#btnDangTin").prop("disabled", false).text("Đăng tin");
            }
        });
    });

    // Gọi validate ban đầu (khi load trang)
    validateForm();
});