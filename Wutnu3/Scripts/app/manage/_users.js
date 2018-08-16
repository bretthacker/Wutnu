$(function () {

});

var UserManager = function () {

    function loadUsers() {
        getUsers(function (res) {
            loadUserTable(res);
        });
    }
    var userTable;

    function loadUserTable(res) {
        userTable = $("#userlist").dataTable({
            destroy: true,
            paging: false,
            data: res,
            oLanguage: {
                "sEmptyTable": "There are no assigned users yet"
            },
            scrollCollapse: false,
            order: [],
            createdRow: function (row, data) {
                $(row).on("click", "td:first", function () {
                    var d = $(this).parent("tr").data().data;
                    UserManager.DeleteUser(d, function (res) {
                        SiteUtil.ShowMessage("User access removed", "Operation complete", SiteUtil.AlertImages.success);
                    });
                });

                var sLinks = "";
                $(data.AssignedLinks).each(function (i, o) {
                    sLinks += o.RealUrl + "\n\r";
                });
                $(row)
                    .data("data", data)
                    .attr("title", sLinks);
            },
            columns: [
                {
                    width: "5%",
                    orderable: false,
                    className: "center",
                    data: function (o) {
                        return ('<span title="Click to delete" class="glyphicon glyphicon-remove"></span>');
                    }
                },
                { width:"70%", data: "PrimaryEmail" },
                {
                    width:"25%",
                    data: function (o) {
                        return (o.AssignedLinks.length);
                    }
                }
            ]
        });
    }
    var deleteUser = function (data, callback) {
        if (!confirm("Remove this user's access to all assigned links?")) return;

        SiteUtil.AjaxCall("/api/Profile/DeleteUser", data, function (res) {
            loadUserTable(res);
        },"POST");
    }

    var getUsers = function (callback) {
        SiteUtil.AjaxCall("/api/Profile/GetUsers", null, function (res) {
            callback(res);
        });
    }

    return {
        LoadUsers: loadUsers,
        DeleteUser: deleteUser
    }
}();