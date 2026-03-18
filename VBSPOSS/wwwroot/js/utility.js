const strictVN = /^(?:0(3|5|7|8|9)\d{8}|(?:\+?84)(3|5|7|8|9)\d{8})$/;
const laxVN = /^(?:0|\+?84)[ .-]?(?:3|5|7|8|9)(?:[ .-]?\d){8}$/;

function getStatusDesc(value) {
    //0 - Đóng/Xóa; 1 - Tạo lập; 2 - Chờ duyệt; 3 - Phê duyệt; 4 - Từ chối
    if (value == 0) return "Đóng/Xóa";
    else if (value == 2) return "Chờ duyệt";
    else if (value == 3) return "Phê duyệt";
    else if (value == 4) return "Từ chối";
    else return "Tạo lập";
}

function isValidVNMobile(s, flexible = false) {
    return (flexible ? laxVN : strictVN).test(String(s).trim());
}