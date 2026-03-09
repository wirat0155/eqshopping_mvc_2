function getQueryParams() {
    const params = {};
    const queryString = window.location.search.substring(1);
    const regex = /([^&=]+)=([^&]*)/g;
    let m;
    while (m = regex.exec(queryString)) {
        params[decodeURIComponent(m[1])] = decodeURIComponent(m[2]);
    }
    return params;
}

function formatNumber(value) {
    let num = parseFloat(value);
    return num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function removeError() {
    $(".error").text("");
}
function showError(errors) {
    errors.forEach(error => {
        const { property, errorMessage } = error;
        console.log(property, errorMessage);
        $("#" + property).text(errorMessage);
    });
}

