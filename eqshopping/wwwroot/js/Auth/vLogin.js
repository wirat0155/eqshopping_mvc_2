async function login(event) {
    showLoader();
    removeError();
    event.preventDefault();
    try {
        const response = await fetch(`${basePath}/Auth/Login`, {
            method: 'POST',
            body: new FormData(event.target)
        });
        const { success, text = "", errors = [] } = await response.json()
        console.log(success, errors);
        if (!success) {
            //showError(errors); // กันไม่ใช่ขนาดจอเกิน
            SwalNG(errors);
        }
        else {
            const empNo = $("[name='txt_empno']").val() || "";
            const plantno = $("[name='txt_plantno']").val() || "";

            if (empNo) {
                localStorage.setItem("eqs_loginuser", empNo);
                localStorage.setItem("eqs_plantno", plantno);
                localStorage.setItem("eqs_role", text);
            }
            location.href = `${basePath}/MainMenu/vList`;
        }
    } catch ({ message }) {
        alert(`Exception: ${message}`);
    }
    hideLoader();
}