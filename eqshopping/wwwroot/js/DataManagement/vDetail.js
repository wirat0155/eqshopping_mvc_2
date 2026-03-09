async function Save(event) {
    showLoader();
    removeError();
    event.preventDefault();

    try {
        const response = await fetch(`${basePath}/DataManagement/Save`, {
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
                    location.reload();
                }, 1000);
            }
        }
    } catch ({ message }) {
        alert(`Exception: ${message}`);
    } finally {
        hideLoader();
    }
}

function confirmDelete() {
    if (confirm("คุณต้องการลบข้อมูลนี้หรือไม่?")) {
        DeleteTran();
    }
}

async function DeleteTran() {
    try {
        showLoader();

        // Send POST request to server
        const response = await $.post(`${basePath}/DataManagement/DeleteTran`, {
            txt_pos: $("[name='txt_posno']").val()
        });

        const { success, text = "", errors = [] } = response;

        // Handle server response
        if (!success) {
            showError(errors);
            if (text) {
                SwalNG(text);
            }
        } else {
            SwalOK(text);
            setTimeout(() => {
                location.href = `${basePath}/MainMenu/vList`;
            }, 1000);
        }
    } catch (error) {
        alert(`Exception: ${error.message}`);
    }
    hideLoader();
}