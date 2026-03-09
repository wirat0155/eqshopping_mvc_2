function txtEquipmentNo_KeyPress(event) {
    event.preventDefault();
    var keyCode = event.keyCode ? event.keyCode : event.which ? event.which : event.charCode;
    if (keyCode == 13) {
        if ((document.getElementById("txtEquipmentNo").readOnly == false) && (document.getElementById("txtEquipmentNo").disabled == false)) {
            event.preventDefault();
            // ตัดท้ายข้อความออก 2 digit
            var str = document.getElementById("txtEquipmentNo").value;
            var xcellno = document.getElementById("txtCellNo").value
            var xposno = document.getElementById("txtPOSNo").value
            var xpartno = document.getElementById("txtPartNo").value
            xcellno = xcellno.toUpperCase();

            if (xposno.substring(0, 1) == "B") { // ถ้าเป็น Brazing Plant
                if (str.slice(-2) == "-@") {
                    document.getElementById("txtEquipmentNo").value = str.substring(0, str.length - 2);
                } else {
                    //เช็คว่าเป็น Master Sample หรือเปล่า เพราะถ้าใช่จะต้องตัด -@ ออกด้วย แต่ถ้าไม่ใช่ก็ไม่ต้องตัดเพราะเราจะ PartNo ที่ Sub Assy Lable เลย
                    if ((xpartno.length + 2) == str.length) {

                        if (xpartno != str.substring(0, str.length - 2)) {
                            //กรณีเป็น Sub Assy
                            document.getElementById("txtEquipmentNo").value = str.substring(0, str.length - 2);
                        }

                    } else {
                        document.getElementById("txtEquipmentNo").value = str.substring(0, str.length - 2);
                    }
                }
                document.getElementById('btnSaveSub').click();
            } else { // ถ้าเป็น Plant อื่น ๆ 
                // 
                if (xcellno.length >= 3) {
                    //เช็คว่าเป็น Line ที่ Mitsu Onsite หรือเปล่า
                    if (xcellno.substring(0, 3) == "MMT") {
                        //เช็คว่าเป็น Master Sample หรือเปล่า เพราะถ้าใช่จะต้องตัด -@ ออกด้วย แต่ถ้าไม่ใช่ก็ไม่ต้องตัดเพราะเราจะ PartNo ที่ Sub Assy Lable เลย
                        if ((xpartno.length + 2) == str.length) {
                            if (xpartno == str.substring(0, str.length - 2)) {
                                document.getElementById("txtEquipmentNo").value = str.substring(0, str.length - 2);
                            }
                        }
                        //กรณีเป็น Sub Assy
                        document.getElementById('btnSaveSub').click();
                    } else {
                        if (xposno.substring(0, 1) == "U") {
                            if (str.slice(-2) == "-@") {
                                document.getElementById("txtEquipmentNo").value = str.substring(0, str.length - 2);
                                document.getElementById('btnSaveSub').click();
                            } else {
                                document.getElementById("txtEquipmentNo").value = str.substring(0, str.length);
                                document.getElementById('btnSaveSub').click();
                            }
                        }
                        else {
                            document.getElementById("txtEquipmentNo").value = str.substring(0, str.length - 2);
                            document.getElementById('btnSaveSub').click();

                        }
                    }
                }
                else {
                    if (xposno.substring(0, 1) == "U") {
                        if (str.slice(-2) == "-@") {
                            document.getElementById("txtEquipmentNo").value = str.substring(0, str.length - 2);
                            document.getElementById('btnSaveSub').click();
                        } else {
                            document.getElementById("txtEquipmentNo").value = str.substring(0, str.length);
                            document.getElementById('btnSaveSub').click();
                        }
                    }
                    else {
                        document.getElementById("txtEquipmentNo").value = str.substring(0, str.length - 2);
                        document.getElementById('btnSaveSub').click();

                    }

                }
            }
        }
    }
}
const audioNG = new Audio(`${basePath}/sound/ng.wav`);
const audioOK = new Audio(`${basePath}/sound/ok.wav`);
const audioFinish = new Audio(`${basePath}/sound/finish.wav`);

async function Save(event) {
    showLoader();
    removeError();
    event.preventDefault();

    try {
        const response = await fetch(`${basePath}/Check/Save`, {
            method: 'POST',
            body: new FormData(event.target)
        });

        const contentType = response.headers.get('content-type');

        if (contentType && contentType.includes('application/json')) {
            const { success, text = "", errors = [], finishflag = "" } = await response.json();
            console.log(success, text, errors);

            if (!success) {
                SwalNG(errors);
                showError(errors);
                audioNG.play().catch(error => console.error("เสียง NG ไม่เล่น:", error));
            } else {
                SwalOK(text);
                if (finishflag == true || finishflag == "true") {
                    audioFinish.play().catch(error => console.error("เสียง Finish ไม่เล่น:", error));
                } else {
                    audioOK.play().catch(error => console.error("เสียง OK ไม่เล่น:", error));
                }

                setTimeout(() => location.reload(), 500);
            }
        }
    } catch ({ message }) {
        alert(`Exception: ${message}`);
    } finally {
        hideLoader();
    }
}
