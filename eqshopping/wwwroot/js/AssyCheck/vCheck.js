$(document).ready(function () {
  setFocus();
});

function setFocus() {
  $("#txt_assylabel").focus();
  setTimeout(function () {
    $("#txt_assylabel").focus();
  }, 500);
}

$("#txt_assylabel").on("input", function () {
  var val = $(this).val();
  if (val.length >= 12) {
    checkAssy(val);
  }
});

$("#txt_assylabel").on("keypress", function (e) {
  if (e.which == 13) {
    var val = $(this).val();
    if (val != "") {
      checkAssy(val);
    }
  }
});

function checkAssy(val) {
  $("#txt_assylabel").prop("disabled", true); // Disable immediately
  $.ajax({
    url: basePath + "/AssyCheck/GetAssy",
    type: "POST",
    data: {
      txt_assylabel: val,
      assy_user: localStorage.getItem("eqs_assy_empno") || ""
    },
    success: function (res) {
      if (res.success) {
        // Show data (placeholder)
        console.log(res.data);

        // Check Product Type removed

        Swal.fire({
          icon: "success",
          title: "พบข้อมูล",
          text:
            "Part No: " + res.data.partNo + ", Type: " + res.data.productType,
          timer: 1500,
          showConfirmButton: false,
        }).then(() => {
          // Show part no input
          $("#div_partno").show();
          $("#txt_partno").val("").focus();

          // Display Scan Count for ALL types
          if (res.scanCount !== undefined && res.targetCount !== undefined) {
            updateScanStatus(res.scanCount, res.targetCount);
          } else {
            $("#div_scan_count").hide();
          }
        });

        // Display History
        renderHistory(res.history);

        $("#btn_reset").show();
      } else {
        Swal.fire({
          icon: "error",
          title: "ข้อผิดพลาด",
          text: res.message || "ไม่พบข้อมูล",
          allowOutsideClick: false,
        }).then(() => {
          $("#btn_reset").show();
        });
      }
    },
    error: function (err) {
      console.error(err);
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "An error occurred while checking Assy.",
      });
      $("#btn_reset").show();
    },
  });
}

var partNoTimeout;
$("#txt_partno").on("input", function () {
  var val = $(this).val();
  clearTimeout(partNoTimeout);
  
  if (val !== "") {
    partNoTimeout = setTimeout(function() {
        var assyLabel = $("#txt_assylabel").val();
        if (assyLabel != "") {
          checkPartNo(assyLabel, val);
        }
    }, 1000);
  }
});

$("#txt_partno").on("keypress", function (e) {
  if (e.which == 13) {
    clearTimeout(partNoTimeout);
    var assyLabel = $("#txt_assylabel").val();
    var partNo = $(this).val();

    if (assyLabel != "" && partNo != "") {
      checkPartNo(assyLabel, partNo);
    }
  }
});

function checkPartNo(assyLabel, partNo) {
  $("#txt_partno").prop("disabled", true);
  $.ajax({
    url: basePath + "/AssyCheck/CheckPartNo",
    type: "POST",
    data: {
      txt_assylabel: assyLabel,
      txt_partno: partNo,
      assy_user: localStorage.getItem("eqs_assy_empno") || ""
    },
    success: function (res) {
      if (res.success) {
        var successMsg = res.message || "ตรวจสอบสำเร็จ";
        Swal.fire({
          icon: "success",
          title: successMsg,
          timer: 1500,
          showConfirmButton: false,
        });

        // Update history
        renderHistory(res.history);

        // Update scan count if provided
        var isComplete = false;
        if (res.scanCount !== undefined && res.targetCount !== undefined) {
          updateScanStatus(res.scanCount, res.targetCount);
          if (res.scanCount >= res.targetCount) {
             isComplete = true;
          }
        }

        // Auto reset after 2 seconds ONLY if NOT complete
        if (!isComplete) {
            setTimeout(function() {
                resetScreen();
            }, 2000);
        }
      } else {
        Swal.fire({
          icon: "error",
          title: "ข้อผิดพลาด",
          text: res.message, // "Part No. ไม่ถูกต้อง"
          allowOutsideClick: false,
        }).then(() => {
          // Reset part no for retry? or Stop?
          // Request says: "ถ้า ไม่พบ ให้หยุด แล้วแสดง error message"
          // Assume stopping means no further action, but user can click start over.
          // Or maybe clear part no input and let them try again?
          // Usually "Stop" means they have to Reset.
          // I will keep it disabled.
        });
      }
    },
    error: function (err) {
      console.error(err);
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "An error occurred while checking Part No.",
      });
    },
  });
}

function renderHistory(historyData) {
  // Destroy existing DataTable if exists
  if ($.fn.DataTable.isDataTable("#tbl_history")) {
    $("#tbl_history").DataTable().destroy();
  }

  var tbody = $("#tbl_history tbody");
  tbody.empty();

  if (historyData && historyData.length > 0) {
    historyData.forEach(function (item) {
      var dateStr = "-";
      if (item.checkdate) {
        var d = new Date(item.checkdate);
        var day = ("0" + d.getDate()).slice(-2);
        var month = ("0" + (d.getMonth() + 1)).slice(-2);
        var year = d.getFullYear();
        var hour = ("0" + d.getHours()).slice(-2);
        var min = ("0" + d.getMinutes()).slice(-2);
        var sec = ("0" + d.getSeconds()).slice(-2);
        dateStr =
          day + "/" + month + "/" + year + " " + hour + ":" + min + ":" + sec;
      }

      var row =
        "<tr>" +
        '<td class="px-4">' +
        (item.assylabel || "") +
        "</td>" +
        "<td>" +
        (item.actual_partno || "") +
        "</td>" +
        "<td>" +
        (item.checkuser || "") +
        "</td>" +
        "<td>" +
        dateStr +
        "</td>" +
        '<td class="text-center text-success fw-bold"><i class="fas fa-check-circle"></i> OK</td>' +
        "</tr>";
      tbody.append(row);
    });

    // Initialize DataTable
    $("#tbl_history").DataTable({
      order: [[3, "desc"]], // Sort by Check Date desc by default
      searching: false,
      paging: false,
      info: false,
    });

    $("#tbl_history").show();
    $("#msg_no_history").hide();
  } else {
    $("#tbl_history").hide();
    $("#msg_no_history").show();
  }
  $("#div_history").show();
}

$("#btn_reset").click(function () {
  resetScreen();
});

function resetScreen() {
  $("#txt_assylabel").val("").prop("disabled", false);
  $("#div_partno").hide();
  $("#txt_partno").val("").prop("disabled", false);
  $("#div_history").hide();
  if ($.fn.DataTable.isDataTable("#tbl_history")) {
    $("#tbl_history").DataTable().destroy();
  }
  $("#tbl_history tbody").empty();

  // Reset scan count
  $("#lbl_scan_count")
    .text("0 / 0")
    .removeClass("text-success")
    .addClass("text-primary");
  $("#div_scan_count_card")
    .removeClass("border-success bg-success-subtle")
    .addClass("border-info bg-light");
  $("#lbl_scan_msg")
    .text("สแกนไปแล้ว / จำนวนที่ต้องสแกน")
    .removeClass("text-success")
    .addClass("text-muted");
  $("#div_scan_count").hide();

  $("#btn_reset").hide();
  setFocus();
}

function updateScanStatus(scanCount, targetCount) {
  if (targetCount === undefined || targetCount === 0) return;

  $("#lbl_scan_count").text(scanCount + " / " + targetCount);

  if (scanCount >= targetCount) {
    // Complete
    $("#lbl_scan_count").removeClass("text-primary").addClass("text-success");
    $("#div_scan_count .card")
      .first()
      .removeClass("border-info bg-light")
      .addClass("border-success bg-success-subtle"); // adjusting selector
    // Or simpler: add ID to card
    $("#lbl_scan_msg")
      .text("Completed! สแกนครบตามจำนวนแล้ว")
      .removeClass("text-muted")
      .addClass("text-success fw-bold");

    // Hide Part No input
    $("#div_partno").hide();
  } else {
    // Not Complete
    $("#lbl_scan_count").removeClass("text-success").addClass("text-primary");
    $("#div_scan_count .card")
      .first()
      .removeClass("border-success bg-success-subtle")
      .addClass("border-info bg-light");
    $("#lbl_scan_msg")
      .text("สแกนไปแล้ว / จำนวนที่ต้องสแกน")
      .removeClass("text-success fw-bold")
      .addClass("text-muted");

    // Show Part No input (if Assy Check was successful)
    // This function is called after success, so showing is fine.
    $("#div_partno").show();
    $("#txt_partno").prop("disabled", false).val("").focus();
  }
  $("#div_scan_count").show();
}
