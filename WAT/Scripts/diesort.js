﻿var DIESORT = function () {

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
                pn: pn
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

    return {
        REVIEWINIT: function () {
            reviewdiesort();
        },
        MANAGEINIT: function () {
            managediesort();
        }
    }
}();