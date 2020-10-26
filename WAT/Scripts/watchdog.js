var WATCHDOG = function () {

    var WATDataMonitor = function () {
        var procworking = 0;
        var vo = new Audio('/Content/wwry.mp3');
        var t = 0;

        function loopfunction()
        {
            if (procworking == 0) {
                //set working flag
                procworking = 1;
                console.log('enter proc' + t);

                //do data checking
                $.post('/WATLogic/CheckWATTestDataUniformity', {
                }, function (output) {

                    if (output.waferlist.length > 0) {

                        $('#coupontab').empty();

                        $.each(output.waferlist, function (i, val) {
                            var appendstr = '<tr style="align-content:center">' +
                                            '<td style="font-size:18px!important;text-align:center">' + val.wafer + '</td>' +
                                            '<td style="font-size:18px!important;text-align:center">' + val.teststep + '</td>' +
                                            '</tr>';
                            $('#coupontab').append(appendstr);
                        });

                        //play music
                        vo.loop = true;
                        vo.play();

                        //show dialog
                        $('#watchdlg').modal('show');
                    }
                    else {
                        //set free flag
                        procworking = 0;
                    }
                });
            }
            else {
                //do nothing
                console.log('proc working' + t);
                t = t + 1;
            }
        }

        $(function () {
            //set interval
            setInterval(loopfunction, 10*60000);
        })

        $('body').on('click', '#btn-cmf', function () {
            //release all resource
            $('#watchdlg').modal('hide');
            procworking = 0;
            vo.pause();
            vo.currentTime = 0;
        });

    }

    var WDog = function () {
        var procworking = 0;
        var vo = new Audio('/Content/wwry.mp3');
        var t = 0;

        function loopfunction() {
            if (procworking == 0) {
                //set working flag
                procworking = 1;
                console.log('enter proc' + t);

                //do data checking
                $.post('/WATLogic/WDogDemo', {
                }, function (output) {

                    if (output.waferlist.length > 0) {

                        $('#coupontab').empty();

                        $.each(output.waferlist, function (i, val) {
                            var appendstr = '<tr style="align-content:center">' +
                                            '<td style="font-size:18px!important;text-align:center">' + val.wafer + '</td>' +
                                            '<td style="font-size:18px!important;text-align:center">' + val.teststep + '</td>' +
                                            '</tr>';
                            $('#coupontab').append(appendstr);
                        });

                        //play music
                        vo.loop = true;
                        vo.play();

                        //show dialog
                        $('#watchdlg').modal('show');
                    }
                    else {
                        //set free flag
                        procworking = 0;
                    }
                });
            }
            else {
                //do nothing
                console.log('proc working' + t);
                t = t + 1;
            }
        }

        $(function () {
            //set interval
            setInterval(loopfunction, 30000);
        })

        $('body').on('click', '#btn-cmf', function () {
            //release all resource
            $('#watchdlg').modal('hide');
            procworking = 0;
            vo.pause();
            vo.currentTime = 0;
        });

    }

    return {
        WATDATA: function () {
            WATDataMonitor();
        },
        DEMO: function () {
            WDog();
        }
    }
}();