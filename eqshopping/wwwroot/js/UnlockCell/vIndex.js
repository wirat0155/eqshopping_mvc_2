async function Search(event) {
    showLoader();
    removeError();
    event.preventDefault();

    try {
        const response = await fetch(`${basePath}/UnlockCell/Search`, {
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
                location.href = `${basePath}/UnlockCell/vDetail?pt=${plantNo}&c=${cellNo}`;
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
            // ตัดท้ายข้อความออก 2 digit
            var str = document.getElementById("txtCellNo").value;
            document.getElementById("txtCellNo").value = str.substring(0, str.length - 2);
            document.getElementById('btnSearch').click();
        }
    }
}