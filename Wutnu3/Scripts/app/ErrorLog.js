$(function () {
    var getData = function (callback) {
        SiteUtil.AjaxCall("/api/ErrorLog/GetErrorItems", { count: 100 }, function (res) {
            callback(res);
        });
    }
    var errTable;

    //initial call
    getData(function (res) {
        errTable = $("#errorlist").dataTable({
            paging: false,
            data: res,
            oLanguage: {
                "sEmptyTable": "There are no errors in the log"
            },
            scrollCollapse: false,
            order: [],
            createdRow: function (row, data) {
                $(row)
                    .on("click", function () { showDetailDialog(data); })
                    .data("data", data);
            },
            columns: [
                {
                    data: function (o) {
                        if (o.ErrorDate == undefined) return "";
                        return SiteUtil.UtcToLocal(o.ErrorDate);
                    }
                },
                { data: "UserName" },
                { data: "ErrorSource" },
                {
                    data: function (o) {
                        var idx = o.ErrorMessage.indexOf("ExceptionType:");
                        idx = (idx > -1) ? idx : 150;
                        var s = o.ErrorMessage.substr(0, idx) + ((o.ErrorMessage.length <= idx) ? "" : "...");
                        if (o.ErrorMessage == "Debugging Message") s += " - <br>" + o.Message;
                        if (o.UserComment) s = "<span class='highlite'>(User message: \"" + o.UserComment + "\")</span><br>" + s;
                        return s;
                    }
                },
                {
                    data: function (o) {
                        if (o.URI == null) return "N/A";
                        var segment = o.URI.split("/");
                        return ".../" + unescape(segment.slice(-2).join("/"));
                    }
                }
            ]
        });
    });

    //events
    $(".listscroll")
        .on("mouseover mouseout", "div", function () {
            $(this).toggleClass("hover");
        });
    $(".btn-primary").on("click", function () {
        if (confirm("Remove this error?")) {
            var data = { ErrorId: $("#ErrorDialog").data("ErrorId") };

            SiteUtil.AjaxCall("/api/ErrorLog/DeleteErrorItem", data, function (res) {
                try {
                    parseInt(res.RecordCount);
                    SiteUtil.ShowMessage(res.RecordCount + " error(s) deleted.");
                    errTable.fnClearTable(true);
                    if (res.ErrorItems.length > 0) {
                        errTable.fnAddData(res.ErrorItems, true);
                    }
                    $('#ErrorDialog').modal('hide');
                } catch (e) {
                    throw new Error("Error deleting error record: " + JSON.parse(res));
                }
            },"DELETE");
        }
    });

    $(".btn-warning").on("click", function () {
        if (confirm("Remove ALL errors like this?")) {
            var data = { ErrorId: $("#ErrorDialog").data("ErrorId") };
            SiteUtil.AjaxCall("/api/ErrorLog/DeleteMatchingErrorItems", data, function (res) {
                try {
                    parseInt(res.RecordCount);
                    SiteUtil.ShowMessage(res.RecordCount + " error(s) deleted.");
                    errTable.fnClearTable(true);
                    if (res.ErrorItems.length > 0) {
                        errTable.fnAddData(res.ErrorItems, true);
                    }
                    $('#ErrorDialog').modal('hide');
                } catch (e) {
                    throw new Error("Error deleting error record: " + JSON.parse(res));
                }
            },"DELETE");
        }
    });

    //helpers
    var showDetailDialog = function (data) {
        var res = $("<div/>");
        var i = 0;
        for (col in data) {
            i++;
            var ds = "";
            var bg = (i % 2 == 0) ? "#fafafa;" : "";
            var d = $("<div/>").css("backgroundColor", bg).appendTo(res);
            if (data[col] != null) {
                if (col=='ErrorDate') {
                    ds = SiteUtil.UtcToServerAndLocal(data[col]);
                } else if (typeof data[col] == "object") {
                    var data2 = data[col];
                    for (var col2 in data2) {
                        ds += col2 + ": " + data2[col2] + "<br>";
                    }
                } else {
                    switch(col) {
                        case "Email":
                            ds = data[col].toString();
                            ds = "<a href='mailto:" + ds + "?subject=ClientNet Bug'>" + ds + "</a>";
                            break;
                        case "UserComment":
                            ds = "<span class='highlite'>" + data[col].toString().replace(/\r\n/g, "<br/>") + "</span>";
                            break;
                        default:
                            ds = data[col].toString().replace(/\r\n/g, "<br/>");
                    }
                }
                d.html("<span class='label'>" + SiteUtil.DeTC(col) + "</span><span class='data'>" + ds + "</span>");
            }
        }
        $(".modal-body").html(res.html());
        $("#ErrorDialog").data("ErrorId", data.ErrorId).modal('show');
    };
});
