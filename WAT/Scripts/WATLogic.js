var WATLOGIC = function () {
    var allenlogic = function ()
    {
        var logictable = null;
        var extable = null;
        var coupontable = null;
        var fmodetable = null;
        var funittable = null;
        var rawdatatable = null;

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
        
        var allenrest = function (mode,withraw) {
            var container = $('#container').val();
            var dcdname = $('#dcdname').val();
            if (container == '' || dcdname == '')
            {
                alert('Please into correct container number and dcdname!');
                return false;
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


                    if (funittable) {
                        funittable.destroy();
                        funittable = null;
                    }
                    $("#funithead").empty();
                    $("#funitcontent").empty();

                    $("#funithead").append(
                            '<tr>' +
                            '<th>Eval_PN</th>' +
                            '<th>Bin_PN</th>' +
                            '<th>UnitNum</th>' +
                            '<th>ParamName</th>' +
                            '<th>Value</th>' +
                            '<th>LowLimit</th>' +
                            '<th>HighLimit</th>' +
                            '<th>FailType</th>' +
                            '<th>RP</th>' +
                            '<th>X</th>' +
                            '<th>Y</th>' +
                            '</tr>'
                        );

                    $.each(output.datatables[2], function (i, val) {
                        $("#funitcontent").append(
                            '<tr>' +
                            '<td>' + val.Eval_PN + '</td>' +
                            '<td>' + val.Bin_PN + '</td>' +
                            '<td>' + val.UnitNum + '</td>' +
                            '<td>' + val.ParamName + '</td>' +
                            '<td>' + val.TVAL + '</td>' +
                            '<td>' + val.LowLimit + '</td>' +
                            '<td>' + val.UpperLimit + '</td>' +
                            '<td>' + val.FailType + '</td>' +
                            '<td>' + val.RP + '</td>' +
                            '<td>' + val.X + '</td>' +
                            '<td>' + val.Y + '</td>' +
                            '</tr>'
                        );
                    });

                    funittable = $('#funittable').DataTable({
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


                    if (rawdatatable) {
                        rawdatatable.destroy();
                        rawdatatable = null;
                    }
                    $("#rawdatahead").empty();
                    $("#rawdatacontent").empty();

                    if (withraw)
                    {
                        $("#rawdatahead").append(
                                '<tr>' +
                                '<th>ContainerNum</th>' +
                                '<th>RP</th>' +
                                '<th>UnitNum</th>' +
                                '<th>CommonTestName</th>' +
                                '<th>TestValue</th>' +
                                '<th>ProbeValue</th>' +
                                '<th>X</th>' +
                                '<th>Y</th>' +
                                '</tr>'
                            );

                        $.each(output.datatables[3], function (i, val) {
                            $("#rawdatacontent").append(
                                '<tr>' +
                                '<td>' + val.ContainerNum + '</td>' +
                                '<td>' + val.RP + '</td>' +
                                '<td>' + val.UnitNum + '</td>' +
                                '<td>' + val.CommonTestName + '</td>' +
                                '<td>' + val.TestValue + '</td>' +
                                '<td>' + val.ProbeValue + '</td>' +
                                '<td>' + val.X + '</td>' +
                                '<td>' + val.Y + '</td>' +
                                '</tr>'
                            );
                        });

                        rawdatatable = $('#rawdatatable').DataTable({
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
                    }

                });
        }

        $('body').on('click', '#btn-withoutex', function () {
            allenrest('withoutex',false);
        });

        $('body').on('click', '#btn-withex', function () {
            allenrest('withex',false);
        });

        $('body').on('click', '#btn-withraw', function () {
            allenrest('withex',true);
        });
    }

    var allenwat = function () {
        var logictable = null;

        $.post('/WATLogic/GetAllen2WXWaferList', {
        }, function (output) {
            $('#wafernum').autoComplete({
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
            $('#wafernum').attr('readonly', false);
        });

        var allenwatrest = function ()
        {
            var wafernum = $('#wafernum').val();
            if (wafernum == '' || wafernum.indexOf('-') == -1)
            {
                alert('Please into correct wafernum number!');
                return false;
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

            $.post('/WATLogic/AllenWaferWATData',
                {
                    wafernum: wafernum
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
                            '<tr>' +
                            '<th>Container</th>' +
                            '<th>RP</th>' +
                            '<th>Parameter</th>' +
                            '<th>Value</th>' +
                            '</tr>'
                        );

                    $.each(output.msglist, function (i, val) {
                        var valtd = '<td>' + val.pval + '</td>';
                        if (val.pname.indexOf('WAT Result') != -1
                            && val.pval != ''
                            && val.pval.indexOf('Proceed') == -1
                            && val.pval.indexOf('Not Production WAT') == -1)
                        {
                            valtd = '<td style="background-color:orangered">' + val.pval + '</td>';
                        }

                        if (val.pname.indexOf('App Exception') != -1)
                        {
                            valtd = '<td style="background-color:orangered">' + val.pval + '</td>';
                        }

                        $("#logiccontent").append(
                            '<tr>' +
                            '<td>' + val.container + '</td>' +
                            '<td>' + val.rp + '</td>' +
                            '<td>' + val.pname + '</td>' +
                             valtd +
                            '</tr>'
                        );
                    });

                    logictable = $('#logictable').DataTable({
                        'iDisplayLength': -1,
                        'aLengthMenu': [[30, 60, 100, -1],
                        [30, 60, 100, "All"]],
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


        $('body').on('click', '#btn-search', function () {
            allenwatrest();
        });
    }

    var wuxilogic = function () {

        var logictable = null;
        var coupontable = null;
        var fmodetable = null;

        $.post('/WATLogic/GetWXCouponID', {
        }, function (output) {
            $('#couponid').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.couponidlist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#couponid').attr('readonly', false);
        });

        $.post('/WATLogic/GetWXJudgementStep', {
        }, function (output) {
            $('#jstepname').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.steplist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#jstepname').attr('readonly', false);
        });

        var wuxirest = function () {
            var couponid = $('#couponid').val();
            var jstepname = $('#jstepname').val();
            if (couponid == '' || jstepname == '') {
                alert('Please into correct coupon id and judgement step name!')
                return false;
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

            $.post('/WATLogic/WUXIWATLogicData',
                {
                    couponid: couponid,
                    jstepname: jstepname
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
                            '<tr>' +
                            '<th>Parameter</th>' +
                            '<th>Value</th>' +
                            '</tr>'
                        );

                    $.each(output.msglist, function (i, val) {
                        $("#logiccontent").append(
                            '<tr>' +
                            '<td>' + val.pname + '</td>' +
                            '<td>' + val.pval + '</td>' +
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

        $('body').on('click', '#btn-run', function () {
            wuxirest();
        });


    }

    var wuxidatamg = function () {
        var watdatatable = null;

        $.post('/WATLogic/GetWXCouponID', {
        }, function (output) {
            $('#couponid').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.couponidlist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#couponid').attr('readonly', false);
        });

        $('body').on('click', '#btn-cmf', function () {
            $('#ignoredlg').modal('hide');

            var reason = $('#ignorereason').val();
            if (reason == '' || reason == null)
            {
                alert('choose an ignore reason!');
                return false;
            }

            var ignoredies = '';
            $('.diecheck').each(function (i, obj) {
                if ($(this).prop('checked') == true) {
                    $(this).parent().parent().addClass("COMMENTLINE");
                    ignoredies += $(this).attr('myid') + ':::';
                }
            });

            $.post('/WATLogic/IgnoreWATDie', {
                ignoredies: ignoredies,
                reason: reason
                    }, function (output) {

                    });

        });

        $.fn.dataTable.ext.buttons.ignoredie = {
            text: 'Ignore Die',
            action: function (e, dt, node, config) {

                var ignoredies = '';
                $('.diecheck').each(function (i, obj) {
                    if ($(this).prop('checked') == true)
                    {
                        ignoredies += $(this).attr('myid') + ':::';
                    }
                });

                if (ignoredies != '')
                {
                    $('#ignoredlg').modal('show');
                }
            }
        };

        function reviewdata()
        {
            var couponid = $('#couponid').val();
            if (couponid == '') {
                alert('Please into correct coupon id and judgement step name!')
                return false;
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

            $.post('/WATLogic/WUXIWATDataMG',
                {
                    couponid: couponid
                },
                function (output) {
                    $.bootstrapLoading.end();

                    if (watdatatable) {
                        watdatatable.destroy();
                        watdatatable = null;
                    }
                    $("#watdatahead").empty();
                    $("#watdatacontent").empty();

                    $("#watdatahead").append(
                            '<tr>' +
                            '<th></th>' +
                            '<th>CouponID</th>' +
                            '<th>CH</th>' +
                            '<th>X</th>' +
                            '<th>Y</th>' +
                            '<th>Test Step</th>' +
                            '<th>BVR_LD_A</th>' +
                            '<th>PO_LD_W</th>' +
                            '<th>VF_LD_V</th>' +
                            '<th>SLOPE_WperA</th>' +
                            '<th>THOLD_A</th>' +
                            '<th>R_LD_ohm</th>' +
                            '<th>IMAX_A</th>' +
                            '<th>Ith</th>' +
                            '<th>SlopEff</th>' +
                            '<th>SeriesR</th>' +
                            '</tr>'
                        );

                    $.each(output.mgdata, function (i, val) {
                        var tempstr = '';
                        if (val.IgnoredFlag != '') {
                            tempstr += '<tr class="COMMENTLINE" data-toggle="tooltip" title="Ignore For ' + val.Comment + '">';
                        }
                        else {
                            tempstr += '<tr>';
                        }

                        tempstr += '<td><input type="checkbox" class="diecheck" myid="' + val.CouponID + '_' + val.X + '_' + val.Y + '"></td>' +
                            '<td>' + val.CouponID + '</td>' +
                            '<td>' + val.CH + '</td>' +
                            '<td>' + val.X + '</td>' +
                            '<td>' + val.Y + '</td>' +
                            '<td>' + val.TestStep + '</td>' +
                            '<td class="' + val.BVR_LD_A_ST + '">' + val.BVR_LD_A + '</td>' +
                            '<td class="' + val.PO_LD_W_ST + '">' + val.PO_LD_W + '</td>' +
                            '<td class="' + val.VF_LD_V_ST + '">' + val.VF_LD_V + '</td>' +
                            '<td class="' + val.SLOPE_WperA_ST + '">' + val.SLOPE_WperA + '</td>' +
                            '<td class="' + val.THOLD_A_ST + '">' + val.THOLD_A + '</td>' +
                            '<td class="' + val.R_LD_ohm_ST + '">' + val.R_LD_ohm + '</td>' +
                            '<td class="' + val.IMAX_A_ST + '">' + val.IMAX_A + '</td>' +
                            '<td>' + val.Ith + '</td>' +
                            '<td>' + val.SlopEff + '</td>' +
                            '<td>' + val.SeriesR + '</td>' +
                            '</tr>';

                        $("#watdatacontent").append(tempstr);
                    });


                    watdatatable = $('#watdatatable').DataTable({
                        'iDisplayLength': 20,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "columnDefs": [
                            { "className": "dt-center", "targets": "_all" }
                        ],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5', 'ignoredie']
                    });


                });
        }

        $('body').on('click', '#btn-review', function () {
            reviewdata();
        });

    }

    var watogp = function () {
        var logictable = null;

        $.post('/WATLogic/LoadOGPWafer', {
        }, function (output) {
            $('#wafernum').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.waferlist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#wafernum').attr('readonly', false);
        });

        var ogprest = function () {
            var wafernum = $('#wafernum').val();
            if (wafernum == '' || wafernum.indexOf('-') == -1) {
                alert('Please into correct wafernum number!');
                return false;
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

            $.post('/WATLogic/LoadOGPData',
                {
                    wafer: wafernum
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
                            '<tr>' +
                            '<th>CouponID</th>' +
                            '<th>ChannelInfo</th>' +
                            '<th>X</th>' +
                            '<th>Y</th>' +
                            '</tr>'
                        );

                    $.each(output.ogpdatalist, function (i, val) {
                        $("#logiccontent").append(
                            '<tr>' +
                            '<td>' + val.CouponID + '</td>' +
                            '<td>' + val.ChannelInfo + '</td>' +
                            '<td>' + val.X+ '</td>' +
                             '<td>' + val.Y + '</td>' +
                            '</tr>'
                        );
                    });

                    logictable = $('#logictable').DataTable({
                        'iDisplayLength': -1,
                        'aLengthMenu': [[30, 60, 100, -1],
                        [30, 60, 100, "All"]],
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


        $('body').on('click', '#btn-search', function () {
            ogprest();
        });
    }

    var wuxiwat = function ()
    {
        var logictable = null;

        $.post('/WATLogic/LoadOGPWafer', {
        }, function (output) {
            $('#wafernum').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.waferlist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#wafernum').attr('readonly', false);
        });

        var wuxiwaferwat = function () {
            var wafernum = $('#wafernum').val();
            if (wafernum == '' || wafernum.indexOf('-') == -1) {
                alert('Please into correct wafernum number!');
                return false;
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

            $.post('/WATLogic/WUXIWaferWATData',
                {
                    wafer: wafernum
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
                            '<tr>' +
                            '<th>Container</th>' +
                            '<th>Test</th>' +
                            '<th>Result</th>' +
                            '<th>Test Times</th>' +
                            '<th>Failure Count</th>' +
                            '<th>Failure Mode</th>' +
                            '<th>Failed Param</th>' +
                            '</tr>'
                        );

                    $.each(output.reslist, function (i, val) {
                        $("#logiccontent").append(
                            '<tr>' +
                            '<td>' + val.coupongroup + '</td>' +
                            '<td>' + val.teststep + '</td>' +
                            '<td>' + val.result + '</td>' +
                             '<td>' + val.testtimes + '</td>' +
                             '<td>' + val.failcount + '</td>' +
                             '<td>' + val.failmode + '</td>' +
                             '<td>' + val.couponstr + '</td>' +
                            '</tr>'
                        );
                    });

                    logictable = $('#logictable').DataTable({
                        'iDisplayLength': -1,
                        'aLengthMenu': [[30, 60, 100, -1],
                        [30, 60, 100, "All"]],
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


        $('body').on('click', '#btn-search', function () {
            wuxiwaferwat();
        });
    }

    var wuxiwatanalyze = function () {

        $.post('/WATLogic/WATAnalyzeParams', {
        }, function (output) {
            $('#param').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.paramlist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#param').attr('readonly', false);
        });

        $.post('/WATLogic/LoadOGPWafer', {}, function (output) {
            $('#wafers').tagsinput({
                freeInput: false,
                typeahead: {
                    source: output.waferlist,
                    minLength: 0,
                    showHintOnFocus: true,
                    autoSelect: false,
                    selectOnBlur: false,
                    changeInputOnSelect: false,
                    changeInputOnMove: false,
                    afterSelect: function (val) {
                        this.$element.val("");
                    }
                }
            });

            $('#wafers').attr('readonly', false);
        });



        function analyzedata(act)
        {
            var param = $('#param').val();
            var wafers = $.trim($('#wafers').tagsinput('items'));
            if (wafers == '') {
                wafers = $.trim($('#wafers').parent().find('input').eq(0).val());
            }
            var rp = $('#rplist').val();
            
            if (param == '' || wafers == '' || rp == '')
            {
                alert('Please input your query condition!');
                return false;
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

            $.post('/WATLogic/WUXIWATAnalyzeData', {
                param: param,
                wafers: wafers,
                rp: rp,
                act: act
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();
                    var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                               '<div class="v-box" id="' + output.boxdata.id + '"></div>' +
                               '</div><div class="col-xs-1"></div></div>';
                    $('#chartdiv').append(appendstr);
                    drawboxplot(output.boxdata);
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-distrib', function () {
            analyzedata('distrib');
        });

        $('body').on('click', '#btn-cmp', function () {
            analyzedata('cmp');
        });

        $('body').on('click', '#btn-sample', function () {
            analyzedata('sample');
        });

        var drawboxplot = function (boxplot_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'boxplot'
                },

                title: {
                    text: boxplot_data.title
                },

                legend: {
                    enabled: false
                },

                xAxis: {
                    categories: boxplot_data.xAxis.data,
                    title: {
                        text: boxplot_data.xAxis.title
                    }
                },

                yAxis: {
                    title: {
                        text: boxplot_data.yAxis.title
                    },
                    plotLines: [{
                        value: boxplot_data.lowlimit,
                        color: 'green',
                        dashStyle: 'Dash',
                        width: 2,
                        label: {
                            text: 'LL',
                            align: 'left'
                        }
                    }, {
                        value: boxplot_data.highlimit,
                        color: 'green',
                        dashStyle: 'Dash',
                        width: 2,
                        label: {
                            text: 'UL',
                            align: 'left'
                        }
                    }]
                },
                annotations: [{
                    labels: boxplot_data.labels,
                    color: '#d4d4d4',
                    draggable: 'xy'
                }],
                series: [{
                    name: boxplot_data.data.name,
                    data: boxplot_data.data.data,
                    tooltip: {
                        headerFormat: '<em>{point.key}</em><br/>'
                    },
                    turboThreshold: 100000
                },
                {
                    name: 'Outlier',
                    type: 'scatter',
                    data: boxplot_data.outlierdata,
                    marker: {
                        lineWidth: 1,
                        radius: 2.5
                    },
                    tooltip: {
                        headerFormat: '',
                        pointFormat: "{point.y}"
                    },
                    turboThreshold:100000
                }],
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + boxplot_data.id).parent().toggleClass('chart-modal');
                                $('#' + boxplot_data.id).highcharts().reflow();
                            },
                            text: 'Full Screen'
                        },
                        datalabel: {
                            onclick: function () {
                                var labelflag = !this.series[0].options.dataLabels.enabled;
                                $.each(this.series, function (idx, val) {
                                    var opt = val.options;
                                    opt.dataLabels.enabled = labelflag;
                                    val.update(opt);
                                })
                            },
                            text: 'Data Label'
                        },
                        copycharts: {
                            onclick: function () {
                                var svg = this.getSVG({
                                    chart: {
                                        width: this.chartWidth,
                                        height: this.chartHeight
                                    }
                                });
                                var c = document.createElement('canvas');
                                c.width = this.chartWidth;
                                c.height = this.chartHeight;
                                canvg(c, svg);
                                var dataURL = c.toDataURL("image/png");
                                //var imgtag = '<img src="' + dataURL + '"/>';

                                var img = new Image();
                                img.src = dataURL;

                                copyImgToClipboard(img);
                            },
                            text: 'copy 2 clipboard'
                        }
                    },
                    buttons: {
                        contextButton: {
                            menuItems: ['fullscreen', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                        }
                    }
                }
            };
            Highcharts.chart(boxplot_data.id, options);
        }
    }

    return {
        ALLENLOGICINIT: function () {
            allenlogic();
        },
        ALLENWATINIT: function () {
            allenwat();
        },
        WUXILOGICINIT: function () {
            wuxilogic();
        },
        WUXIWATDATAMG: function ()
        {
            wuxidatamg();
        },
        OGPINIT: function () {
            watogp();
        },
        WUXIWATINIT: function ()
        {
            wuxiwat();
        },
        WUXIWATANALYZE : function(){
            wuxiwatanalyze();
        }
    }
}();