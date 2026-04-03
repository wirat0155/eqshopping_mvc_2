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

function togglePassword(el) {
    const passwordInput = document.querySelector('input[name="txt_password"]');
    const eyeIcon = `
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-1">
            <path d="M10 12a2 2 0 1 0 4 0a2 2 0 0 0 -4 0" />
            <path d="M21 12c-2.4 4 -5.4 6 -9 6c-3.6 0 -6.6 -2 -9 -6c2.4 -4 5.4 -6 9 -6c3.6 0 6.6 2 9 6" />
        </svg>`;
    const eyeOffIcon = `
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-1">
            <path d="M10.584 10.587a2 2 0 0 0 2.828 2.83" />
            <path d="M9.363 5.365a9.467 9.467 0 0 1 2.637 -.365c4 0 7.333 2.333 10 7c-.778 1.361 -1.612 2.524 -2.503 3.488m-2.14 1.861c-1.631 1.1 -3.415 1.651 -5.357 1.651c-4 0 -7.333 -2.333 -10 -7c1.369 -2.395 2.913 -4.175 4.632 -5.341" />
            <path d="M3 3l18 18" />
        </svg>`;

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        el.innerHTML = eyeOffIcon;
        el.setAttribute('title', 'Hide password');
    } else {
        passwordInput.type = 'password';
        el.innerHTML = eyeIcon;
        el.setAttribute('title', 'Show password');
    }
}