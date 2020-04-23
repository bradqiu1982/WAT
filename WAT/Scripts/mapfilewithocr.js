var MAPOCR = function () {

    var mapocrfun = function () {
        var wafertable = null;

        $.post('/Main/GetWXWATWafer', {
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

        $.post('/Main/GetWXOCRWafer', {
        }, function (output) {
            $('#lotnum').autoComplete({
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
            $('#lotnum').attr('readonly', false);
        });

        var checkbin = function ()
        {
            var wafer = $('#wafernum').val();
            var lotnum = $('#lotnum').val();

            if (wafer == '' || lotnum == '') {
                alert('please input wafer number and ocr lot number!');
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

            $.post('/Main/BinCheckWithOCRData', {
                wafer: wafer,
                lotnum: lotnum
            }, function (output) {
                $.bootstrapLoading.end();
                
                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }

                $("#wafercontent").empty();

                $.each(output.ocrlist, function (i, val) {
                    var capimg = '<img src="data:image/png;base64,' + val.CaptureImg + '" />';
                    $("#wafercontent").append(
                        '<tr class="CFD'+val.CFDLevel+'">' +
                            '<td>' + val.WaferNum + '</td>' +
                            '<td>' + val.Product + '</td>' +
                            '<td>' + val.OCRFile + '</td>' +
                            '<td>' + capimg + '</td>' +
                            '<td>' + val.X + '</td>' +
                            '<td>' + val.Y + '</td>' +
                            '<td>' + val.Bin + '</td>' +
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

                if (output.MSG != '') {
                    alert(output.MSG);
                }
            });
            
        }

        $('body').on('click', '#btn-check', function () {
            checkbin();
        });
    }

    return {
        mapocrinit: function () {
            mapocrfun();
        }
    }
}();