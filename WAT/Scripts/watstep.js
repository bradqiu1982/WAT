var WATSTEP = function () {

    var GetWatStep = function(){
        $(function () {
            $('#wafernum').focus();
        })

        function RetrieveStep() {
            var wafernum = $('#wafernum').val().trim();
            $('#wafernum').val('');
            $('#wafernum').focus();
            $('#sn').html(wafernum);                
            $('#normaltest').html('');
            $('#retest').html('');
            $('#status').html('');

            if (wafernum) {
                $.post('/Main/WATTestStepData',
                    { wafernum: wafernum },
                    function (output) {
                        $('#normaltest').html(output.normaltest);
                        $('#retest').html(output.retest);
                        $('#status').html(output.status);
                        if (output.status.indexOf('OK') != -1) {
                            $('#status').removeClass('error-info').removeClass('ok-info').addClass('ok-info');
                        }
                        else {
                            $('#status').removeClass('error-info').removeClass('ok-info').addClass('error-info');
                        }

                        $('#wafernum').focus();
                    });
            }
            return false;
        }

        $('body').on('click', '#btn-submit', function () {
            RetrieveStep();
        })

        $('body').on('keypress', '#wafernum', function (e) {
            if (e.keyCode == 13) {
                RetrieveStep();
            }
        })
    }

    return {
        WATStepInit: function () {
            GetWatStep();
        }
    }
}();