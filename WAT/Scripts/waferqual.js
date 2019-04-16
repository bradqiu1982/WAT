var WAFERQUAL = function () {
    var show = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months", pickerPosition: "bottom-left" });
        var wafertable = null;
        var allentable = null;

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
                            '<th>Allen-ValueCheck</th>' +
                            '<th>Allen-WAT</th>' +
                            '<th>Allen-RawData</th>' +
                            '<th>WXQUAL-Yield</th>' +
                            '<th>WXQUAL-QTY</th>' +
                            '<th>WUQUAL-Report</th>' +
                         '</tr>'
                    );

                $.each(output.waferdata, function (i, val) {
                    var allenvalcheckcell = '<td>' + val.AllenValCheck + '</td>';
                    var allenwatrescell = '<td>' + val.AllenWATResult + '</td>';
                    var allenrawdatacell = '<td></td>';

                    var reportcell = '<td></td>';
                    var yieldcell = '<td></td>';
                    var totalcell = '<td></td>';

                    if (val.AllenValCheck != '' && val.AllenValCheck != 'PASS')
                    {
                        allenvalcheckcell = '<td><font color="red">' + val.AllenValCheck + '</font></td>';
                    }

                    if (val.AllenWATResult != '' && val.AllenWATResult != 'PASS') {
                        allenwatrescell = '<td><font color="red">' + val.AllenWATResult + '</font></td>';
                    }

                    if (val.AllenValCheck != '')
                    { allenrawdatacell = '<td class="allenrawdatacla" myid= "' + val.WaferNum + '">Raw Data</td>'; }

                    if (val.WXQUALPass != val.WXQUALTotal)
                    { reportcell = '<td><button class = "btn btn-primary btn-waferreport" myid= "' + val.WaferNum + '">Report</button></td>' }
                    if (val.WXQUALYield != '')
                    { yieldcell = '<td>' + val.WXQUALYield + '%</td>'; }
                    if (val.WXQUALTotal != 0)
                    { totalcell = '<td>' + val.WXQUALTotal + '</td>'; }

                    $("#wafercontent").append(
                        '<tr>' +
                            '<td>' + val.WaferNum + '</td>' +
                            '<td>' + val.ComingDate + '</td>' +
                            '<td>' + val.PN + '</td>' +
                            '<td>' + val.VArray + '</td>' +
                            '<td>' + val.VRate + '</td>' +
                            '<td>' + val.VTech + '</td>' +
                            allenvalcheckcell +
                            allenwatrescell +
                            allenrawdatacell +
                            yieldcell +
                            totalcell +
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
                $('#wxwaferreport').modal('show');
            });
        })

        $('body').on('click', '.allenrawdatacla', function () {
            var wf = $(this).attr('myid');
            $.post('/Wafer/AllenRawData', {
                wf: wf
            }, function (output) {

                if (allentable) {
                    allentable.destroy();
                    allentable = null;
                }
                $("#allenhead").empty();
                $("#allencontent").empty();

                $("#allenhead").append(
                        '<tr style="font-size:12px;">' +
                            '<th class="smallth">Wafer</th>' +
                            '<th class="smallth">Container</th>' +
                            '<th class="smallth">X</th>' +
                            '<th class="smallth">Y</th>' +
                            '<th class="smallth">Product</th>' +
                            '<th class="smallth">Param</th>' +
                            '<th class="smallth">Val</th>' +
                            '<th class="smallth">LL</th>' +
                            '<th class="smallth">HL</th>' +
                            '<th class="smallth">RP</th>' +
                            '<th class="smallth">WorkFlow</th>' +
                            '<th class="smallth">ValueCheck</th>' +
                            '<th class="smallth">TestTime</th>' +
                         '</tr>'
                    );

                $.each(output.waferdata, function (i, val) {
                    $("#allencontent").append(
                        '<tr style="font-size:10px;">' +
                            '<td class="smalltd">'+val.WaferNum+'</td>' +
                            '<td class="smalltd">'+val.ContainerNum+'</td>' +
                            '<td class="smalltd">'+val.X+'</td>' +
                            '<td class="smalltd">'+val.Y+'</td>' +
                            '<td class="smalltd">'+val.Product+'</td>' +
                            '<td class="smalltd">'+val.Parameter+'</td>' +
                            '<td class="smalltd">'+val.TestValue+'</td>' +
                            '<td class="smalltd">'+val.LowLimit+'</td>' +
                            '<td class="smalltd">'+val.HighLimit+'</td>' +
                            '<td class="smalltd">'+val.RPNum+'</td>' +
                            '<td class="smalltd">'+val.WorkFlow+'</td>' +
                            '<td class="smalltd">'+val.ValueCheck+'</td>' +
                            '<td class="smalltd">' + val.TimeStamp + '</td>' +
                         '</tr>'
                    );
                });

                allentable = $('#allentable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                $('#allenrawdatamd').modal('show');
            });
        })


    }

    return {
        init: function () {
            show();
        }
    }
}();