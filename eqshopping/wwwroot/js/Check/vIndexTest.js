async function GetFormView() {
    showLoader();
    const plantNo = localStorage.getItem("eqs_plantno") ?? "";
    const cellNo = $("[name='txt_cellno']").val() ?? "";
    const posno = $("[name='txt_posno']").val() ?? "";
    const assylabel = $("[name='txt_assylabel']").val() ?? "";

    
    const response = await $.post(`${basePath}/Check/GetFormView`, {
        txt_plantno: plantNo,
        txt_cellno: cellNo,
        txt_posno: posno,
        txt_assy: assylabel
    });

    $('#form').html(response);
    hideLoader();
    // Find the first enabled, visible input/select/textarea with no value
    const inputs = $('#form')
        .find('input:not(:disabled):visible, select:not(:disabled):visible, textarea:not(:disabled):visible');

    const nextInput = inputs.filter(function () {
        return !$(this).val();
    }).first();

    if (nextInput.length) {
        nextInput.focus();
    } else {
        // All fields have values, trigger the first visible submit button
        const submitBtn = $('button[type="submit"]:visible').first();
        if (submitBtn.length) {
            submitBtn.trigger('click');
        }
    }
}


async function Search(event) {
    showLoader();
    removeError();
    event.preventDefault();

    try {
        const response = await fetch(`${basePath}/Check/Search`, {
            method: 'POST',
            body: new FormData(event.target)
        });

        const contentType = response.headers.get('content-type');

        if (contentType && contentType.includes('application/json')) {
            const { success, text = "", errors = [] } = await response.json();
            console.log(success, text, errors);

            if (!success) {
                SwalNG(errors);
                showError(errors);
            }
            else {
                SwalOK(text);
                const plantNo = localStorage.getItem("eqs_plantno") ?? "";
                const cellNo = $("[name='txt_cellno']").val() ?? "";
                const posno = $("[name='txt_posno']").val() ?? "";
                const assylabel = $("[name='txt_assylabel']").val() ?? "";
                location.href = `${basePath}/Check/vDetail?pt=${plantNo}&c=${cellNo}&p=${posno}&a=${assylabel}`;
            }
        }
    } catch ({ message }) {
        alert(`Exception: ${message}`);
    } finally {
        hideLoader();
    }
}

function txtCellNo_KeyPress(event) {
    event.preventDefault();
    var keyCode = event.keyCode ? event.keyCode : event.which ? event.which : event.charCode;
    if (keyCode == 13) {
        if ((document.getElementById("txtCellNo").readOnly == false) && (document.getElementById("txtCellNo").disabled == false)) {
            event.preventDefault();
            // ตัดท้ายข้อความออก 2 digit
            var str = document.getElementById("txtCellNo").value;
            var xcellno = str.substring(0, str.length - 2);
            xcellno = xcellno.toUpperCase();
            document.getElementById("txtCellNo").value = xcellno;
            if (xcellno.length >= 3) {
                if (xcellno.substring(0, 3) == "MMT") {
                    document.getElementById("txtPOSNo").readOnly = true;
                    //document.getElementById("txtPOSNo").disabled = true;
                    document.getElementById("txtLotAssy").focus();
                }
                else {
                    document.getElementById("txtPOSNo").readOnly = false;
                    //document.getElementById("txtPOSNo").disabled = false;
                    document.getElementById("txtPOSNo").focus();
                }
            }
            else {
                if (xcellno == "D4" || xcellno == "P9") // Cell นี้ผลิตงาน Sub-Assy จะไม่มี Assy Label
                {
                    document.getElementById("txtLotAssy").readOnly = true;
                    //document.getElementById("txtLotAssy").disabled = true;
                    document.getElementById("txtPOSNo").readOnly = false;
                    //document.getElementById("txtPOSNo").disabled = false;
                    document.getElementById("txtPOSNo").focus();
                }
                else {
                    document.getElementById("txtLotAssy").readOnly = false;
                    //document.getElementById("txtLotAssy").disabled = false;
                    document.getElementById("txtPOSNo").readOnly = false;
                    //document.getElementById("txtPOSNo").disabled = false;
                    document.getElementById("txtPOSNo").focus();
                }
            }
        }
    }
}
async function txtPOSNo_KeyPress(event) {
    event.preventDefault();
    var keyCode = event.keyCode ? event.keyCode : event.which ? event.which : event.charCode;
    if (keyCode == 13) {
        if ((document.getElementById("txtPOSNo").readOnly == false) && (document.getElementById("txtPOSNo").disabled == false)) {
            var xlotassy = document.getElementById("txtPOSNo").value + "01";
            const plantNo = localStorage.getItem("eqs_plantno") ?? "";
            const posno = $("[name='txt_posno']").val() ?? "";

            try {
                const response = await $.post(`${basePath}/Check/GetAssy`, {
                    txt_plantno: plantNo,
                    txt_posno: posno
                });

                if (!response.success) {
                    SwalNG(response.text);
                } else {
                    if (response.result == "S") {
                        document.getElementById("txtLotAssy").value = xlotassy;
                        document.getElementById('btnSearch').click();
                    } else {
                        event.preventDefault();
                        document.getElementById("txtLotAssy").readOnly = false;
                        document.getElementById("txtLotAssy").disabled = false;
                        document.getElementById("txtLotAssy").focus();
                    }
                }
            } catch (error) {
                let message = "Error";

                if (error.responseJSON && error.responseJSON.text) {
                    message = error.responseJSON.text;
                } else if (error.responseText) {
                    try {
                        const parsed = JSON.parse(error.responseText);
                        message = parsed.text ?? JSON.stringify(parsed);
                    } catch (e) {
                        message = error.responseText;
                    }
                } else if (error.statusText) {
                    message = error.statusText;
                }

                SwalNG(message);
            }
        }
    }
}


function txtLotAssy_KeyPress(event) {
    var keyCode = event.keyCode ? event.keyCode : event.which ? event.which : event.charCode;
    if (keyCode == 13) {
        if ((document.getElementById("txtLotAssy").readOnly == false) && (document.getElementById("txtLotAssy").disabled == false)) {
            event.preventDefault();


            // 20250521 Wirat Sakorn
            var xcellno = document.getElementById("txtCellNo").value;
            if (xcellno.length >= 3) {
                if (xcellno.substring(0, 3) == "MMT") {
                    var lotAssy = document.getElementById("txtLotAssy").value;
                    document.getElementById("txtPOSNo").value = lotAssy.substring(0, lotAssy.length - 2);
                }
            }
            //
            document.getElementById('btnSearch').click();
        }
    }
}