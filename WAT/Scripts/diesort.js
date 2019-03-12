var DIESORT = function () {

    var reviewdiesort = function () {
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

                var appendstr = '<div class="col-xs-12">' +
                        '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                        '</div>';
                $('.v-content').append(appendstr);
                drawdiesortmap(output.chartdata);
            })
        }

        var reconstructdata = function () {
            var fs = $('#mapfile').val();
            if (fs == '') {
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

            $.post('/DieSort/ReConstructDieSort', {
                fs: fs
            }, function (output) {
                $.bootstrapLoading.end();

                $('.v-content').empty();
                if (!output.sucess) {
                    alert('Fail to re-construct this file!');
                    return false;
                }

                var appendstr = '<div class="col-xs-12">' +
                        '<div class="v-box" id="' + output.chartdata.id + '"></div>' +
                        '</div>';
                $('.v-content').append(appendstr);
                drawdiesortmap(output.chartdata);
            })
        }

        $('body').on('click', '#btn-search', function () {
            reviewdata();
        });

        $('body').on('click', '#btn-reconstruct', function () {
            if (confirm('Do you really want to re-construct this file? Your action will be recorded!'))
            {
                reconstructdata();
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
            },
            boost: {
                useGPUTranslations: true
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
        }
    }
}();