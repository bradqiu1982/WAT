var WTRACE = function () {
    var TraceInfo = function () {
        var watdatatable = null;

        function commitwafer() {
            var wafer = $('#wafer').val();
            var traceid = $('#traceid').val();
            var prod = $('#prod').val();
            var pn = $('#pn').val();
            var deliever = $('#deliever').val();
            var priority = $('#priority').val();

            if (wafer == '' || traceid == '' || prod == '')
            { alert('wafer number,trace id, product should not be empty!'); return false; }


            wafer = wafer.toUpperCase();
            waferfail = true;
            if((wafer.indexOf('E') != -1 || wafer.indexOf('R') != -1|| wafer.indexOf('T') != -1)
                && (wafer.indexOf('08') != -1 || wafer.indexOf('09') != -1))
            { waferfail = false; }

            if(waferfail)
            { alert('wafer number should contains suffix E08 or E09 ...!'); return false;}

            $.post('/Main/CommitWaferTraceData', {
                wafer: wafer,
                traceid: traceid,
                prod: prod,
                pn: pn,
                deliever: deliever,
                priority: priority
            }, function (output) {
                if (output.res)
                { window.location.reload(true); }
                else
                { alert(output.msg); return false;}
            })
        }


        $('body').on('click', '#btn-search', function () {
            commitwafer();
        })


        $.fn.dataTable.ext.buttons.logist = {
            text: 'Refresh Logistics',
            action: function (e, dt, node, config) {
            }
        };

        function loadtracedata()
        {
            $.post('/Main/LoadWaferTraceData', {}, function (output) {

                if (watdatatable) {
                    watdatatable.destroy();
                    watdatatable = null;
                }
                
                $.each(output.wafertracelist, function (i, val) {
                    var tempstr = '';
                    tempstr += '<tr>';
                    tempstr += '<td>' + val.WaferNum + '</td>' +
                            '<td>' + val.Priority + '</td>' +
                            '<td>' + val.Product + '</td>' +
                            '<td>' + val.PN + '</td>' +
                            '<td>' + val.TraceCompany + '</td>' +
                            '<td>' + val.TraceID + '</td>' +
                            '<td>' + val.DeliverStatus + '</td>' +
                            '<td>' + val.ArriveDate + '</td>' +
                            '<td>' + val.Assemblyed + '</td>' +
                            '<td>' + val.TestStuatus + '</td>' +
                            '</tr>';
                    $("#watdatacontent").append(tempstr);
                });

                watdatatable = $('#watdatatable').DataTable({
                    'iDisplayLength': 100,
                    'aLengthMenu': [[100, 200, 300, -1],
                    [100, 200, 300, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all" }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5', 'logist']
                });

            })
        }

        $(function () {
            loadtracedata();
        });
    }

    return {
        TRACEINIT: function () {
            TraceInfo();
        }
    }
}();