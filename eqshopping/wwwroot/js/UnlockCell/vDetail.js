async function Save(event) {
    showLoader();
    removeError();
    event.preventDefault();

    try {
        const response = await fetch(`${basePath}/UnlockCell/Save`, {
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
                setTimeout(function () {
                    window.location.href = basePath + '/UnlockCell/vIndex';
                }, 1000);
            }
        }
    } catch ({ message }) {
        alert(`Exception: ${message}`);
    } finally {
        hideLoader();
    }
}

function txtUnlockReason_KeyPress(event) {
    event.preventDefault();
    var keyCode = event.keyCode ? event.keyCode : event.which ? event.which : event.charCode;
    if (keyCode == 13) {
        if ((document.getElementById("txtUnlockReason").readOnly == false) && (document.getElementById("txtUnlockReason").disabled == false)) {
            document.getElementById('btnSave').click();
        }
    }
}