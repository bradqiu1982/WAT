var WAFERQUAL = function () {
    var show = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });
        var wafertable = null;

        function searchdata() {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Wafer/WaferQUALData', {
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();

                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }
                $("#waferhead").empty();
                $("#wafercontent").empty();

                $("#waferhead").append(
                        '<tr>' +
                            '<th>Wafer</th>' +
                            '<th>Coming</th>' +
                            '<th>PN</th>' +
                            '<th>Array</th>' +
                            '<th>Rate</th>' +
                            '<th>Tech</th>' +
                            '<th>WXQUAL-Yield</th>' +
                            '<th>WXQUAL-QTY</th>' +
                            '<th>Report</th>' +
                         '</tr>'
                    );

                $.each(output.waferdata, function (i, val) {
                    var reportcell = '<td></td>';
                    if (val.WXQUALPass != val.WXQUALTotal)
                    {
                        reportcell = '<td><button class = "btn btn-primary action" myid= "' + val.WaferNum + '">Report</button></td>'
                    }

                    $("#wafercontent").append(
                        '<tr>' +
                            '<td>' + val.WaferNum + '</td>' +
                            '<td>' + val.ComingDate + '</td>' +
                            '<td>' + val.PN + '</td>' +
                            '<td>' + val.VArray + '</td>' +
                            '<td>' + val.VRate + '</td>' +
                            '<td>' + val.VTech + '</td>' +
                            '<td>' + val.WXQUALYield + '</td>' +
                            '<td>' + val.WXQUALTotal + '</td>' +
                            reportcell +
                        '</tr>'
                        );
                })

                wafertable = $('#wafertable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });
            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        $(function () {
            searchdata();
        });


        $('body').on('click', '.btn-waferreport', function () {
            var wkey = 'HTOL_'+$(this).attr('myid');
            $.post('/Wafer/WaferQUALReport', {
                wkey: wkey
            }, function (output) {
                if (output.success) {
                    $('#rc-info').html(output.report.content);
                    $('#rc-reporter').html(output.report.reporter);
                    $('#rc-datetime').html(output.report.time);
                }
                $('#boxplot-alert').modal('show');
            });
        })
    }

    return {
        init: function () {
            show();
        }
    }
}();