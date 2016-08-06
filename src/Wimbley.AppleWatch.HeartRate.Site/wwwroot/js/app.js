/*
    App specific functionality intalization
*/
var App = function () {

    function initDropZone (){
        $("#dz-upload").dropzone({
            url: "/HeartRate/UploadFile",
            autoProcessQueue: true,
            paramName: "file",
            uploadMultiple: true,
            parallelUploads: 20,
            previewTemplate: '<div class="dz-preview dz-file-preview"><div class="dz-details"><div class="dz-size" data-dz-size></div><div class="dz-filename"><span data-dz-name></span></div><img class="dz-image" data-dz-thumbnail /></div><div class="dz-progress"><i class="icon-spinner icon-spin"></i></div></div>',
            init: function() {
                var that = this;
                
                that.on("sending", function(file, xhr, formData) {
                    $("#msg").html("");
                    $("#hr-data").html("");
                    $(".highchart-container").html("");
                });

                that.on("success", function(file, response) {
                    if (response.msg === "SUCCESS") {
                        $("#msg").html("<div class='alert alert-success alert-dismissable'><a href='#' class='close' data-dismiss='alert' aria-label='close'>&times;</a>Data uploaded successfully</div>");

                        renderHrDataToTable(response.data);
                        initHrCharts(response.data);
                        
                    } else {
                        var errormsgs = $.isArray(response.errors) ? response.errors : [response.errors];

                        var displayerrors = "";
                        $.each(errormsgs, function(k, v) {
                            displayerrors += "<div class='alert alert-danger'>" + v + "</div>";
                        });

                        $("#msg").html(displayerrors);
                    }
                });

                that.on("complete", function(file) {
                    this.removeAllFiles();
                });

                that.on("error", function(file, response) {

                });
            }
        });
    }

    function initHrCharts(data) {
        // Highcharts can't handle complex data, reformat it so it will cooperate.
        var seriesData = [];
        var categories = [];

        for (var i = 0; i < data.length; i++) {
            var recordedDate = moment(data[i].recordedDate).format("MM/DD/YYYY h:mm:ss a");
            seriesData.push(parseInt(data[i].value));
            categories.push(recordedDate);
        }

        $(".highchart-container").highcharts({
            chart: {
                zoomType: 'x'
            },
            title: {
                text: 'Heart rate over time'
            },
            subtitle: {
                text: document.ontouchstart === undefined ? 'Click and drag in the plot area to zoom in' : 'Pinch the chart to zoom in'
            },
            xAxis: {
                type: 'datetime',
                categories: categories
            },
            yAxis: {
                title: {
                    text: 'Heart Rate'
                }
            },
            legend: {
                enabled: false
            },
            plotOptions: {
                area: {
                    fillColor: {
                        linearGradient: {
                            x1: 0,
                            y1: 0,
                            x2: 0,
                            y2: 1
                        },
                        stops: [
                            [0, Highcharts.getOptions().colors[0]],
                            [1, Highcharts.Color(Highcharts.getOptions().colors[0]).setOpacity(0).get('rgba')]
                        ]
                    },
                    marker: {
                        radius: 2
                    },
                    lineWidth: 1,
                    states: {
                        hover: {
                            lineWidth: 1
                        }
                    },
                    threshold: null
                }
            },
            series: [{
                type: 'line',
                name: 'Heart Rate',
                data: seriesData
            }]
        });
    }

    function renderHrDataToTable(data) {
        var hrdata = "<table class='table table-bordered table-responisve table-hover table-striped'><thead>";
        hrdata += "<tr>";
        hrdata += "<th>Source</th>";
        hrdata += "<th>Time</th>";
        hrdata += "<th>Heart Rate</th>";
        hrdata += "</tr>";
        hrdata += "</thead><tbody>";

        $.each(data, function (k, v) {
            var recordedDate = moment(v.recordedDate).format("MM/DD/YYYY h:mm:ss a");

            hrdata += "<tr>";
            hrdata += "<td>" + v.sourceName + "</td>";
            hrdata += "<td>" + recordedDate + "</td>";
            hrdata += "<td>" + v.value + "</td>";
            hrdata += "</tr>";
        });

        hrdata += "</tbody></table>";

        $("#hr-data").html(hrdata);
    }

    return {
        init: function () {
            initDropZone();
        }
    };
}();
