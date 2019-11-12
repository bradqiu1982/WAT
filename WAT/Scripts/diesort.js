var DIESORT = function () {

    var reviewdiesort = function () {
        var wafertable = null;

        $.post('/DieSort/LoadSortedFiles', {
        }, function (output) {
            $('#mapfile').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.filelist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#mapfile').attr('readonly', false);
        });

        var reviewdata = function ()
        {
            var fs = $('#mapfile').val();
            if (fs == '')
            {
                alert('Please input your file!');
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

            $.post('/DieSort/ReviewDieData', {
                fs:fs
            }, function (output) {
                $.bootstrapLoading.end();

                $('.v-content').empty();
                if (!output.sucess)
                {
                    alert('File not exist,you can re-construct this file!');
                    return false;
                }

                showdiedata(output);
            })
        }

        var showdiedata = function (output)
        {
                $('#bompn').val(output.pn);
                $('#bomarray').val(output.warray);
                $('#bomdesc').val(output.desc);

                var appendstr = '<div class="col-xs-12">' +
                        '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                        '</div>';
                $('.v-content').append(appendstr);
                drawdiesortmap(output.chartdata);

                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }
                $("#waferhead").empty();
                $("#wafercontent").empty();

                $("#waferhead").append(
                        '<tr>' +
                            '<th>Wafer</th>' +
                            '<th>X</th>' +
                            '<th>Y</th>' +
                            '<th>BIN</th>' +
                            '<th>LayoutID</th>' +
                            '<th>MPN</th>' +
                            '<th>FPN</th>' +
                            '<th>Array</th>' +
                            '<th>Desc</th>' +
                            '<th>Tech</th>' +
                            '<th>MAPFILE</th>' +
                            '<th>Time</th>' +
                         '</tr>'
                    );

                $.each(output.sampledata, function (i, val) {
                    $("#wafercontent").append(
                        '<tr>' +
                            '<td>' + val.Wafer + '</td>' +
                            '<td>' + val.XX + '</td>' +
                            '<td>' + val.YY + '</td>' +
                            '<td>' + val.BIN + '</td>' +
                            '<td>' + val.LayoutId + '</td>' +
                            '<td>' + val.MPN + '</td>' +
                            '<td>' + val.FPN + '</td>' +
                            '<td>' + val.PArray + '</td>' +
                            '<td>' + val.Des + '</td>' +
                            '<td>' + val.Tech + '</td>' +
                            '<td>' + val.MapFile + '</td>' +
                            '<td>' + val.UpdateTime + '</td>' +
                        '</tr>'
                        );
                });

                wafertable = $('#wafertable').DataTable({
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

        var reconstructdata = function () {
            var fs = $('#mapfile').val();
            if (fs == '') {
                alert('Please input your file!');
                return false;
            }

            var pn = $('#cmf-pn').val();
            var ctype = $('#constructtype').val();

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/DieSort/ReConstructDieSort', {
                fs: fs,
                pn: pn,
                ctype: ctype
            }, function (output) {
                $.bootstrapLoading.end();

                $('.v-content').empty();
                if (!output.sucess) {
                    alert('Fail to re-construct this file!');
                    return false;
                }

                showdiedata(output);
            })
        }

        $('body').on('click', '#btn-search', function () {
            reviewdata();
        });

        $('body').on('click', '#btn-reconstruct', function () {
            $('#confirmdlg').modal('show');
        });

        $('body').on('click', '#btn-cmf', function () {
            $('#confirmdlg').modal('hide');
            var pwd = $('#cfmpwd').val();
            if (pwd != '10086')
            { return false; }
            $('#cfmpwd').val('');
            reconstructdata();
        });
        
    }

    var wafer4planning = function ()
    {
        var wafertable = null;
        var wfdatatable = null;

        $.post('/DieSort/LoadSortedWafers', {
        }, function (output) {
            $('#wafernum').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.wafers;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#wafernum').attr('readonly', false);
        });


        var waferdatafun = function ()
        {
            var wafernum = $('#wafernum').val();
            if (wafernum == '') {
                alert('Please input wafer number!');
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

            $.post('/DieSort/LoadWaferData4Plan', {
                wafernum: wafernum
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
                            '<th>BIN</th>' +
                            '<th>Count</th>' +
                            '<th>Test</th>' +
                            //'<th>Finisar PN</th>' +
                            '<th>Array</th>' +
                            '<th>Desc</th>' +
                         '</tr>'
                    );

                $.each(output.sampledata, function (i, val) {
                    $("#wafercontent").append(
                        '<tr>' +
                            '<td>' + val.Wafer + '</td>' +
                            '<td>' + val.BIN + '</td>' +
                            '<td>' + val.Count + '</td>' +
                            '<td>' + val.Test + '</td>' +
                            //'<td>' + val.FPN + '</td>' +
                            '<td>' + val.Array + '</td>' +
                            '<td>' + val.Desc + '</td>' +
                        '</tr>'
                        );
                });

                wafertable = $('#wafertable').DataTable({
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


                if (wfdatatable) {
                    wfdatatable.destroy();
                    wfdatatable = null;
                }
                $("#wfdatahead").empty();
                $("#wfdatacontent").empty();

                $("#wfdatahead").append(
                        '<tr>' +
                            '<th>Wafer</th>' +
                            '<th>BIN</th>' +
                            '<th>Count</th>' +
                            '<th>Unsorted PN</th>' +
                            '<th>Uninspect PN</th>' +
                            '<th>Bom PN</th>' +
                            '<th>PN BIN</th>' +
                            //'<th>Finisar PN</th>' +
                            '<th>Array</th>' +
                            '<th>Desc</th>' +
                         '</tr>'
                    );

                $.each(output.waferorgdata, function (i, val) {
                    $("#wfdatacontent").append(
                        '<tr>' +
                            '<td>' + val.Wafer + '</td>' +
                            '<td>' + val.BIN + '</td>' +
                            '<td>' + val.Count + '</td>' +
                            '<td>' + val.UnsortedPN + '</td>' +
                            '<td>' + val.UninspectPN + '</td>' +
                            '<td>' + val.BomPN + '</td>' +
                            '<td>' + val.PNBIN + '</td>' +
                            //'<td>' + val.FPN + '</td>' +
                            '<td>' + val.Array + '</td>' +
                            '<td>' + val.Desc + '</td>' +
                        '</tr>'
                        );
                });

                wfdatatable = $('#wfdatatable').DataTable({
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

            })
        }


        $('body').on('click', '#btn-search', function () {
            waferdatafun();
        });


    }

    var managediesort = function ()
    {
        var diesorttable = null;
        function searchdata() {
            $.post('/DieSort/GetFailedConvertFile',
                {},
                function (output) {

                    if (diesorttable) {
                        diesorttable.destroy();
                        diesorttable = null;
                    }
                    $("#diesortcontent").empty();

                    $.each(output.datalist, function (i, val) {
                        $("#diesortcontent").append('<tr>' +
                            '<td>' + val.Name + '</td>' +
                            '<td>' + val.MSG + '</td>' +
                            '<td>' + val.UpdateTime + '</td>' +
                            '<td><button class = "btn btn-primary action" fs= "' + val.Name + '">Ignore</button></td>'
                            + '</tr>');
                    });

                    diesorttable = $('#diesorttable').DataTable({
                        'iDisplayLength': 50,
                        'aLengthMenu': [[20, 50, 100, -1],
                        [20, 50, 100, "All"]],
                        "aaSorting": [],
                        "order": [],
                        dom: 'lBfrtip',
                        buttons: ['copyHtml5', 'csv', 'excelHtml5']
                    });
                });
        }

        $(function () {
            searchdata();
        });

        $('body').on('click', '.action', function () {
            var fs = $(this).attr('fs');
            if (confirm('Do you really want to ignore file:' + fs + ' ?'))
            {
                $.post('/DieSort/UpdateIgnoreDieSort',
                    {fs:fs},
                    function (output) {
                        window.location.reload(true);
                    });
            }
        });

    }

    var drawdiesortmap = function (line_data)
    {
        var options = {
            title: {
                text: line_data.title
            },
            chart: {
                type: 'heatmap',
                zoomType: 'xy',
            },
            boost: {
                useGPUTranslations: true,
                usePreallocated: true,
                seriesThreshold: 1
            },
            xAxis:{
                title: {
                    text: 'X'
                },
                max:line_data.xmax
            },
            yAxis:{
                title: {
                    text: 'Y'
                },
                max: line_data.ymax,
                reversed: true
            },
            colorAxis: {
                min: 0,
                max: 10,
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

    var pddownloadfun = function () {

        function downloadpd(wf)
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

            $.post('/DieSort/DownLoadPDMapFileData', {
                wf: wf
            }, function (output) {
                $.bootstrapLoading.end();

                if (output.sucess) {
                    
                    $('.v-content').empty();
                    var appendstr = '<div class="col-xs-12">' +
                    '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                    '</div>';

                    $('.v-content').append(appendstr);
                    drawdiesortmap(output.chartdata);
                }
                else {
                    alert(output.MSG);
                }
            });
        }

        $('body').on('click', '#btn-search', function () {
            var wf = $('#mapfile').val();
            if (wf == '')
            { alert('please input the PD map file!'); }
            downloadpd(wf);
        });
    }

    var binsubstitutefun = function () {
        var wafertable = null;

        function loadbininfo() {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);
            $.post('/DieSort/LoadBinSubstituteData', {
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
                            '<th>WAT_Result</th>' +
                            '<th>From_Bin</th>' +
                            '<th>Bin_Count</th>' +
                            '<th>To_Bin</th>' +
                            '<th>From_PN</th>' +
                            '<th>To_PN</th>' +
                            '<th>Action</th>' +
                         '</tr>'
                    );

                $.each(output.binsubdata, function (i, val) {
                    $("#wafercontent").append(
                        '<tr>' +
                            '<td>' + val.wafer + '</td>' +
                            '<td>' + val.result + '</td>' +
                            '<td>' + val.frombin + '</td>' +
                            '<td>' + val.bincount + '</td>' +
                            '<td>' + val.tobin + '</td>' +
                            '<td>' + val.fpn + '</td>' +
                            '<td>' + val.tpn + '</td>' +
                            '<td><Button class="btn btn-success convertbin" wf = "' + val.wafer + '" fbin = "' + val.frombin + '" tbin = "' + val.tobin + '">To ' + val.tobin + '</Button></td>' +
                        '</tr>'
                        );
                });

                wafertable = $('#wafertable').DataTable({
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

        $(function(){
            loadbininfo();
        });


        function convertbin(wf, fbin, tbin)
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

            $.post('/DieSort/ConvertBinMapFileData', {
                wf: wf,
                fbin: fbin,
                tbin: tbin
            }, function (output) {
                $.bootstrapLoading.end();
                alert(output.MSG);
                window.location.reload(true);
            });
        }

        $('body').on('click', '.convertbin', function () {
            var wf = $(this).attr('wf');
            var fbin = $(this).attr('fbin');
            var tbin = $(this).attr('tbin');
            convertbin(wf, fbin, tbin);
        });
    }

    return {
        REVIEWINIT: function () {
            reviewdiesort();
        },
        MANAGEINIT: function () {
            managediesort();
        },
        WAFER4PLAN: function () {
            wafer4planning();
        },
        PDINIT: function () {
            pddownloadfun();
        },
        BINSUBSTITUTE: function ()
        {
            binsubstitutefun();
        }
    }
}();