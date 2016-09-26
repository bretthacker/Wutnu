$(function () {
    $("#ReportList").on("click", "div", function (e) {
        var report = $(e.target).data("report-name");
        //debugger;
        $("#ReportViewer").load("/Manage/Reports/Run/" + report + "?d=" + new Date().valueOf());
    });
});

var ReportManager = function () {
    function loadReports() {
        getReports(function (res) {
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

    var getReports = function (callback) {
        SiteUtil.AjaxCall("/api/Report/GetReports", null, function (res) {
            callback(res);
        });
    }

    return {
        LoadReports: loadReports
    }
}();

/*

@foreach (var report in Model)
{
    <tr data-report-name="@report.ReportPath">
        <td>@report.CreateDate</td>
        <td>@report.ReportName</td>
        <td>@report.Description</td>
        <td>@report.ReportPath</td>
    </tr>
}
*/