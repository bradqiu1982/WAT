var WATLOGIC = function () {
    var allenlogic = function ()
    {
        var logictable = null;
        var extable = null;
        var coupontable = null;
        var fmodetable = null;

        $.post('/WATLogic/GetAllenContainerList', {
        }, function (output) {
            $('#container').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.containlist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#container').attr('readonly', false);
        });

        $.post('/WATLogic/GetAllenDCDName', {
        }, function (output) {
            $('#dcdname').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.dcdnamelist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#dcdname').attr('readonly', false);
        });
        
        var allenrest = function (mode) {
            var container = $('#container').val();
            var dcdname = $('#dcdname').val();
            if (container == '' || dcdname == '')
            {
                alert('Please into correct container number and dcdname!')
            }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/VerifyAllenLogicData',
                {
                    container: container,
                    dcdname: dcdname,
                    exclusion: mode
                },
                function (output) {
                    $.bootstrapLoading.end();

                    if (logictable) {
                        logictable.destroy();
                        logictable = null;
                    }
                    $("#logichead").empty();
                    $("#logiccontent").empty();

                    $("#logichead").append(
                            '<tr>'+
                            '<th>Parameter</th>'+
                            '<th>Value</th>'+
                            '</tr>'
                        );

                    $.each(output.msglist, function (i, val) {
                        $("#logiccontent").append(
                            '<tr>' +
                            '<td>' + val.pname + '</td>' +
                            '<td>'+val.pval+'</td>' +
                            '</tr>'
                        );
                    });

                    logictable = $('#logictable').DataTable({
                        'iDisplayLength': 20,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "columnDefs": [
                            { "className": "dt-center", "targets": "_all" }
                        ],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });



                    if (extable) {
                        extable.destroy();
                        extable = null;
                    }
                    $("#exhead").empty();
                    $("#excontent").empty();
                    $("#exhead").append(
                            '<tr>' +
                            '<th>Container</th>' +
                            '<th>Unit</th>' +
                            '<th>X</th>' +
                            '<th>Y</th>' +
                            '<th>Comment</th>' +
                            '</tr>'
                        );

                    $.each(output.exclusionlist, function (i, val) {
                        $("#excontent").append(
                            '<tr>' +
                            '<td>' + val.Container + '</td>' +
                            '<td>' + val.DeviceNumber + '</td>' +
                            '<td>' + val.X + '</td>' +
                            '<td>' + val.Y + '</td>' +
                            '<td>' + val.Notes + '</td>' +
                            '</tr>'
                        );
                    });

                    extable = $('#extable').DataTable({
                        'iDisplayLength': 20,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "columnDefs": [
                            { "className": "dt-center", "targets": "_all" }
                        ],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });



                    if (coupontable) {
                        coupontable.destroy();
                        coupontable = null;
                    }
                    $("#couponhead").empty();
                    $("#couponcontent").empty();

                    $("#couponhead").append(
                            '<tr>' +
                            '<th>Eval_PN</th>' +
                            '<th>Bin_Product</th>' +
                            '<th>ParameterName</th>' +
                            '<th>UpperSpec</th>' +
                            '<th>LowerSpec</th>' +
                            '<th>Min_Value</th>' +
                            '<th>Max_Value</th>' +
                            '<th>DUTCount</th>' +
                            '<th>FailType</th>' +
                            '</tr>'
                        );

                    $.each(output.datatables[0], function (i, val) {
                        $("#couponcontent").append(
                            '<tr>' +
                            '<td>' + val.Eval_PN + '</td>' +
                            '<td>' + val.Bin_PN + '</td>' +
                            '<td>' + val.ParamName + '</td>' +
                            '<td>' + val.UpperLimit + '</td>' +
                            '<td>' + val.LowLimit + '</td>' +
                            '<td>' + val.MinVal + '</td>' +
                            '<td>' + val.MaxVal + '</td>' +
                            '<td>' + val.DUTCount + '</td>' +
                            '<td>' + val.failtype + '</td>' +
                            '</tr>'
                        );
                    });

                    coupontable = $('#coupontable').DataTable({
                        'iDisplayLength': 20,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "columnDefs": [
                            { "className": "dt-center", "targets": "_all" }
                        ],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });




                    if (fmodetable) {
                        fmodetable.destroy();
                        fmodetable = null;
                    }
                    $("#fmodehead").empty();
                    $("#fmodecontent").empty();

                    $("#fmodehead").append(
                            '<tr>' +
                            '<th>RP</th>' +
                            '<th>Unit</th>' +
                            '<th>DPO</th>' +
                            '<th>DPO_rd</th>' +
                            '<th>DIth</th>' +
                            '<th>BVR</th>' +
                            '<th>DVF</th>' +
                            '<th>Pwr</th>' +
                            '<th>DPOvsDITHcheck</th>' +
                            '<th>DPO_LL</th>' +
                            '<th>DVF_UL</th>' +
                            '<th>FailMode</th>' +
                            '</tr>'
                        );

                    $.each(output.datatables[1], function (i, val) {
                        $("#fmodecontent").append(
                            '<tr>' +
                            '<td>' + val.RP + '</td>' +
                            '<td>' + val.UnitNum + '</td>' +
                            '<td>' + val.DPO + '</td>' +
                            '<td>' + val.DPO_rd + '</td>' +
                            '<td>' + val.DIth + '</td>' +
                            '<td>' + val.BVR + '</td>' +
                            '<td>' + val.DVF + '</td>' +
                            '<td>' + val.PWR + '</td>' +
                            '<td>' + val.DPOvsDITHcheck + '</td>' +
                            '<td>' + val.DPO_LL + '</td>' +
                            '<td>' + val.DVF_UL + '</td>' +
                            '<td>' + val.Failure + '</td>' +
                            '</tr>'
                        );
                    });

                    fmodetable = $('#fmodetable').DataTable({
                        'iDisplayLength': 20,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "columnDefs": [
                            { "className": "dt-center", "targets": "_all" }
                        ],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });

                });
        }

        $('body').on('click', '#btn-withoutex', function () {
            allenrest('withoutex');
        });

        $('body').on('click', '#btn-withex', function () {
            allenrest('withex');
        });
    }

    return {
        ALLENLOGICINIT: function () {
            allenlogic();
        }
    }
}();