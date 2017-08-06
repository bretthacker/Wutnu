$(function () {
    $("#ReportList").on("click", "div", function (e) {
        var report = $(e.target).data("report-name");
        $("#ReportViewer").load("/Manage/Reports/Run/" + report + "?d=" + new Date().valueOf());
    });
});

var ReportManager = function () {
    function loadReports() {
        SiteUtil.AjaxCall("/api/Report/GetReports", null, function (res) {
            var r = $("#ReportList");
            if (res.length == 0) {
                r.html("No reports are available or have been assigned");
                return;
            }
            r.html("");
            $(res).each(function (i) {
                $("<div>").data("report-name", this.ReportPath).html(this.ReportName + " (" + this.Description + ")").appendTo(r);
            });
        });
    }

    return {
        LoadReports: loadReports
    }
}();
