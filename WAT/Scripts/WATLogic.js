﻿var WATLOGIC = function () {
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

        var wuxirest = function (couponid, jstepname,r100) {

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
                    jstepname: jstepname,
                    r100:r100
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
                            '<th>LowerSpec</th>' +
                            '<th>UpperSpec</th>' +
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
                            '<td>' + val.LowLimit + '</td>' +
                            '<td>' + val.UpperLimit + '</td>' +
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
                            '<th>X</th>' +
                            '<th>Y</th>' +
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
                            '<td>' + val.X + '</td>' +
                            '<td>' + val.Y + '</td>' +
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
            var couponid = $('#couponid').val();
            var jstepname = $('#jstepname').val();
            if (couponid == '' || jstepname == '') {
                alert('Please into correct coupon id and judgement step name!')
                return false;
            }

            wuxirest(couponid, jstepname,'FALSE');
        });

        $('body').on('click', '#btn-r100', function () {
            //var couponid = $('#couponid').val();
            //var jstepname = $('#jstepname').val();
            //if (couponid == '' || jstepname == '') {
            //    alert('Please into correct coupon id and judgement step name!')
            //    return false;
            //}

            //wuxirest(couponid, jstepname, 'TRUE');
        });

        $(function () {
            var couponid = $('#hcouponid').val();
            var jstepname = $('#hjstepname').val();
            if (couponid == '' || jstepname == '') {
                return false;
            }

            wuxirest(couponid, jstepname,'FALSE');
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

       $.fn.dataTable.ext.buttons.uploadpicture = {
            text: 'Upload Ignore Picture',
            action: function (e, dt, node, config) {
                $('#file1').click();
            }
        };

       $('body').on('change', '#file1', function () {
           var ignoredies = '';
           $('.diecheck').each(function (i, obj) {
               if ($(this).prop('checked') == true) {
                   ignoredies = $(this).attr('myid');
               }
           });

           if (ignoredies != null)
           {
                $.ajaxFileUpload({
                            url: '/WATLogic/IgnoreWATDiePicture',
                            secureuri: false,
                            data: { 'igid': ignoredies },
                            fileElementId: 'file1',
                            dataType: 'json',
                            success: function (data, status) {
                                window.location.reload(true);
                            },
                            error: function (e) {
                                //alert(e);
                            }
                        });
           }
        })

        function reviewdata(couponid)
        {
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
                            '<th>TestTimeStamp</th>' +
                            '</tr>'
                        );

                    $.each(output.mgdata, function (i, val) {
                        var tempstr = '';
                        if (val.IgnoredFlag != '' && val.Atta != '') {
                            tempstr += '<tr class="COMMENTLINE WITHATTA" data-toggle="tooltip" myatta="' + val.Atta + '" title="Ignore For ' + val.Comment + '">';
                        }
                        else if (val.IgnoredFlag != '') {
                            tempstr += '<tr class="COMMENTLINE" data-toggle="tooltip" title="Ignore For ' + val.Comment + '">';
                        }
                        else {
                            tempstr += '<tr>';
                        }

                        tempstr += '<td><input type="checkbox" class="diecheck" myid="' + val.CouponID + '_' + val.X + '_' + val.Y + '_' + val.CH + '"></td>' +
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
                            '<td>' + val.TestTime + '</td>' +
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
                        buttons: ['copyHtml5', 'csv', 'excelHtml5', 'ignoredie', 'uploadpicture']
                    });


                });
        }

        $('body').on('click', '#btn-review', function () {
            var couponid = $('#couponid').val();
            if (couponid == '') {
                alert('Please into correct coupon id and judgement step name!')
                return false;
            }
            reviewdata(couponid);
        });

        $('body').on('click', '.WITHATTA', function () {
            window.open($(this).attr('myatta'), '_blank');
        });

        $(function () {
            var wf = $('#hcouponid').val();
            if (wf != '')
            { reviewdata(wf); }
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
                    var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10">' +
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
                    turboThreshold: 500000
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
                    turboThreshold:500000
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

    var wuxiwatxyfun = function ()
    {
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

        function watxy(param, wafernum, rp) {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATXYDATA', {
                param: param,
                wafernum: wafernum,
                rp: rp
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();
                    var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10">' +
                               '<div class="v-box" id="' + output.xydata.id + '"></div>' +
                               '</div><div class="col-xs-1"></div></div>';
                    $('#chartdiv').append(appendstr);
                    drawdiesortmap(output.xydata);
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var param = $('#param').val();
            var wafernum = $('#wafernum').val();
            var rp = $('#rplist').val();

            if (param == '' || wafernum == '' || rp == '') {
                alert('Please input your query condition!');
                return false;
            }
            watxy(param,wafernum,rp);
        });

        $(function () {
            var param = $('#hparam').val();
            var wafernum = $('#hwafer').val();
            var rp = $('#hrp').val();

            if (param == '' || wafernum == '' || rp == '') {
                return false;
            }
            watxy(param, wafernum, rp);
        });

        var drawdiesortmap = function (line_data) {
            var options = {
                title: {
                    text: line_data.title
                },
                chart: {
                    type: 'heatmap',
                    zoomType: 'xy'
                },
                boost: {
                    useGPUTranslations: true,
                    usePreallocated: true,
                    seriesThreshold: 1
                },
                xAxis: {
                    title: {
                        text: 'X'
                    },
                    max: line_data.xmax
                },
                yAxis: {
                    title: {
                        text: 'Y'
                    },
                    max: line_data.ymax,
                    reversed: true
                },
                colorAxis: {
                    min: line_data.datamin,
                    max: line_data.datamax,
                    stops: line_data.stops
                },
                series: line_data.serial,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + line_data.id).parent().toggleClass('chart-modal');
                                $('#' + line_data.id).highcharts().reflow();
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
            Highcharts.chart(line_data.id, options);

        }
    }


    
    var allenwatxyfun = function ()
    {
        function allenwatxy(wafernum) {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/ALLENWATXYDATA', {
                wafernum: wafernum
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();
                    var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10">' +
                               '<div class="v-box" id="' + output.xydata.id + '"></div>' +
                               '</div><div class="col-xs-1"></div></div>';
                    $('#chartdiv').append(appendstr);
                    drawdiesortmap(output.xydata);
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var wafernum = $('#wafernum').val();

            if (wafernum == '') {
                alert('Please input your query condition!');
                return false;
            }
            allenwatxy( wafernum);
        });

        var drawdiesortmap = function (line_data) {
            var options = {
                title: {
                    text: line_data.title
                },
                chart: {
                    type: 'heatmap',
                    zoomType: 'xy'
                },
                boost: {
                    useGPUTranslations: true,
                    usePreallocated: true,
                    seriesThreshold: 1
                },
                xAxis: {
                    title: {
                        text: 'X'
                    },
                    max: line_data.xmax
                },
                yAxis: {
                    title: {
                        text: 'Y'
                    },
                    max: line_data.ymax,
                    reversed: true
                },
                colorAxis: {
                    min: line_data.datamin,
                    max: line_data.datamax,
                    stops: [
                    [0, '#c8c8c8'],
                    [0.1, '#0000ff'],
                    [0.2, '#0080ff'],
                    [0.3, '#00ffff'],
                    [0.4, '#00ff80'],
                    [0.5, '#00ff00'],
                    [0.6, '#80ff00'],
                    [0.7, '#ffff00'],
                    [0.8, '#ff8000'],
                    [0.9, '#ff0000']
                    ]
                },
                series: line_data.serial,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + line_data.id).parent().toggleClass('chart-modal');
                                $('#' + line_data.id).highcharts().reflow();
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
            Highcharts.chart(line_data.id, options);

        }
    }


    var wuxiwatdisfun = function ()
    {
        $.post('/WATLogic/LoadLocalOGPWafer', {
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


        function wuxiwatdis(wafernum) {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATDistributionDATA', {
                wafernum: wafernum
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();
                    var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10">' +
                               '<div class="v-box" id="' + output.xydata.id + '"></div>' +
                               '</div><div class="col-xs-1"></div></div>';
                    $('#chartdiv').append(appendstr);
                    drawdiesortmap(output.xydata);
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var wafernum = $('#wafernum').val();

            if (wafernum == '') {
                alert('Please input your query condition!');
                return false;
            }
            wuxiwatdis(wafernum);
        });

        var drawdiesortmap = function (line_data) {
            var options = {
                title: {
                    text: line_data.title
                },
                chart: {
                    type: 'heatmap',
                    zoomType: 'xy'
                },
                boost: {
                    useGPUTranslations: true,
                    usePreallocated: true,
                    seriesThreshold: 1
                },
                xAxis: {
                    title: {
                        text: 'X'
                    },
                    max: line_data.xmax
                },
                yAxis: {
                    title: {
                        text: 'Y'
                    },
                    max: line_data.ymax,
                    reversed: true
                },
                colorAxis: {
                    min: line_data.datamin,
                    max: line_data.datamax,
                    stops: [
                    [0, '#c8c8c8'],
                    [0.1, '#0000ff'],
                    [0.2, '#0080ff'],
                    [0.3, '#00ffff'],
                    [0.4, '#00ff80'],
                    [0.5, '#00ff00'],
                    [0.6, '#80ff00'],
                    [0.7, '#ffff00'],
                    [0.8, '#ff8000'],
                    [0.9, '#ff0000']
                    ]
                },
                series: line_data.serial,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + line_data.id).parent().toggleClass('chart-modal');
                                $('#' + line_data.id).highcharts().reflow();
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
            Highcharts.chart(line_data.id, options);

        }
    }

    var wuxiwatcouponfun = function () {


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



        function coupondatas(param, wafers, rp) {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATCouponData', {
                param: param,
                wafers: wafers,
                rp: rp
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();
                    var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-12">' +
                               '<div class="v-box" id="' + output.coupondata.id + '"></div>' +
                               '</div></div>';
                    $('#chartdiv').append(appendstr);

                    drawcouponplot(output.coupondata);
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var param = $('#param').val();
            var wafers = $.trim($('#wafers').tagsinput('items'));
            if (wafers == '') {
                wafers = $.trim($('#wafers').parent().find('input').eq(0).val());
            }
            var rp = $('#rplist').val();

            if (param == '' || wafers == '' || rp == '') {
                alert('Please input your query condition!');
                return false;
            }

            coupondatas(param, wafers,rp);
        });

        $(function () {
            var param = $('#hparam').val();
            var wafernum = $('#hwafer').val();
            var rp = $('#hrp').val();

            if (param == '' || wafernum == '' || rp == '') {
                return false;
            }

            coupondatas(param, wafernum, rp);
        });

        var drawcouponplot = function (boxplot_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'scatter'
                },

                title: {
                    text: boxplot_data.title
                },

                legend: {
                    enabled: false
                },

                xAxis: {
                    categories: boxplot_data.categories
                },

                yAxis: {
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
                series: [
                {
                    type: 'scatter',
                    data: boxplot_data.datalist,
                    marker: {
                        lineWidth: 1,
                        radius: 2.5
                    },
                    tooltip: {
                        headerFormat: '',
                        pointFormat: "{point.y}"
                    },
                    turboThreshold: 500000
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

    var wuxiwatpvpfun = function () {

        $.post('/WATLogic/WATAnalyzeParams', {
        }, function (output) {
            $('#xparam').autoComplete({
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
            $('#xparam').attr('readonly', false);
        });

        $.post('/WATLogic/WATAnalyzeParams', {
        }, function (output) {
            $('#yparam').autoComplete({
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
            $('#yparam').attr('readonly', false);
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

        function pvpdatas(xparam, yparam, wafers, rp) {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATPvsPData', {
                xparam: xparam,
                yparam: yparam,
                wafers: wafers,
                rp: rp
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();
                    var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-12">' +
                               '<div class="v-box" id="' + output.pvpdata.id + '"></div>' +
                               '</div></div>';
                    $('#chartdiv').append(appendstr);

                    drawpvpplot(output.pvpdata);
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var xparam = $('#xparam').val();
            var yparam = $('#yparam').val();

            var wafers = $.trim($('#wafers').tagsinput('items'));
            if (wafers == '') {
                wafers = $.trim($('#wafers').parent().find('input').eq(0).val());
            }

            var rp = $('#rplist').val();

            if (xparam == '' || yparam == '' || wafers == '' || rp == '') {
                alert('Please input your query condition!');
                return false;
            }

            pvpdatas(xparam, yparam, wafers,rp);
        });

        $(function () {
            var xparam = $('#hxparam').val();
            var yparam = $('#hyparam').val();
            var wafernum = $('#hwafer').val();
            var rp = $('#hrp').val();

            if (xparam == '' || yparam == '' || wafernum == '' || rp == '') {
                return false;
            }

            pvpdatas(xparam, yparam, wafernum, rp);
        });

        var drawpvpplot = function (boxplot_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'scatter'
                },

                title: {
                    text: boxplot_data.title
                },
                legend: {
                    enabled: true
                },
                xAxis: {
                    title: {
                        text: boxplot_data.xtitle
                    },
                    plotLines: [{
                        value: boxplot_data.xlowlimit,
                        color: 'green',
                        dashStyle: 'Dash',
                        width: 2,
                        label: {
                            text: 'X-LL',
                            align: 'left'
                        }
                    }, {
                        value: boxplot_data.xhighlimit,
                        color: 'green',
                        dashStyle: 'Dash',
                        width: 2,
                        label: {
                            text: 'X-UL',
                            align: 'left'
                        }
                    }]
                },

                yAxis: {
                    title: {
                        text: boxplot_data.ytitle
                    },
                    plotLines: [{
                        value: boxplot_data.ylowlimit,
                        color: 'green',
                        dashStyle: 'Dash',
                        width: 2,
                        label: {
                            text: 'Y-LL',
                            align: 'left'
                        }
                    }, {
                        value: boxplot_data.yhighlimit,
                        color: 'green',
                        dashStyle: 'Dash',
                        width: 2,
                        label: {
                            text: 'Y-UL',
                            align: 'left'
                        }
                    }]
                },
                annotations: [{
                    labels: boxplot_data.labels,
                    color: '#d4d4d4',
                    draggable: 'xy'
                }],
                series: boxplot_data.series,
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

    var wuxiwatstatusfun = function () {
        var logictable = null;

        $.fn.dataTable.ext.buttons.refreshstatus = {
            text: 'Refresh',
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

                $.post('/MGTL/RefreshWATWIP',
                    {}, function (output) {
                        $.bootstrapLoading.end();

                        window.location.reload(true);
                    });
            }
        };

        $.fn.dataTable.ext.buttons.refreshtoday = {
            text: 'Today',
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

                $.post('/MGTL/RefreshWATWIPToday',
                    {}, function (output) {
                        $.bootstrapLoading.end();

                        window.location.reload(true);
                    });
            }
        };

        var wuxistat = function () {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/MGTL/WUXIWATStatusData',
                { },
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
                            '<th>Prod</th>' +
                            '<th>Array</th>' +
                            '<th>TestTime</th>' +
                            '<th>TestStep</th>' +
                            '<th>Result</th>' +
                            '<th>DVF</th>' +
                            '<th>WOT</th>' +
                            '<th>DIS</th>' +
                            '<th>LPW</th>' +
                            '<th>DLA</th>' +
                            '<th>RAW</th>' +
                            '<th>E10</th>' +
                            '<th>Logic</th>' +
                            '<th>PWROnCP</th>' +
                            '<th>DVFOnCP</th>' +
                            '<th>Assembly</th>' +
                            '<th>PWRXY</th>' +
                            '<th>DVFXY</th>' +
                            '<th>DITHvsDPO</th>' +
                            '</tr>'
                        );
                    
                    $.each(output.wipdata, function (i, val) {
                        var rawdatalink = '<td><a href="/WATLogic/WUXIWATDataManage?wafer=' + val.CouponID + '" target="_blank" >RAW</a></td>';
                        var e10link = '<td><a href="/WATLogic/WUXIWATDataManage?wafer=' + val.CouponID.substr(0,val.CouponID.length-2) + '10" target="_blank" >E10</a></td>';
                        var logiclink = '<td></td>';
                        var poweroncoupon = '<td></td>';
                        var dvfoncoupon = '<td></td>';
                        var poweronwafer = '<td></td>';
                        var dvfonwafer = '<td></td>';
                        var powervsdith = '<td></td>';

                        var DVFtd = '<td>' + val.DVF + '</td>';
                        if (val.DVF == 0 || val.DVF == 1)
                        { DVFtd = '<td style="color:orange;">' + val.DVF + '</td>'; }

                        var assemblylink = '<td><a href="/WATLogic/WUXIWATCoupon?param=Assembly&wafer=' + val.CouponID + '&rp=RP00" target="_blank" >Assembly</a></td>';

                        if (val.RPStr != 'RP00' && val.ReTest.indexOf('productname') == -1 && val.ReTest.indexOf('coupon count') == -1 && val.ReTest.indexOf('Fail to') == -1)
                        {
                            poweroncoupon = '<td><a href="/WATLogic/WUXIWATCoupon?param=PO_LD_W&wafer=' + val.CouponID + '&rp=' + val.RPStr + '" target="_blank" >PWROnCP</a></td>';
                            dvfoncoupon = '<td><a href="/WATLogic/WUXIWATCoupon?param=Dvf&wafer=' + val.CouponID + '&rp=' + val.RPStr + '" target="_blank" >DVFOnCP</a></td>';
                            poweronwafer = '<td><a href="/WATLogic/WUXIWATXY?param=PO_LD_W&wafer=' + val.CouponID + '&rp=' + val.RPStr + '" target="_blank" >PWRXY</a></td>';
                            dvfonwafer = '<td><a href="/WATLogic/WUXIWATXY?param=Dvf&wafer=' + val.CouponID + '&rp=' + val.RPStr + '" target="_blank" >DVFXY</a></td>';
                            powervsdith = '<td><a href="/WATLogic/WUXIWATPvsP?xparam=DIth&yparam=DPO&wafer=' + val.CouponID + '&rp=' + val.RPStr + '" target="_blank" >DITHvsDPO</a></td>';
                        }

                        if (val.RPStr != 'RP00' && val.ReTest.indexOf('productname') == -1) {
                            logiclink = '<td><a href="/WATLogic/WUXIWATLogic?wafer=' + val.CouponID + '&rp=' + val.RPStr + '" target="_blank" >Logic</a></td>';
                        }

                        $("#logiccontent").append(
                            '<tr>' +
                            '<td class="watanalyzebtn ' + val.HasComment + '" id="' + val.CouponID + '" myid="' + val.CouponID + '">' + val.CouponID + '</td>' +
                            '<td>' + val.VType + '</td>' +
                            '<td>' + val.VArray + '</td>' +
                            '<td>' + val.TestTime + '</td>' +
                            '<td>' + val.TestStep.replace('PRLL_', '').replace('VCSEL_', '') + '</td>' +
                             '<td>' + val.ReTest + '</td>' +
                             DVFtd +
                             '<td>' + val.WOT + '</td>' +
                             '<td>' + val.DIS + '</td>' +
                             '<td>' + val.LPW + '</td>' +
                             '<td>' + val.DLA + '</td>' +

                             rawdatalink +
                             e10link +
                             logiclink +
                             poweroncoupon +
                             dvfoncoupon +
                             assemblylink +
                             poweronwafer +
                             dvfonwafer +
                             powervsdith +
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
                        buttons: ['copyHtml5', 'csv', 'excelHtml5', 'refreshstatus', 'refreshtoday']
                    });
                });
        }


        $(function(){
            wuxistat();
        });

        $('body').on('click', '.FAILUREDETIAL', function () {
            var detailinfo = $(this).attr('detailinfo');
            if (detailinfo != '')
            { alert(detailinfo); }
        });

        $('body').on('click', '#closecomment', function () {
            $('#myeditorx').removeClass('hide').addClass('hide');
            $('#watreport').modal('hide');
        });

        var myWATtable = null;
        function showwatcomment(output)
        {
            if (myWATtable) {
                myWATtable.destroy();
                myWATtable = null;
            }
            $("#wATTableID").empty();

            $.each(output.watcommentdata, function (i, val) {
                $("#wATTableID").append(
                            '<tr>' +
                            '<td>' + val.comment + '</td>' +
                            '<td>' + val.updatetime + '</td>' +
                            '</tr>'
                        );
            });
            
            myWATtable = $('#myWATtable').DataTable({
                'iDisplayLength': -1,
                'aLengthMenu': [[30, 60, 100, -1],
                [30, 60, 100, "All"]],
                "columnDefs": [
                    { "targets": "_all" }
                ],
                "aaSorting": [],
                "order": [],
                dom: 'lBfrtip',
                buttons: ['copyHtml5']
            });
        }

        $('body').on('click', '.watanalyzebtn', function () {
            var watid = $(this).attr('myid');
            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options)

            $.post('/WATLogic/LoadWATAnalyzeComment', {
                watid:watid
            }, function (output) {
                showwatcomment(output);
                $.bootstrapLoading.end();
            });

            $('#hwatid').val(watid);
            $('modaltitle').html(watid + " WAT Analyze");
            $('#watreport').modal('show');
        });

        $('body').on('click', '#addcomment', function () {

            var watid = $('#hwatid').val();
            $.base64.utf8encode = true;
            var wholeval = CKEDITOR.instances.editor1.getData();
            if (wholeval == '')
            { return false; }

            var comment = $.base64.btoa(wholeval);
            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options)

            $.post('/WATLogic/AddWATAnalyzeComment', {
                watid: watid,
                comment: comment
            }, function (output) {
                showwatcomment(output);
                CKEDITOR.instances.editor1.setData('');
                $("#" + watid).removeClass('HasComment').addClass('HasComment');
                $.bootstrapLoading.end();
            });

        });

        $('body').on('click', '#expandeditor', function () {
            if ($('#myeditorx').hasClass('hide')) {
                $('#myeditorx').removeClass('hide');
            }
            else { $('#myeditorx').addClass('hide'); }
        })

    }

        var wuxiwatwipfun = function () {
        var logictable = null;
        var wuxistat = function () {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/MGTL/WATWIPDATA',
                { },
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
                            '<th class="dt-center">WAFER</th>' +
                            '<th class="dt-center">Type</th>' +
                            '<th class="dt-center">Array</th>' +
                            '<th class="dt-center">STEP</th>' +
                            '<th class="dt-center">HASXY</th>' +
                            '<th class="dt-center">Last Active</th>' +
                            '<th class="dt-center">Spend Hours</th>' +
                            '<th class="dt-center">OGPPHOTO</th>' +
                            '</tr>'
                        );
                    
                    $.each(output.wipdata, function (i, val) {

                        $("#logiccontent").append(
                            '<tr>' +
                            '<td class="dt-center">' + val.WAFER + '</td>' +
                            '<td class="dt-center">' + val.VType + '</td>' +
                            '<td class="dt-center">' + val.VArray + '</td>' +
                            '<td class="dt-center">' + val.STEP + '</td>' +
                            '<td class="dt-center">' + val.LocalXY + '</td>' +
                            '<td class="dt-center">' + val.TESTTIMESTAMP + '</td>' +
                            '<td class="dt-center">' + val.During + '</td>' +
                            '<td class="dt-center">' + val.HasOGP + '</td>' +
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


        $(function(){
            wuxistat();
        });

    }

    var wuxiwatgoldenfun = function ()
    {
        $('.date').datepicker({ autoclose: true, viewMode: "days", minViewMode: "days" });

        function golddata(tester, sdate, edate)
        {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATGoldSampleData', {
                tester: tester,
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();

                    $.each(output.chartlist, function (idx, val) {
                        var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-12">' +
                                   '<div class="v-box" id="' + val.id + '"></div>' +
                                   '</div></div>';
                        $('#chartdiv').append(appendstr);
                        drawgoldplot(val);
                    });
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var tester = $('#goldentesterlist').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            if (tester == '')
            {
                alert('To review golden sample data,tester need to be selected!');
                return false;
            }
            golddata(tester, sdate, edate);
        });

        $(function () {
            var tester = $('#htester').val();
            if (tester == '') {
                return false;
            }
            golddata(tester, '', '');
        });

        var drawgoldplot = function (boxplot_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'scatter'
                },
                title: {
                    text: boxplot_data.title
                },

                legend: {
                    enabled: true
                },

                xAxis: {
                    categories: boxplot_data.categories
                },

                yAxis: {
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
                series: boxplot_data.seriallist,
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


        function goldpwrdata(tester, sdate, edate)
        {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATGoldSamplePwrData', {
                tester: tester,
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();

                    $.each(output.chartlist, function (idx, val) {
                        var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-12">' +
                                   '<div class="v-box" id="' + val.id + '"></div>' +
                                   '</div></div>';
                        $('#chartdiv').append(appendstr);
                        drawgoldpwrplot(val);
                    });
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-pwr', function () {
            var tester = $('#goldentesterlist').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            if (tester == '')
            {
                alert('To review golden sample data,tester need to be selected!');
                return false;
            }
            goldpwrdata(tester, sdate, edate);
        });

        var drawgoldpwrplot = function (boxplot_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'line'
                },
                title: {
                    text: boxplot_data.title
                },

                legend: {
                    enabled: true
                },

                xAxis: {
                    categories: boxplot_data.categories
                },

                yAxis: {
                    min: boxplot_data.min,
                    max:boxplot_data.max,
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
                series: boxplot_data.seriallist,
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


        function golddttdata(tester, sdate, edate) {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATGoldSampleDttData', {
                tester: tester,
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();

                    $.each(output.chartlist, function (idx, val) {
                        var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-12">' +
                                   '<div class="v-box" id="' + val.id + '"></div>' +
                                   '</div></div>';
                        $('#chartdiv').append(appendstr);
                        drawdttplot(val);
                    });
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-dtt', function () {
            var tester = $('#goldentesterlist').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            if (tester == '') {
                alert('To review golden sample data,tester need to be selected!');
                return false;
            }
            golddttdata(tester, sdate, edate);
        });

        var drawdttplot = function (boxplot_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'line'
                },
                title: {
                    text: boxplot_data.title
                },

                legend: {
                    enabled: true
                },

                xAxis: {
                    categories: boxplot_data.categories
                },
                yAxis: {
                //    min: boxplot_data.min,
                //    max: boxplot_data.max,
                //    plotLines: [{
                //        value: boxplot_data.lowlimit,
                //        color: 'green',
                //        dashStyle: 'Dash',
                //        width: 2,
                //        label: {
                //            text: 'LL',
                //            align: 'left'
                //        }
                //    }, {
                //        value: boxplot_data.highlimit,
                //        color: 'green',
                //        dashStyle: 'Dash',
                //        width: 2,
                //        label: {
                //            text: 'UL',
                //            align: 'left'
                //        }
                //    }]
                },
                //annotations: [{
                //    labels: boxplot_data.labels,
                //    color: '#d4d4d4',
                //    draggable: 'xy'
                //}],
                series: boxplot_data.seriallist,
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

    var wuxiwatovenfun = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "days", minViewMode: "days" });

        function ovendata(tester, sdate, edate) {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATOvenData', {
                tester: tester,
                sdate: sdate,
                edate: edate
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();

                    $.each(output.chartlist, function (idx, val) {
                        var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-12">' +
                                   '<div class="v-box" id="' + val.id + '"></div>' +
                                   '</div></div>';
                        $('#chartdiv').append(appendstr);
                        drawovenplot(val);
                    });
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var tester = $('#goldentesterlist').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            if (tester == '') {
                alert('To review OVEN data,tester need to be selected!');
                return false;
            }
            ovendata(tester, sdate, edate);
        });

        $(function () {
            var tester = $('#htester').val();
            if (tester == '') {
                return false;
            }
            ovendata(tester, '', '');
        });
    }

    var wuxiwatcouponovenfun = function ()
    {
        $.post('/WATLogic/GetWXCouponID', {
        }, function (output) {
            $('#wafernum').autoComplete({
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
            $('#wafernum').attr('readonly', false);
        });

        $.post('/WATLogic/GetWXCouponIndex', {
        }, function (output) {
            $('#cpidx').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.idxlist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#cpidx').attr('readonly', false);
        });

        function covendata(couponid) {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATCouponOVENData', {
                couponid: couponid
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    $('#chartdiv').empty();

                    $.each(output.chartlist, function (idx, val) {
                        var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-12">' +
                                   '<div class="v-box" id="' + val.id + '"></div>' +
                                   '</div></div>';
                        $('#chartdiv').append(appendstr);
                        drawovenplot(val);
                    });
                }
                else {
                    $('#chartdiv').empty();
                    alert(output.msg);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var couponid = $('#wafernum').val() + $('#cpidx').val();
            if (couponid.length < 11) {
                alert('To review COUPON OVEN data,correct wafer number need to be input');
                return false;
            }
            covendata(couponid);
        });

        function downloadoven(couponid) {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/WATLogic/WUXIWATCouponOVENDownload', {
                couponid: couponid
            }, function (output) {
                $.bootstrapLoading.end();
                window.location.href = output.url;
            });
        }

        $('body').on('click', '#btn-download', function () {
            var couponid = $('#wafernum').val() + $('#cpidx').val();
            if (couponid.length < 11) {
                alert('To download COUPON OVEN data,correct wafer number need to be input');
                return false;
            }
            downloadoven(couponid);
        });
        

        $(function () {
            var couponid = $('#hwafernum').val();
            if (couponid == '') {
                return false;
            }
            covendata(couponid);
        });

    }

    var drawovenplot = function (boxplot_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'scatter'
            },
            title: {
                text: boxplot_data.title
            },

            legend: {
                enabled: false
            },

            xAxis: {
                categories: boxplot_data.categories
            },

            yAxis: {
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
            series: [
            {
                type: 'scatter',
                data: boxplot_data.datalist,
                marker: {
                    lineWidth: 1,
                    radius: 2.5
                },
                tooltip: {
                    headerFormat: '',
                    pointFormat: "{point.y}"
                },
                turboThreshold: 800000
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
        },
        WUXIWATXY: function () {
            wuxiwatxyfun();
        },
        ALLENWATXY: function () {
            allenwatxyfun();
        },
        WUXIWATCOUPON: function () {
            wuxiwatcouponfun();
        },
        WUXIWATPVP: function () {
            wuxiwatpvpfun();
        },
        WUXIWATSTATUS: function () {
            wuxiwatstatusfun();
        },
        WUXIWATWIP: function () {
            wuxiwatwipfun();
        },
        WUXIWATGOLDEN: function ()
        {
            wuxiwatgoldenfun();
        },
        WUXIWATOVEN: function () {
            wuxiwatovenfun();
        },
        WUXIWATCOUPONOVEN: function ()
        {
            wuxiwatcouponovenfun();
        },
        WUXIWATDIS: function () {
            wuxiwatdisfun();
        }

    }
}();