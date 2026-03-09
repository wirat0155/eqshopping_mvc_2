function formatDate(dateString) {
    // Check if the dateString is null or invalid
    if (!dateString) {
        return "-";
    }
    // Create a new Date object from the input string
    const date = new Date(dateString);

    // Format the date to 'DD MMM YYYY'
    const options = { day: '2-digit', month: 'short', year: 'numeric' };
    return new Intl.DateTimeFormat('en-GB', options).format(date);
}

function formatDateSort(dateString) {
    // Create a new Date object from the input string
    const date = new Date(dateString);

    // Check if the date is valid
    if (isNaN(date.getTime())) {
        return "-";
    }

    // Extract components of the date
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Month is 0-indexed
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    // Format the date to 'YYYYMMDDHHMMSS'
    return `${year}${month}${day}${hours}${minutes}${seconds}`;
}

function formatDateForInput(dateString) {
    // Create a Date object from the ISO string
    const date = new Date(dateString);

    // Extract the year, month, and day without altering the timezone
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0'); // Months are zero-indexed, so add 1
    const day = date.getDate().toString().padStart(2, '0');

    // Return the date in YYYY-MM-DD format
    return `${year}-${month}-${day}`;
}

function formatDateNumber(dateString) {
    if (!dateString) return "";

    let date = new Date(dateString);
    if (isNaN(date.getTime())) return ""; // Handle invalid dates

    let day = String(date.getDate()).padStart(2, '0');
    let month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-based
    let year = date.getFullYear();
    let hours = String(date.getHours()).padStart(2, '0');
    let minutes = String(date.getMinutes()).padStart(2, '0');

    return `${day}-${month}-${year} ${hours}:${minutes}`;
}
function formatDateddMMyyyy(dateString) {
    if (!dateString) return ""; // Handle empty or null values

    let date = new Date(dateString);
    if (isNaN(date.getTime())) return ""; // Handle invalid dates

    let day = String(date.getDate()).padStart(2, '0'); // Ensure two digits
    let month = String(date.getMonth() + 1).padStart(2, '0'); // Month is 0-based
    let year = date.getFullYear();

    return `${day}-${month}-${year}`;
}

