﻿var WTRACE = function () {
    var TraceInfo = function () {
        var watdatatable = null;

        function commitwafer() {
            var wafer = $('#wafer').val();
            var traceid = $('#traceid').val();
            var deliever = $('#deliever').val();
            var priority = $('#priority').val();

            if (wafer == '' || traceid == ''|| priority == '')
            { alert('wafer number,trace id,priority should not be empty!'); return false; }


            wafer = wafer.toUpperCase();
            waferfail = true;
            if((wafer.indexOf('E') != -1 || wafer.indexOf('R') != -1|| wafer.indexOf('T') != -1)
                && (wafer.indexOf('08') != -1 || wafer.indexOf('09') != -1))
            { waferfail = false; }

            if(waferfail)
            { alert('wafer number should contains suffix E08 or E09 ...!'); return false;}

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/CommitWaferTraceData', {
                wafer: wafer,
                traceid: traceid,
                deliever: deliever,
                priority: priority
            }, function (output) {
                $.bootstrapLoading.end();

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

                var options = {
                    loadingTips: "loading data......",
                    backgroundColor: "#aaa",
                    borderColor: "#fff",
                    opacity: 0.8,
                    borderColor: "#fff",
                    TipsColor: "#000",
                }
                $.bootstrapLoading.start(options);

                $.post('/Main/RefreshWaferLogis', {}, function (output) {
                    $.bootstrapLoading.end();
                    window.location.reload(true);
                });
            }
        };

        $.fn.dataTable.ext.buttons.refreshall = {
            text: 'Refresh All',
            action: function (e, dt, node, config) {

                var options = {
                    loadingTips: "loading data......",
                    backgroundColor: "#aaa",
                    borderColor: "#fff",
                    opacity: 0.8,
                    borderColor: "#fff",
                    TipsColor: "#000",
                }
                $.bootstrapLoading.start(options);

                $.post('/Main/RefreshWaferTrace', {}, function (output) {
                    $.bootstrapLoading.end();
                    window.location.reload(true);
                });
            }
        };

        function loadtracedata()
        {
            $.post('/Main/LoadWaferTraceData', {}, function (output) {

                if (watdatatable) {
                    watdatatable.destroy();
                    watdatatable = null;
                }
                $("#watdatacontent").empty();

                $.each(output.wafertracelist, function (i, val) {
                    var tempstr = '';

                    var cla = '';
                    if (val.DeliverStatus.indexOf('DELIVERED') != -1) { cla = 'ARRIVED'; }
                    if (val.Assemblyed != '') { cla = 'ASSEMBLYED'; }
                    if (val.TestStuatus != '') { cla = 'TESTING'; }

                    tempstr += '<tr>';
                    tempstr += '<td class="'+cla+'">' + val.WaferNum + '</td>' +

                            '<td><span id="' + val.WaferNum + 'PY">' + val.Priority + '</span>&nbsp;&nbsp;<span class="glyphicon glyphicon-arrow-up PYUP" myval="' + val.WaferNum
                            + '" aria-hidden="true"></span>&nbsp;&nbsp;&nbsp;&nbsp;<span class="glyphicon glyphicon-arrow-down PYDOWN" myval="' + val.WaferNum
                            +'" aria-hidden="true"></span>' + '</td>' +

                            '<td>' + val.Product + '</td>' +
                            '<td>' + val.PN + '</td>' +
                            '<td>' + val.TraceCompany + '</td>' +
                            '<td>' + val.TraceID + '</td>' +
                            '<td>' + val.DeliverStatus + '</td>' +
                            '<td>' + val.ArriveDate + '</td>' +
                            '<td>' + val.Assemblyed + '</td>';

                    if (val.TestStuatus.indexOf('PASS') != -1 || val.TestStuatus.indexOf('FAIL') != -1)
                    { tempstr += '<td class="' + val.TestStuatus + '">' + val.TestStuatus + '</td>'; }
                    else
                    { tempstr += '<td>' + val.TestStuatus + '</td>'; }

                    tempstr += '<td>' + val.ECD + '</td>';

                    tempstr += '</tr>';
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
                    buttons: ['copyHtml5', 'csv', 'excelHtml5', 'logist', 'refreshall']
                });

            })
        }

        $(function () {
            loadtracedata();
        });

        $('body').on('click', '.PYUP', function () {
            var wf = $(this).attr("myval");

            $.post('/Main/WaferTracePriority', {
                act: 'UP',
                wf:wf
            }, function (output) {
                if (output.res)
                { $('#' + wf + 'PY').html(output.py); }
            });
            

        })

        $('body').on('click', '.PYDOWN', function () {

            var wf = $(this).attr("myval");

            $.post('/Main/WaferTracePriority', {
                act: 'DOWN',
                wf: wf
            }, function (output) {
                if (output.res)
                { $('#' + wf + 'PY').html(output.py); }
            });

        })
    }

    return {
        TRACEINIT: function () {
            TraceInfo();
        }
    }
}();