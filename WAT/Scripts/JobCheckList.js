var JOBCHECK = function () {

    var watjobcheck = function () {
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

            $.post('/DashBoard/LoadWATJOBCheckData', {

            }, function (output) {

                if (wafertable) {
                    wafertable.destroy();
                    wafertable = null;
                }
                $("#waferhead").empty();
                $("#wafercontent").empty();

                var appstr = '<tr>';
                appstr += '<th>ID</th>';
                appstr += '<th>ITEM</th>';
                appstr += '<th>MARK</th>';
                appstr += '<th>CONFIRM</th>';
                appstr += '</tr>';
                $("#waferhead").append(appstr);

                $.each(output.jobchecklist, function (i, val) {
                    appstr = '<tr>';
                    appstr += '<td>' + val.CheckItemID + '</td>';
                    appstr += '<td>' + val.CheckItem + '</td>';
                    appstr += '<td><input class="form-control" type="text" id="Mark' + val.CheckItemID + '" name="Mark' + val.CheckItemID + '" value="' + val.Mark + '"></td>';
                    if (val.Status.indexOf("PENDING") != -1)
                    { appstr += '<td class="PENDING" id="BTN_' + val.CheckItemID + '" MARKNEED="' + val.MarkNeed + '" CHECKITEMID="' + val.CheckItemID + '">CONFIRM</td>'; }
                    else
                    { appstr += '<td class="DONE" MARKNEED="' + val.MarkNeed + '" CHECKITEMID="' + val.CheckItemID + '">DONE</td>'; }
                    appstr += '</tr>';
                    $("#wafercontent").append(appstr);
                });


                wafertable = $('#wafertable').DataTable({
                    'iDisplayLength': -1,
                    'aLengthMenu': [[ -1],
                    [ "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all" }
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

        function confirmitm(itemid,mark) {
            $.post('/DashBoard/UpdateWATJobData', {
                itemid: itemid,
                mark: mark
            }, function (output) {
                if (output.STATUS.indexOf('DONE') != -1)
                { $('#BTN_' + itemid).removeClass('PENDING').addClass('DONE'); }
                if (output.MSG != '')
                { alert(output.MSG);}
            })
        }

        $('body').on('click', '.PENDING', function () {
            var itemid = $(this).attr('CHECKITEMID');
            var markneed = $(this).attr('MARKNEED');
            var markval = $('#Mark' + itemid).val();
            if (markneed.indexOf("TRUE") != -1 && markval == '')
            {
                alert("This confirm need you to input MARK!");
                return false;
            }
            confirmitm(itemid, markval);
        })

    }

    return {
        WATCHECK: function () {
            watjobcheck();
        }
    }
}();