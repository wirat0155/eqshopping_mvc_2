$('form').on('input', 'input, select, textarea', function () {
    const inputName = $(this).attr('name');
    if (inputName) {
        // Escape special characters for the property name in the error object
        const escapedInputName = inputName.replace(/([.#\[\]\\'"])/g, '\\$&');
        $('.error#' + escapedInputName).text('');
    }
});

function removeError() {
    $(".error").text("");
}

function showError(errors, formid = "") {
    // Helper function to escape special characters in CSS selectors
    const escapeSelector = (selector) => selector.replace(/([.#\[\]\\'"])/g, '\\$&');

    // Determine the base selector based on whether formid is provided
    const baseSelector = formid ? `${formid} .error#` : ".error#";

    // Iterate through errors and display them
    errors.forEach(error => {
        const escapedProperty = escapeSelector(error.property);
        const errorElement = $(`${baseSelector}${escapedProperty}`);
        errorElement.text(error.errorMessage);
    });

    const topmostElement = errors
        .map(error => {
            const escapedProperty = escapeSelector(error.property);
            return $(`[name="${escapedProperty}"]`)[0];
        })
        .sort((a, b) => a.getBoundingClientRect().top - b.getBoundingClientRect().top)[0];

    if (topmostElement) {
        $(topmostElement).focus();
    }
}

function renderErrorSpans() {
    $('input[data-form], select[data-form], textarea[data-form]').each(function () {
        const formName = $(this).attr('data-form');
        if (!$('#' + formName).length) {
            $('<p>', { class: 'error text-danger', id: formName }).insertAfter(this);
        }
    });
}

const basePath = (window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1') ?  "/" + window.location.pathname.split('/')[1] : ''; // Adjust '/myapp' to your subfolder name

function SwalOK(text) {
    Swal.fire({
        title: "OK",
        text: text,
        icon: "success",
        timer: 2000
    });
}
function SwalNG(errors) {
    // แสดงเฉพาะ errorMessage จากสมาชิก [0] เท่านั้น
    if (Array.isArray(errors) && errors.length > 0) {
        const { property, errorMessage } = errors[0];
        Swal.fire({
            title: "NG",
            html: errorMessage + "<br/>ลอง Refresh หรือติดต่อ #8434",
            icon: "error"
        });
    } else {
        // fallback กรณีไม่ได้ส่ง array มา
        Swal.fire({
            title: "NG",
            html: errors + "<br/>ลอง Refresh หรือติดต่อ #8434",
            icon: "error"
        });
    }
}


function setDefaultValue() {
    // Get all input elements with type "date" or "datetime-local" and class "defaultValue"
    const inputs = document.querySelectorAll('input[type="date"].defaultValue, input[type="datetime-local"].defaultValue');

    // Get the current local date and time
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0'); // Months are zero-based
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');

    // Loop through each input element
    inputs.forEach(input => {
        // If the input has no value, set it to the current date or datetime
        if (!input.value) {
            if (input.type === 'date') {
                // For date inputs, set the value in YYYY-MM-DD format
                input.value = `${year}-${month}-${day}`;
            } else if (input.type === 'datetime-local') {
                // For datetime-local inputs, set the value in YYYY-MM-DDTHH:MM format
                input.value = `${year}-${month}-${day}T${hours}:${minutes}`;
            }
        }
    });

    // search input with name that contain "empno" with class "defaultValue"
    // and no value then set it to "240002"
    const storedEmpNo = localStorage.getItem("is_system_loginuser") || "";

    $('input[name*="empno"].defaultValue').each(function () {
        if ($(this).val().trim() === '') {
            $(this).val(storedEmpNo);
        }
    });
}

function toggle(elem) {
    if (elem.classList.contains("collapsed")) {
        document.querySelector("#sidebar-menu").classList.add("show");
        elem.classList.remove("collapsed");
        elem.setAttribute("aria-expanded", "true");
    } else {
        document.querySelector("#sidebar-menu").classList.remove("show");
        elem.classList.add("collapsed");
        elem.setAttribute("aria-expanded", "false");
    }
}
function toggleVerticalNavbar() {
    let $navbar = $(".navbar.navbar-vertical.navbar-expand-lg");
    let $topNavbar = $(".navbar-expand-lg.navbar-vertical ~ .navbar");
    let $pageWrapper = $(".navbar-expand-lg.navbar-vertical ~ .page-wrapper");

    if ($navbar.is(":visible")) {
        $navbar.hide(); // Hide navbar
        $topNavbar.css("margin-left", "0rem");
        $pageWrapper.css("margin-left", "0rem");
    } else {
        $navbar.show(); // Show navbar
        $topNavbar.css("margin-left", "");
        $pageWrapper.css("margin-left", "");
    }
}
function UpdateButtonStates(tableSelector) {
    const checkedRows = $(tableSelector).find('tbody input[type="checkbox"]:checked').closest('tr');
    $('#center-clone').prop('disabled', checkedRows.length !== 1);

    // Check if all checked rows have "item-quantity" value == 0 or don't have the class
    const allZeroQuantity = checkedRows.length > 0 && checkedRows.toArray().every(row => {
        const quantityText = $(row).find('.item-quantity').text().trim();
        return quantityText === '' || parseInt(quantityText, 10) === 0;
    });

    $('#center-delete').prop('disabled', !allZeroQuantity);
}

function CheckTrAll(masterCheckbox, tableSelector) {
    $(tableSelector).find('tbody input[type="checkbox"]').prop('checked', $(masterCheckbox).prop('checked'));
    UpdateButtonStates(tableSelector);
}

function CheckTr(checkbox, tableSelector) {
    UpdateButtonStates(tableSelector);
}

function preventBackForward() {
    history.pushState(null, null, location.href);
    window.addEventListener('popstate', function () {
        history.pushState(null, null, location.href);
        Swal.fire("ไม่สามารถย้อนกลับหน้านี้ได้");
    });

    // บางกรณี Android ใช้การ "swipe" หรือสลับ tab แทน
    document.addEventListener("visibilitychange", function () {
        if (document.visibilityState === "visible") {
            // ดัน state อีกครั้งเผื่อโดน back ผ่านระบบ
            history.pushState(null, null, location.href);
        }
    });
}

$(document).on("click", ".dt-orderable-asc.dt-orderable-desc", function () {
    $(".dt-orderable-asc.dt-orderable-desc").each(function () {
        console.log("test");
        let $this = $(this);
        let $iconContainer = $this.find(".dt-column-order");

        setTimeout(function () {  // Wait for 0.5 seconds before executing
            if ($this.hasClass("dt-ordering-asc")) {
                $iconContainer.html('<i style="font-size: 1rem;" class="bi bi-caret-down-square"></i>');
            } else if ($this.hasClass("dt-ordering-desc")) {
                $iconContainer.html('<i style="font-size: 1rem;" class="bi bi-caret-up-square"></i>');
            } else {
                $iconContainer.html('');
            }
        }, 500);
    });
});
