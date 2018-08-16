$(function () {
    $("#btnUpload").on("click", function () {
        $("#UploadDialog").modal('show');
    });
    $("#btnRefresh").on("click", function () {
        FileManager.LoadFiles();
    });
    $("#btnShortenFile").on("click", function () {
        var url = $("#EditFileDialog").data("data").Uri;
        FileManager.Shorten(url, function (res) {
            $("#EditFileDialog").modal('hide');
            window.location.hash = "list";
        });
    });

    $("#btnOpenTestLink").on("click", function () {
        window.open($("#testShortLink").val());
    });

    $("#btnCopyTestLink").on("click", function () {
        if (SiteUtil.Copy($("#testShortLink").val())) {
            SiteUtil.ShowMessage("Link copied to clipboard", "Copy Successful", SiteUtil.AlertImages.success);
        } else {
            SiteUtil.ShowMessage("Link not copied to clipboard, your browser may not support this operation. Please Ctrl-C to copy.", "Copy Unsuccessful", SiteUtil.AlertImages.warning);
        }
    });
    $("#btnDeleteFile").on("click", function () {
        if (!confirm("Are you sure you want to delete this file?")) return;

        var url = $("#EditFileDialog").data("data").Uri;
        FileManager.Delete(url, function (res) {
            $("#EditFileDialog").modal('hide');
            SiteUtil.ShowMessage("File deleted", "Operation complete", SiteUtil.AlertImages.success);

            FileManager.LoadFiles();
        });
    });
    var progItem = {};
    FileManager.Uploader = new qq.azure.FineUploader({
        element: document.getElementById('fine-uploader'),
        blobProperties: {
            name: 'filename'
        },
        cors: {
            expected: true,
            sendCredentials: true
        },
        chunking: {
            enabled:true
        },
        request: {
            endpoint: ''
        },
        signature: {
            endpoint: '/api/File/GetUploadSAS'
        },
        uploadSuccess: {
            endpoint: '/success'
        },
        retry: {
            enableAuto: true
        },
        deleteFile: {
            enabled: true
        },
        callbacks: {
            onComplete: function (id, name, responseJSON, xhr) {
                FileManager.Uploader.clearStoredFiles();
                FileManager.LoadFiles();
                $("#UploadDialog").modal('hide');
                clearInterval(progItem.interval);
                SiteUtil.ShowMessage("Upload complete, transfer rate " + progItem.rate + " mbps.")
            },
            onError: function (id, name, errorReason, xhr) {
                console.error("upload error: " + errorReason);
            },
            onProgress: function (id, name, uploadedBytes, totalBytes) {
                var elapsedSecs = (new Date() - progItem.startTime) / 1000;
                progItem.rate = ((uploadedBytes / elapsedSecs) / 1024 / 1024).toFixed(2);
                console.log(progItem.rate);
                if (progItem.interval != 0) {
                    progItem.interval = setInterval(function () {
                        $("#UploadRate").html("(" + progItem.rate + " mbps)");
                    }, 500);
                }
            },
            onUpload: function (id, name) {
                progItem.id = id;
                progItem.startTime = new Date();
                proItem.interval = 0;
            }
        }
    });
});

var FileManager = function () {
    var fileTable;
    var uploader;
    //uploader.request.endpoint

    //helpers
    var showDetailDialog = function (data) {
        var div = $("<div/>");

        $("<div/>").html("File Name: ").appendTo(div);
        $("<div/>").addClass("dialogData").html(data.Name).appendTo(div);

        $("<div/>").html("File Type: ").appendTo(div);
        $("<div/>").addClass("dialogData").html(data.Properties.ContentType).appendTo(div);

        $("<div/>").html("Size: ").appendTo(div);
        var sz = getFileSize(data);
        $("<div/>").addClass("dialogData").html(sz).appendTo(div);

        $("<div/>").html("Link: ").appendTo(div);
        $("<div/>").addClass("dialogData").html(data.Uri).appendTo(div);

        $("#EditFileDialog div.modal-body").html("").append(div);
        $("#EditFileDialog").data("data", data).modal('show');
    };

    function loadFileTable(res) {
        fileTable = $("#filelist").dataTable({
            destroy: true,
            paging: false,
            data: res,
            oLanguage: {
                "sEmptyTable": "There are no files yet"
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
                        if (o.Properties.LastModified === undefined) return "";
                        return SiteUtil.UtcToLocal(o.Properties.LastModified);
                    }
                },
                { data: "Name" },
                {
                    data: function (o) {
                        var sz = getFileSize(o);
                        return sz;
                    }
                },
            ]
        });
    }

    function loadFiles() {
        getFiles(function (res) {
            loadFileTable(res);
        });
    }
    function getFileSize(data) {
        return SiteUtil.AddCommas((data.Properties.Length > 1000000) ? (data.Properties.Length / 1024 / 1024).toFixed(2) + "MB" : (data.Properties.Length / 1024).toFixed(2) + "KB");
    }
    var getFiles = function (callback) {
        SiteUtil.AjaxCall("/api/File/GetFiles", null, function (res) {
            callback(res);
        });
    }
    var shortenUrl = function (longUrl, callback) {
        var data = {"realUrl": longUrl, "isBlob": true }
        SiteUtil.AjaxCall("/api/Url/CreateUrl", data, function (res) {
            callback(res);
        }, "POST");
    }
    var deleteFile = function (filePath, callback) {
        var data = { "bloburi": filePath }
        SiteUtil.AjaxCall("/api/File/DeleteBlob", data, function (res, xhr) {
            callback(res, xhr);
        }, "POST");
    }

    return {
        LoadFiles: loadFiles,
        Uploader: uploader,
        Shorten: shortenUrl,
        Delete: deleteFile
    }
}();