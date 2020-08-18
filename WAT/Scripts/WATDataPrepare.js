var WATDATAPAREPARE = function () {

    var neomapload = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("input the wafer numbers!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/NeomapToAllenData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               $.each(output.wfdatalist, function (i, val) {
                   var trstyle = '';
                   if (val.stat.indexOf('OK') == -1)
                   { trstyle = 'background-color:orange'; }

                   var appendstr = '<tr style="'+trstyle+'">';
                   appendstr += '<td>' + val.wf + '</td>'
                   appendstr += '<td>' + val.stat + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var probe2wx = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("input the wafer numbers!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/Prepare4WATData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               $.each(output.wfdatalist, function (i, val) {
                   var trstyle = '';
                   if (val.stat.indexOf('OK') == -1)
                   { trstyle = 'background-color:orange'; }

                   var appendstr = '<tr style="' + trstyle + '">';
                   appendstr += '<td>' + val.wf + '</td>'
                   appendstr += '<td>' + val.stat + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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
        

        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var map2wx = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("input the wafer numbers!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/PrepareBinMapFile',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               $.each(output.wfdatalist, function (i, val) {
                   var trstyle = '';
                   if (val.stat.indexOf('OK') == -1)
                   { trstyle = 'background-color:orange'; }

                   var appendstr = '<tr style="' + trstyle + '">';
                   appendstr += '<td>' + val.wf + '</td>'
                   appendstr += '<td>' + val.stat + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var samplepick = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("input the wafer numbers!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/DieSort/WATSamplePickData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
                   mywafertable = null;
               }

               $("#WaferTableID").empty();

               $.each(output.wfdatalist, function (i, val) {
                   var trstyle = '';
                   if (val.stat.indexOf('OK') == -1)
                   { trstyle = 'background-color:orange'; }

                   var appendstr = '<tr style="' + trstyle + '">';
                   appendstr += '<td>' + val.wf + '</td>'
                   appendstr += '<td>' + val.stat + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var allenbinfun = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("input the wafer numbers!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/ALLENBinData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               $.each(output.datalist, function (i, val) {

                   var appendstr = '<tr">';
                   appendstr += '<td>' + val.wafer + '</td>'
                   appendstr += '<td>' + val.product + '</td>'
                   appendstr += '<td>' + val.bin54 + '</td>'
                   appendstr += '<td>' + val.bin55 + '</td>'
                   appendstr += '<td>' + val.bin56 + '</td>'
                   appendstr += '<td>' + val.bin57 + '</td>'
                   appendstr += '<td>' + val.bin58 + '</td>'
                   appendstr += '<td>' + val.bin59 + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var wuxibinfun = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("input the wafer numbers!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/WUXIBinData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
               }
               $("#WaferTableID").empty();

               $.each(output.datalist, function (i, val) {

                   var appendstr = '<tr">';
                   appendstr += '<td>' + val.Wafer + '</td>'
                   appendstr += '<td>' + val.Status + '</td>'
                   appendstr += '<td>' + val.Bin + '</td>'
                   appendstr += '<td>' + val.PN + '</td>'
                   appendstr += '<td>' + val.QTY + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var iivisamplepickfun = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("input the wafer numbers!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/DieSort/IIVISamplePickData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
                   mywafertable = null;
               }

               $("#WaferTableID").empty();

               $.each(output.wfdatalist, function (i, val) {
                   var trstyle = '';
                   if (val.stat.indexOf('OK') == -1)
                   { trstyle = 'background-color:orange'; }
                   else
                   { trstyle = 'background-color:green'; }

                   var appendstr = '<tr style="' + trstyle + '">';
                   appendstr += '<td>' + val.wf + '</td>'
                   appendstr += '<td>' + val.stat + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var wf2dcfun = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                if (warning) {
                    alert("input the wafer numbers!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/DieSort/WF2DCData',
           {
               marks: JSON.stringify(cur_marks)
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
                   mywafertable = null;
               }

               $("#WaferTableID").empty();

               $.each(output.wfdatalist, function (i, val) {
                   var appendstr = '<tr>';
                   appendstr += '<td>' + val.wf + '</td>'
                   appendstr += '<td>' + val.dc + '</td>'
                   appendstr += '<td>' + val.jo + '</td>'
                   appendstr += '<td>' + val.date + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    var iivibinfun = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(warning) {

            var wafernum = $('#wafernum').val();
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        alert(val + " has already existed.");
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })

            if (wafernum == '') {
                if (warning) {
                    alert("输入WAFER 号!");
                }
                return false;
            }

            if (cur_marks.length === 0) {
                if (warning) {
                    alert("输入坐标!");
                }
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/IIVIBinData',
           {
               marks: JSON.stringify(cur_marks),
               wafernum: wafernum
           }, function (output) {
               $.bootstrapLoading.end();
               if (mywafertable) {
                   mywafertable.destroy();
                   mywafertable = null;
               }

               $("#WaferTableID").empty();

               if (output.MSG != '')
               { alert(output.MSG); return false;}

               $.each(output.wfdatalist, function (i, val) {
                   var appendstr = '<tr>';
                   appendstr += '<td>' + val.wf + '</td>'
                   appendstr += '<td>' + val.x + '</td>'
                   appendstr += '<td>' + val.y + '</td>'
                   appendstr += '<td>' + val.bin + '</td>'
                   appendstr += '</tr>';

                   $("#WaferTableID").append(appendstr);
               });


               mywafertable = $('#mywafertable').DataTable({
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


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable(true);
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');
            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })
    }

    return {
        NEOMAPLOADER:function(){
            neomapload();
        },
        PROBE2WUXI: function () {
            probe2wx();
        }, MAPFILE2WUXI: function () {
            map2wx();
        }, WATSAMPLEPICK: function () {
            samplepick();
        },
        ALLENBININFO: function () {
            allenbinfun();
        },
        WUXIBININFO: function () {
            wuxibinfun();
        },
        IIVISAMPLEPICK: function () {
            iivisamplepickfun();
        },
        WF2DC: function () {
            wf2dcfun();
        },
        IIVIBIN: function () {
            iivibinfun();
        }
    }

}();
