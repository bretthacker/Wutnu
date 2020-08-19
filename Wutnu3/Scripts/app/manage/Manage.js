﻿$(function () {
    if (location.hash.length > 0) {
        ManageMain.DoNav(location.hash);
    } else {
        location.hash = "#list";
    }

    //events
    $("#btnSaveUrl").on("click", function () {
        ManageMain.Save();
    });
    $("#btnDeleteUrl").on("click", function () {
        ManageMain.Delete();
    });

    $("#IsProtected").on("click", function (o) {
        ManageMain.CheckProtected();
    });
    var urlcheck = 0;
    $("#ShortUrl").on("keyup", function () {
        clearTimeout(urlcheck);
        urlcheck = setTimeout(function () {
            var val = $("#ShortUrl").val();
            var orgVal = $("#ShortUrl").data("orgValue");
            if (val === orgVal) return;

            $("#shortUrlStatus").removeClass("glyphicon-ok").removeClass("glyphicon-remove").addClass("glyphicon-time");
            ManageMain.CheckShortLink(val, function (res) {
                if (!res) {
                    SiteUtil.ShowMessage("That code is in use already - please choose another", "Invalid Short Link", SiteUtil.AlertImages.warning, 3000);
                    $("#ShortUrl").val(orgVal).focus();
                    $("#shortUrlStatus").removeClass("glyphicon-time").addClass("glyphicon-remove");
                    return;
                }
                var data = SiteUtil.GetDataObject("#EditLinkDialog div.modal-body");
                data.ShortUrl = val;
                $("#shortUrlStatus").removeClass("glyphicon-time").addClass("glyphicon-ok");
                ManageMain.SetNewShortUrl(data);
            });
        }, 500);
    });
    $(".listscroll")
        .on("mouseover mouseout", "div", function () {
            $(this).toggleClass("hover");
        });
    $('a[data-toggle="pill"]').on('shown.bs.tab', function (e) {
        location.hash = e.target.hash;
        //ManageMain.DoNav(e.target.hash);
    });
    $(window).on("hashchange", function (h) {
        ManageMain.DoNav(document.location.hash);
    });
    ManageMain.LoadLinks();
});

var ManageMain = function () {
    var doNav = function (hash) {
        $('a[data-toggle="pill"][href="' + hash + '"]').tab("show");
        switch (hash) {
            case "#list":
                loadLinks();
                break;
            case "#files":
                FileManager.LoadFiles();
                break;
            case "#users":
                UserManager.LoadUsers();
                break;
            case "#reports":
                ReportManager.LoadReports();
                break;
        }
        $(document.body).scrollTop(0);
    };
    var checkShortLink = function (link, callback) {
        SiteUtil.AjaxCall("/api/Url/IsUniqueShortLink", { shortLinkCandidate: link }, function (res) {
            callback(res);
        });
    };
    var getLinks = function (callback) {
        SiteUtil.AjaxCall("/api/Url/GetUrls", { count: 100 }, function (res) {
            callback(res);
        });
    };
    var linkTable;

    function loadLinkTable(res) {
        linkTable = $("#urllist").dataTable({
            destroy: true,
            paging: false,
            data: res,
            oLanguage: {
                "sEmptyTable": "There are no saved links yet"
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
                        if (o.CreateDate === undefined) return "";
                        return SiteUtil.UtcToLocal(o.CreateDate);
                    }
                },
                { data: "ShortUrl" },
                { data: "RealUrl" },
                {
                    data: function (o) {
                        return (o.IsProtected) ? "<span class='secure locked'>" : "<span class='secure unlocked'>";
                    }
                },
                { data: "Comments" }
            ]
        });
    }
    function loadLinks() {
        //initial call
        getLinks(function (res) {
            loadLinkTable(res);
        });
    }

    //helpers
    var showDetailDialog = function (data) {
        $("#ShortUrl").val(data.ShortUrl).data("orgValue", data.ShortUrl);
        setNewShortUrl(data);
        $("#RealUrl").val(data.RealUrl);
        $("#IsProtected")[0].checked = data.IsProtected;
        checkProtected();
        $("#Comments").val(data.Comments);
        $("#UserEmails").val(data.UserEmails);

        $("#EditLinkDialog").data("data", data).modal('show');
    };
    function setNewShortUrl(data) {
        var lnk = $("#testShortLink");
        var root = lnk.data("root");
        if (data.IsProtected) root += "a/";

        //lnk.attr("href", root + data.ShortUrl);
        lnk.val(root + data.ShortUrl);
    }
    function checkProtected() {
        $("#protectedInfo").css("display", ($("#IsProtected")[0].checked) ? "block" : "none");
    }
    function saveLink() {
        if ($("#shortUrlStatus").hasClass("glyphicon-remove")) {
            SiteUtil.ShowMessage("Please set a valid Short Link before saving", "Invalid Short Link", SiteUtil.AlertImages.error, 3000);
            $("#ShortUrl").val(orgVal).focus();
            return;
        }
        var data = $("#EditLinkDialog").data("data");
        data.ShortUrl = $("#ShortUrl").val();
        data.RealUrl = $("#RealUrl").val();
        data.IsProtected = $("#IsProtected")[0].checked;
        data.Comments = $("#Comments").val();
        data.UserEmails = $("#UserEmails").val();
        data.UserEmailColl = data.UserEmails.split(",");

        SiteUtil.AjaxCall("/api/Url/SaveUrl", data, function (res) {
            $("#EditLinkDialog").modal('hide');
            ManageMain.LoadLinkTable(res);
        }, "POST");
    }
    function deleteLink() {
        var data = SiteUtil.GetDataObject("#EditLinkDialog div.modal-body");
        SiteUtil.AjaxCall("/api/Url/DeleteUrl", data, function (res) {
            $("#EditLinkDialog").modal('hide');
            SiteUtil.ShowMessage("Link deleted", "Operation complete", SiteUtil.AlertImages.success);
            ManageMain.LoadLinkTable(res);
        }, "POST");
    }
    return {
        DoNav: doNav,
        LoadLinks: loadLinks,
        LoadLinkTable: loadLinkTable,
        CheckShortLink: checkShortLink,
        ShowDetailDialog: showDetailDialog,
        SetNewShortUrl: setNewShortUrl,
        CheckProtected: checkProtected,
        Save: saveLink,
        Delete: deleteLink
    };
}();