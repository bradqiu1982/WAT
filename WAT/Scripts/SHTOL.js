var SHTOL = function () {

    var SHTOLDS = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "days", minViewMode: "days", pickerPosition: "bottom-left" });

        function GetSHTOLDIST() {
            var sdate = $.trim($('#sdate').val());
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/SHTOL/GetSHTOLOutputDis', {
                sdate: sdate
            }, function (output) {
                $.bootstrapLoading.end();
                $('#v-content1').empty();
                var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                              '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                              '</div><div class="col-xs-1"></div></div>';
                $('#v-content1').append(appendstr);
                drawline(output.chartdata);
            });
        }

        $('body').on('click', '#btn-search', function () {
            GetSHTOLDIST();
        })

        function GetSHTOLWIP() {
            $.post('/SHTOL/GetSHTOLWIP', {
            }, function (output) {
                $.bootstrapLoading.end();
                $('#v-content2').empty();
                var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                              '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                              '</div><div class="col-xs-1"></div></div>';
                $('#v-content2').append(appendstr);
                drawcolumn(output.chartdata);
            });
        }

        $(function () {
            GetSHTOLDIST();
            GetSHTOLWIP();
        });

        var drawline = function (col_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'line'
                },
                title: {
                    text: col_data.title
                },
                xAxis: {
                    categories: col_data.xlist
                },
                legend: {
                    enabled: true,
                },
                yAxis: {
                    title: {
                        text: 'Count'
                    }
                },
                tooltip: {
                    headerFormat: '',
                    pointFormatter: function () {
                        return (this.y == 0) ? '' : '<span>' + this.series.name + '</span>: <b>' + this.y + '</b><br/>';
                    },
                    shared: true
                },
                plotOptions: {
                    line: {
                        dataLabels: {
                            enabled: true
                        }
                    }
                },
                series: col_data.series,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + col_data.id).parent().toggleClass('chart-modal');
                                $('#' + col_data.id).highcharts().reflow();
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
            Highcharts.chart(col_data.id, options);
        }

        var drawcolumn = function (col_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'column'
                },
                title: {
                    text: col_data.title
                },
                xAxis: {
                    categories: col_data.xlist
                },
                legend: {
                    enabled: true,
                },
                yAxis: {
                    title: {
                        text: 'Count'
                    }
                },
                tooltip: {
                    headerFormat: '',
                    pointFormatter: function () {
                        return (this.y == 0) ? '' : '<span>' + this.category + '</span>: <b>' + this.y + '</b><br/>';
                    },
                    shared: true
                },
                plotOptions: {
                    column: {
                        dataLabels: {
                            enabled: true
                        }
                    }
                },
                series: col_data.series,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + col_data.id).parent().toggleClass('chart-modal');
                                $('#' + col_data.id).highcharts().reflow();
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
            Highcharts.chart(col_data.id, options);
        }
    }

    var SHTOLSTAT = function () {
        var watdatatable = null;

        function loadshtolstatdata() {
            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/SHTOL/LoadSHTOLStatus', {}, function (output) {
                $.bootstrapLoading.end();
                
                if (watdatatable) {
                    watdatatable.destroy();
                    watdatatable = null;
                }
                $("#shtoldata").empty();

                $.each(output.shtolstatdata, function (i, val) {
                    var analyze = '<a href="/SHTOL/SHTOLAnalyze?SN=' + val.SN + '" target="_blank" >Analyze</a>';
                    var cfm = '<input class="btn btn-success CFMBTN" type="button" sn="' + val.SN + '" value="Confirm"/>'
                    var ign = '<input class="btn btn-success IGNBTN" type="button" sn="' + val.SN + '" value="Ignore"/>'
                    $("#shtoldata").append(
                        '<tr class="' + val.CFM + '">' +
                            '<td>' + val.SN + '</td>' +
                            '<td>' + val.Product + '</td>' +
                            '<td>' + val.DataField + '</td>' +
                            '<td>' + val.MNVal + '</td>' +
                            '<td>' + val.MXVal + '</td>' +
                            '<td>' + val.Reason + '</td>' +
                            '<td>' + val.FinishTime + '</td>' +
                            '<td>' + analyze + '</td>' +
                            '<td>' + cfm + '</td>' +
                            '<td>' + ign + '</td>' +
                            //'<td>' + val.Desc + '</td>' +
                        '</tr>'
                        );
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
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });
            });

        }

        $(function () {
            loadshtolstatdata();
        });


        function updatejudge(sn,stat)
        {
            $.post('/SHTOL/UpdateSHTOLJudgement', {
                sn: sn,
                stat: stat
            }, function (output) {
                window.location.reload(true);
            });
        }

        $('body').on('click', '.CFMBTN', function () {
            var sn = $(this).attr('sn');
            if (sn != '')
            { updatejudge(sn, 'CONFIRM'); }
        });

        $('body').on('click', '.IGNBTN', function () {
            var sn = $(this).attr('sn');
            if (sn != '')
            { updatejudge(sn, 'IGNORE'); }
        });
    }

    var SHTOLALZ = function () {
        function LoadAnalyzeChart(sn)
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

            $.post('/SHTOL/SHTOLAnalyzeChart', {
                sn: sn
            }, function (output) {
                $.bootstrapLoading.end();

                $('#v-content1').empty();
                var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                              '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                              '</div><div class="col-xs-1"></div></div>';
                $('#v-content1').append(appendstr);
                drawline(output.chartdata);
            });
        }

        $(function () {
            var sn = $('#sn').val();
            if (sn != '')
            { LoadAnalyzeChart(sn);}
        });

        $('body').on('click', '#btn-search', function () {
            var sn = $('#sn').val();
            if (sn != '')
            { LoadAnalyzeChart(sn); }
        });

        var drawline = function (col_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'line',
                    alignTicks: false
                },
                title: {
                    text: col_data.title
                },
                xAxis: {
                    categories: col_data.xlist
                },
                legend: {
                    enabled: true,
                },
                yAxis: [{
                    title: {
                        text: 'DBm'
                    }
                }, { // Secondary yAxis
                    labels: {
                        format: '{value}°C',
                    },
                    title: {
                    text: 'Temperature'
                  },
                  opposite: true,
                  min: 60,
                  max: 80
                }],
                tooltip: {
                    headerFormat: '',
                    pointFormatter: function () {
                        return (this.y == 0) ? '' : '<span>' + this.series.name + '</span>: <b>' + this.y + '</b><br/>';
                    },
                    shared: true
                },
                plotOptions: {
                    line: {
                        dataLabels: {
                            enabled: false
                        }
                    }
                },
                series: col_data.series,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + col_data.id).parent().toggleClass('chart-modal');
                                $('#' + col_data.id).highcharts().reflow();
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
            Highcharts.chart(col_data.id, options);
        }
    }

    return {
        SHTOLdash: function () {
            SHTOLDS();
        },
        SHTOLStatus: function () {
            SHTOLSTAT();
        },
        SHTOLAnalyze: function () {
            SHTOLALZ();
        }
    }
}();