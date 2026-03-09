$(document).ready(function () {
    var table;

    // Default init
    initTable();

    $('#btnSearch').on('click', function () {
        initTable();
    });

    $('#cbo_mode').on('change', function () {
        var mode = $(this).val();
        if (mode === "ASSY") {
            $('#div_cell_no').hide();
        } else {
            $('#div_cell_no').show();
        }
    });

    function initTable() {
        if ($.fn.DataTable.isDataTable('#historyTable')) {
            $('#historyTable').DataTable().destroy();
        }
        
        // Clear table content completely to avoid column count mismatch
        $('#historyTable').empty();

        var mode = $('#cbo_mode').val();
        var columns = [];
        var thead = '';

        if (mode === "ASSY") {
            // Final Assy Columns
            thead = '<thead><tr>' +
                '<th>ID</th>' +
                '<th>Assy Label</th>' +
                '<th>Seq No</th>' +
                '<th>Check User</th>' +
                '<th>Check Date</th>' +
                '<th>Part No</th>' +
                '<th>Actual Part No</th>' +
                '<th>Check Flag</th>' +
                '</tr></thead>';
            
            columns = [
                { data: 'id' },
                { data: 'assylabel' },
                { data: 'sequenceno' },
                { data: 'checkuser' },
                { data: 'checkdate', render: function(data){ return formatDateTime(data); } },
                { data: 'partno' },
                { data: 'actual_partno' },
                { data: 'checkflag', render: function(data) { return data ? '<span class="badge bg-success text-white">สำเร็จ</span>' : '<span class="badge bg-danger text-white">ไม่สำเร็จ</span>'; } }
            ];
        } else {
            // EQ Shopping Columns (Default)
            thead = '<thead><tr>' +
                '<th>Tran ID</th>' +
                '<th>Plant No</th>' +
                '<th>POS No</th>' +
                '<th>Part No</th>' +
                '<th>Cell No</th>' +
                '<th>Finish Date</th>' +
                '<th>Start Check User</th>' +
                '<th>Seq No</th>' +
                '<th>Equip No</th>' +
                '<th>Check Date</th>' +
                '<th>Check User</th>' +
                '</tr></thead>';

            columns = [
                { data: 'tranid' },
                { data: 'plantno' },
                { data: 'posno' },
                { data: 'partno' },
                { data: 'cellno' },
                { data: 'checkingfinishdate', render: function(data){ return formatDateTime(data); } },
                { data: 'startCheckUser' }, 
                { data: 'seqno' },
                { data: 'equipmentno' },
                { data: 'checkingdate', render: function(data){ return formatDateTime(data); } },
                { data: 'checkinguser' }
            ];
        }

        $('#historyTable').append(thead);

        table = $('#historyTable').DataTable({
            destroy: true, // Ensure destroy is called
            serverSide: true,
            processing: true,
            ajax: {
                url: basePath + '/History/Search',
                type: 'POST',
                beforeSend: function() {
                    // showLoader(); 
                },
                data: function (d) {
                    d.mode = $('#cbo_mode').val();
                    d.startDate = $('input[name="startDate"]').val();
                    d.endDate = $('input[name="endDate"]').val();
                    d.posNos = $('input[name="posNos"]').val();
                    d.partNos = $('input[name="partNos"]').val();
                    d.cellNos = $('input[name="cellNos"]').val(); // Only relevant for EQS
                    d.plantno = localStorage.getItem("eqs_plantno");
                },
                complete: function() {
                    // hideLoader();
                },
                error: function (xhr, error, thrown) {
                    console.error(error);
                }
            },
            columns: columns,
            order: [[0, 'desc']], 
            searching: false,
            lengthMenu: [10, 25, 50, 100],
            pageLength: 10
        });
    }

     $('#btnExport').on('click', function () {
        var mode = $('#cbo_mode').val();
        var startDate = $('input[name="startDate"]').val();
        var endDate = $('input[name="endDate"]').val();
        var posNos = $('input[name="posNos"]').val();
        var partNos = $('input[name="partNos"]').val();
        var cellNos = $('input[name="cellNos"]').val();
        var plantno = localStorage.getItem("eqs_plantno");

        // showLoader();

        var form = $('<form method="POST" action="' + basePath + '/History/ExportExcel">');
        form.append($('<input type="hidden" name="mode">').val(mode));
        form.append($('<input type="hidden" name="startDate">').val(startDate));
        form.append($('<input type="hidden" name="endDate">').val(endDate));
        form.append($('<input type="hidden" name="posNos">').val(posNos));
        form.append($('<input type="hidden" name="partNos">').val(partNos));
        form.append($('<input type="hidden" name="cellNos">').val(cellNos));
        form.append($('<input type="hidden" name="plantno">').val(plantno));
        $('body').append(form);
        
        // setTimeout(function(){
        //     hideLoader();
        // }, 3000); 

        form.submit();
        form.remove();
    });

    function formatDateTime(date) {
        if (!date) return '';
        var d = new Date(date);
        var day = ('0' + d.getDate()).slice(-2);
        var month = ('0' + (d.getMonth() + 1)).slice(-2);
        var year = d.getFullYear();
        var hour = ('0' + d.getHours()).slice(-2);
        var min = ('0' + d.getMinutes()).slice(-2);
        var sec = ('0' + d.getSeconds()).slice(-2);
        return day + '/' + month + '/' + year + ' ' + hour + ':' + min + ':' + sec;
    }
});
