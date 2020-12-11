var WWDS = function () {

    var CAPINPUT = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months", pickerPosition: "bottom-left" });
        var wafertable = null;

        function searchdata() {
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

            $.post('/DashBoard/WeeklyCapacityDSData', {
                sdate: sdate
            }, function (output) {
                $.bootstrapLoading.end();

                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }
                $("#waferhead").empty();
                $("#wafercontent").empty();

                var appstr = '<tr>';
                $.each(output.title, function (i, val) {
                    appstr += '<th>' + val + '</th>';
                });
                appstr += '</tr>';
                $("#waferhead").append(appstr);

                $.each(output.table, function (i, line) {
                    appstr = '<tr>';
                    $.each(line, function (i, val) {
                        appstr += '<td>' + val + '</td>';
                    });
                    appstr += '</tr>';
                    $("#wafercontent").append(appstr);
                });

                
                wafertable = $('#wafertable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all", "bSortable": false }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                $('.v-content1').empty();
                var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                              '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                              '</div><div class="col-xs-1"></div></div>';
                $('.v-content1').append(appendstr);
                drawcolumn(output.chartdata);

            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        $(function () {
            searchdata();
        });

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
                    categories: col_data.xAxis
                },
                legend: {
                    enabled: true,
                },
                yAxis: [{
                    title: {
                        text: 'INPUT'
                    },
                    stackLabels: {
                        enabled: true,
                        style: {
                            fontWeight: 'bold',
                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                        }
                    }
                }, {
                    opposite: true,
                    title: {
                        text: 'CAPACITY'
                    }
                }],
                tooltip: {
                    headerFormat: '',
                    pointFormatter: function () {
                        return (this.y == 0) ? '' : '<span>' + this.series.name + '</span>: <b>' + this.y + '</b><br/>';
                    },
                    shared: true
                },
                plotOptions: {
                    column: {
                        stacking: 'normal'
                    }
                },
                series: col_data.data,
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

    var WKYIELD = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months", pickerPosition: "bottom-left" });
        var wafertable = null;
        var wattable = null;

        function searchdata() {
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

            $.post('/DashBoard/WeeklyYieldDSData', {
                sdate: sdate
            }, function (output) {
                $.bootstrapLoading.end();

                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }
                $("#waferhead").empty();
                $("#wafercontent").empty();

                var appstr = '<tr>';
                $.each(output.title, function (i, val) {
                    appstr += '<th>' + val + '</th>';
                });
                appstr += '</tr>';
                $("#waferhead").append(appstr);

                $.each(output.table, function (i, line) {
                    appstr = '<tr>';
                    $.each(line, function (i, val) {
                        if (val == '0/0') {
                            appstr += '<td></td>'; }
                        else { appstr += '<td>' + val + '</td>'; }
                    });
                    appstr += '</tr>';
                    $("#wafercontent").append(appstr);
                });


                wafertable = $('#wafertable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all", "bSortable": false }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });


                if (wattable)
                {
                    wattable.destroy();
                    wattable = null;
                }
                $("#watcontent").empty();

                $.each(output.passfailcap, function (i, val) {
                    $("#watcontent").append(
                        '<tr class="' + val.Pass + '">' +
                        '<td>' + val.Wafer + '</td>' +
                        '<td>' + val.VType + '</td>' +
                        '<td>' + val.PN + '</td>' +
                        '<td>' + val.WKStr + '</td>' +
                        '<td>' + val.Pass + '</td>' +
                        '</tr>'
                    );
                });

                wattable = $('#wattable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center1", "targets": "_all" }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                //$('.v-content1').empty();
                //var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                //              '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                //              '</div><div class="col-xs-1"></div></div>';
                //$('.v-content1').append(appendstr);
                //drawyield(output.chartdata);

            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        $(function () {
            searchdata();
        });

        var drawyield = function (col_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'column'
                },
                title: {
                    text: col_data.title
                },
                xAxis: {
                    categories: col_data.xAxis
                },
                legend: {
                    enabled: true,
                },
                yAxis: [{
                    title: {
                        text: 'INPUT'
                    },
                    stackLabels: {
                        enabled: true,
                        style: {
                            fontWeight: 'bold',
                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                        }
                    }
                }, {
                    opposite: true,
                    title: {
                        text: 'CAPACITY'
                    }
                }],
                tooltip: {
                    headerFormat: '',
                    pointFormatter: function () {
                        return (this.y == 0) ? '' : '<span>' + this.series.name + '</span>: <b>' + this.y + '</b><br/>';
                    },
                    shared: true
                },
                plotOptions: {
                    column: {
                        stacking: 'normal'
                    }
                },
                series: col_data.data,
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

    var WKYIELD4PLAN = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months", pickerPosition: "bottom-left" });
        var wafertable = null;
        var wattable = null;

        function searchdata() {
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

            $.post('/DashBoard/WeeklyYield4PlanningData', {
                sdate: sdate
            }, function (output) {
                $.bootstrapLoading.end();

                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }
                $("#waferhead").empty();
                $("#wafercontent").empty();

                var appstr = '<tr>';
                $.each(output.title, function (i, val) {
                    appstr += '<th>' + val + '</th>';
                });
                appstr += '</tr>';
                $("#waferhead").append(appstr);

                $.each(output.table, function (i, line) {
                    appstr = '<tr>';
                    $.each(line, function (i, val) {
                        if (val == '0/0') {
                            appstr += '<td></td>';
                        }
                        else { appstr += '<td>' + val + '</td>'; }
                    });
                    appstr += '</tr>';
                    $("#wafercontent").append(appstr);
                });


                wafertable = $('#wafertable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all", "bSortable": false }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });


                if (wattable) {
                    wattable.destroy();
                    wattable = null;
                }
                $("#watcontent").empty();

                $.each(output.passfailcap, function (i, val) {
                    $("#watcontent").append(
                        '<tr class="' + val.Pass + '">' +
                        '<td>' + val.Wafer + '</td>' +
                        '<td>' + val.VType + '</td>' +
                        '<td>' + val.PN + '</td>' +
                        '<td>' + val.WKStr + '</td>' +
                        '<td>' + val.Pass + '</td>' +
                        '</tr>'
                    );
                });

                wattable = $('#wattable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center1", "targets": "_all" }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                //$('.v-content1').empty();
                //var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                //              '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                //              '</div><div class="col-xs-1"></div></div>';
                //$('.v-content1').append(appendstr);
                //drawyield(output.chartdata);

            })
        }

        $('body').on('click', '#btn-search', function () {
            searchdata();
        })

        $(function () {
            searchdata();
        });

        var drawyield = function (col_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'column'
                },
                title: {
                    text: col_data.title
                },
                xAxis: {
                    categories: col_data.xAxis
                },
                legend: {
                    enabled: true,
                },
                yAxis: [{
                    title: {
                        text: 'INPUT'
                    },
                    stackLabels: {
                        enabled: true,
                        style: {
                            fontWeight: 'bold',
                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                        }
                    }
                }, {
                    opposite: true,
                    title: {
                        text: 'CAPACITY'
                    }
                }],
                tooltip: {
                    headerFormat: '',
                    pointFormatter: function () {
                        return (this.y == 0) ? '' : '<span>' + this.series.name + '</span>: <b>' + this.y + '</b><br/>';
                    },
                    shared: true
                },
                plotOptions: {
                    column: {
                        stacking: 'normal'
                    }
                },
                series: col_data.data,
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

    var WXWIP = function () {
        var wafertable = null;

        function searchdata() {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/DashBoard/WUXIWATWIPDSDATA', {

            }, function (output) {


                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }
                $("#waferhead").empty();
                $("#wafercontent").empty();

                var appstr = '<tr>';
                $.each(output.title, function (i, val) {
                    appstr += '<th>' + val + '</th>';
                });
                appstr += '</tr>';
                $("#waferhead").append(appstr);

                $.each(output.table, function (i, line) {
                    appstr = '<tr>';
                    $.each(line, function (i, val) {
                        if (val.indexOf('PASS') != -1 || val.indexOf('PENDING') != -1)
                        { appstr += '<td class="'+val+'"></td>'; }
                        else
                        { appstr += '<td>'+val+'</td>'; }
                    });
                    appstr += '</tr>';
                    $("#wafercontent").append(appstr);
                });


                wafertable = $('#wafertable').DataTable({
                    'iDisplayLength': 100,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all"}
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5']
                });

                $.bootstrapLoading.end();
            })
        }

        $(function () {
            searchdata();
        });

    }

    var CAPPredict = function ()
    {
        function searchdata() {

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/DashBoard/WATCapPredictDATA', {

            }, function (output) {

                $('.v-content1').empty();
                var appendstr = '<div class="row" style="margin-top:10px!important"><div class="col-xs-1"></div><div class="col-xs-10" style="height: 410px;">' +
                              '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                              '</div><div class="col-xs-1"></div></div>';
                $('.v-content1').append(appendstr);
                drawcolumn(output.chartdata);
                $.bootstrapLoading.end();
            })
        }

        $(function () {
            searchdata();
        });

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
                    categories: col_data.xAxis
                },
                legend: {
                    enabled: true,
                },
                yAxis: [{
                    title: {
                        text: 'SLOT'
                    },
                    stackLabels: {
                        enabled: true,
                        style: {
                            fontWeight: 'bold',
                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                        }
                    }
                }],
                tooltip: {
                    headerFormat: '',
                    pointFormatter: function () {
                        return (this.y == 0) ? '' : '<span>' + this.series.name + '</span>: <b>' + this.y + '</b><br/>';
                    },
                    shared: true
                },
                plotOptions: {
                    column: {
                        stacking: 'normal'
                    }
                },
                series: col_data.data,
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


    var RTYIELD = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months", pickerPosition: "bottom-left" });
        var wafertable = null;
        var wattable = null;
        var wafertableprod = null;

        function searchdata() {
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

            $.post('/DashBoard/RTYieldDSData', {
                sdate: sdate
            }, function (output) {
                $.bootstrapLoading.end();

                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }
                $("#waferhead").empty();
                $("#wafercontent").empty();

                var appstr = '<tr>';
                $.each(output.title, function (i, val) {
                    appstr += '<th>' + val + '</th>';
                });
                appstr += '</tr>';
                $("#waferhead").append(appstr);

                $.each(output.table, function (i, line) {
                    appstr = '<tr>';
                    $.each(line, function (i, val) {
                        if (val == '0/0') {
                            appstr += '<td></td>';
                        }
                        else { appstr += '<td>' + val + '</td>'; }
                    });
                    appstr += '</tr>';
                    $("#wafercontent").append(appstr);
                });


                wafertable = $('#wafertable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all", "bSortable": false }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5'],
                    //scrollY: true,
                    //scrollX: true,
                    //scrollCollapse: true,
                    //paging: false,
                    //fixedColumns: {
                    //    leftColumns: 1
                    //}
                });

                if (wafertableprod) {
                    wafertableprod.destroy();
                    wafertableprod = null;
                }
                $("#waferheadprod").empty();
                $("#wafercontentprod").empty();

                var appstr = '<tr>';
                $.each(output.title, function (i, val) {
                    appstr += '<th>' + val + '</th>';
                });
                appstr += '</tr>';
                $("#waferheadprod").append(appstr);

                $.each(output.tableprod, function (i, line) {
                    appstr = '<tr>';
                    $.each(line, function (i, val) {
                        if (val == '0/0') {
                            appstr += '<td></td>';
                        }
                        else { appstr += '<td>' + val + '</td>'; }
                    });
                    appstr += '</tr>';
                    $("#wafercontentprod").append(appstr);
                });


                wafertableprod = $('#wafertableprod').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all", "bSortable": false }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['copyHtml5', 'csv', 'excelHtml5'],
                    //scrollX: true,
                });

                if (wattable) {
                    wattable.destroy();
                    wattable = null;
                }
                $("#watcontent").empty();

                $.each(output.passfailcap, function (i, val) {
                    $("#watcontent").append(
                        '<tr class="' + val.Pass + '">' +
                        '<td>' + val.Wafer + '</td>' +
                        '<td>' + val.Prod + '</td>' +
                        '<td>' + val.VType + '</td>' +
                        '<td>' + val.PN + '</td>' +
                        '<td>' + val.WKStr + '</td>' +
                        '<td>' + val.Pass + '</td>' +
                        '</tr>'
                    );
                });

                wattable = $('#wattable').DataTable({
                    'iDisplayLength': 50,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center1", "targets": "_all" }
                    ],
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

    }

    return {
        CapInit: function () {
            CAPINPUT();
        },
        YieldInit: function () {
           WKYIELD();
        },
        Yield4Planning: function () {
            WKYIELD4PLAN();
        },
        WIPInit: function () {
            WXWIP();
        },
        CapPredictInit: function () {
            CAPPredict();
        },
        RTYieldInit: function () {
            RTYIELD();
        }
    }
}();