var lTimezone, lTimezoneAbb, bSkipSBInit;

//global initializer
$(function () {
    var timezone = jstz.determine();
    lTimezone = timezone.name();
    lTimezoneAbb = moment().tz(lTimezone).zoneName();
    $.fx.speeds._default = 200;
});

$(document).ajaxComplete(function () {
    $(".ui-loader").hide();
});
$(document).ajaxStart(function () {
    $(".ui-loader").show();
});
var localErr = {};
//global events
window.onerror = function (msg, url, line, col, err) {
    $(".ajaxloader").hide();
    localErr.Message = msg + "\n  URL: " + url + "\n  Line: " + line + "\n  Col: " + col + "\n  Stack:\n  " + (((err) && (err.stack)) ? err.stack : "N/A");
    localErr.thrownError = "Global Catch";
    var txt = "A general web application error occured. Please <a href='javascript:popErrorUpdater();'>help us out</a> by supplying additional information.";

    SiteUtil.ShowMessage(txt.length > 0 ? txt : 'Unexpected error.', 'Error', SiteUtil.AlertImages.error);
    // If you return true, then error alerts (like in older versions of Internet Explorer) will be suppressed.
    return true;
};
$(document).ajaxError(function (event, xhr, ajaxOptions, thrownError) {
    if (xhr.status == 401) {
        //re-authenticate
        SiteUtil.ShowMessage("Sorry, your authentication token has expired. Please reload your keyed landing page.", "Authentication Expired", SiteUtil.AlertImages.warning);
        return;
    }
    if (typeof xhr.responseJSON == "object") {
        localErr = xhr.responseJSON;

        if (localErr.ErrorMessage != null && localErr.ErrorMessage.length > 0) {
            SiteUtil.ShowMessage(localErr.ErrorMessage, localErr.ErrorTitle, SiteUtil.AlertImages.warning);
            return;
        }

        localErr.thrownError = thrownError;
    } else {
        localErr.Message = xhr.responseText;
        localErr.thrownError = (thrownError || "Unknown");
    }
    var txt = "There was a problem with your request. Please <a href='javascript:popErrorUpdater();'>help us out</a> by supplying additional information.";
    SiteUtil.ShowMessage(txt.length > 0 ? txt : 'Unexpected error.', 'Error', SiteUtil.AlertImages.error);
});
function popErrorUpdater() {
    var err = localErr;
    localErr = {};
    var eid = err.DbErrorId || 0;

    var dialog = SiteUtil.ShowModal({
        body: "Please enter any additional information that might help us sort this out - what you were trying to do, what data you were manipulating, etc.:",
        title: "Additional Error Detail",
        callback: function (userComment) {
            bSkipSBInit = true;
            SiteUtil.AjaxCall("/api/UpdateError", { "Id": eid, "Comment": userComment, "Error": JSON.stringify(err) }, function () {
                $(dialog).modal("hide");
            }, "POST", "Error message updated; thanks!");
        },
        displayCallback: function () {
            $("#nDialogVal").focus();
        }
    });
}
function setHeaderTitle(s) {
    $("div[data-role=header] h1").html(s);
}
function notifyError(title, text) {
    _notify(title, text, SiteUtil.ErrorImages.Warning);
}
function notifySuccess(title, text) {
    _notify(title, text, SiteUtil.ErrorImages.Default);
}

var radioButton = {
    Get: function (radioName) {
        var $radio = $("input[name = " + radioName + "]");
        return $radio.filter(":checked").val();
    },
    Set: function (radioName, val) {
        var $radio = $("input[name = " + radioName + "]");
        if (val == null) val = "";
        $radio.each(function () {
            this.checked = (this.value.toString() == val.toString());
        });
        return $radio;
    }
};
$.extend($.gritter.options, {
    position: 'bottom-left',
    fade_in_speed: 250,
    fade_out_speed: 1000,
    time: 4000,
    class_name: 'gritter-light',
    sticky: false
});

function _notify(title, text, image) {
    $.gritter.add({
        title: title,
        text: text,
        image: (image || SiteUtil.ErrorImages.Default)
    });
}

function CallAPI(data, method, callback) {
    $.ajax({
        type: 'POST',
        data: JSON.stringify(data),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        url: '/api/' + method,
        success: function (res) {
            callback(res);
        }
    });
}
function GetJSONDate(sDate) {
    //return (sDate == "" || sDate == null || typeof sDate == "undefined") ? "" : eval(sDate.replace(/\/Date\((-?\d+)\)\//, "new Date($1)"));
    return Date.parse(sDate);
}

var SiteUtil = function () {
    function isValidEmailAddress(emailAddress) {
        var pattern = new RegExp(/^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i);
        return pattern.test(emailAddress);
    }
    var ErrorImages = {
        Warning: '/content/images/warning.png', 
        Default: '/content/images/info.png'
        };
    function _showModal(options) {
        options.okHide = (options.okHide == null) ? true : options.okHide;
        options.title = (options.title == null) ? "Message" : options.title;
        options.id = (options.id == null) ? "nAlertDialog" : options.id;
        var dialogId = "#" + options.id;
        var btn = {
            OK: function () {
                $(dialogId).modal("hide");
                return false;
            }
        };
        if (options.callback !== null) {
            if (options.callback.length > 0) {
                options.body += "<br><textarea rows='3' cols='50' style='margin-top:5px' id='nDialogVal' size='20'></textarea>";
            }
            btn.Cancel = function () {
                $(dialogId).modal("hide");
                return false;
            };
            btn.OK = function () {
                var res = $("#nDialogVal").val();
                options.callback(res);
                if (options.okHide) $(dialogId).modal("hide");
            };
        }
        var modal = _getModal({
            title: options.title,
            buttons: btn,
            id: options.id,
            body: options.body,
            modalClass: options.modalClass,
            displayCallback: options.displayCallback
        });
        $("#nDialogVal").focus();
        modal.setContent = function (body) {
            modal.body.html(body);
        };
        return modal;
    }
    function _getModal(opts) {
        var res = $("<div/>").addClass("modal fade").attr("id", opts.id || "nAlertDialog");
        var d = $("<div/>").addClass("modal-dialog").appendTo(res);
        if (opts.modalClass) {
            d.addClass(opts.modalClass);
        }
        var c = $("<div/>").addClass("modal-content").appendTo(d);
        var h = $("<div/>").addClass("modal-header").appendTo(c);
        $("<button/>").attr({ "type": "button", "data-dismiss": "modal", "aria-hidden": "true" }).addClass("close").html("&times;").appendTo(h);
        $("<h4/>").addClass("modal-title").html(opts.title || "Alert").appendTo(h);

        var body = $("<div/>").addClass("modal-body").appendTo(c);
        if (typeof opts.body == "object") {
            body.append(opts.body);
        } else {
            body.html(opts.body);
        }
        //body.append("<img src='/images/ajax-loader-dialog.gif' class='ajaxloader ajaxloader-dialog' />");

        if (opts.buttons) {
            var f = $("<div/>").addClass("modal-footer").appendTo(c);
            for (var button in opts.buttons) {
                $("<button/>").attr({ "type": "button" }).addClass("btn btn-default").on("click", opts.buttons[button]).html(button).appendTo(f);
            }
        }
        res.on("hidden.bs.modal", function () {
            $(this).remove();
        });
        res.body = body;
        return res.modal().on("shown.bs.modal", function () {
            if (opts.displayCallback) opts.displayCallback();
        });
    }
    function _utcToLocal(sDate, sInputFormatMask, sOutputFormatMask, bIncludeTZAbb) {
        bIncludeTZAbb = (bIncludeTZAbb == null) ? true : bIncludeTZAbb;
        sOutputFormatMask = sOutputFormatMask || 'MM/DD/YYYY h:mmA';
        var bIsUTC = (typeof sDate == "string" && sDate.indexOf("T") == 10);
        sInputFormatMask = (bIsUTC) ? null : (sInputFormatMask || 'MM/DD/YYYY HH:mmA');
        var res = moment.utc(sDate, sInputFormatMask).tz(lTimezone).format(sOutputFormatMask);
        if (bIncludeTZAbb) res += " " + lTimezoneAbb;
        return (res.indexOf("Invalid date") > -1) ? "N/A" : res;
    }
    function _getShortDate(sDate, sFormat) {
        sFormat = (sFormat || "MM/DD/YYYY");
        return SiteUtil.UtcToLocal(moment(sDate), null, sFormat, false)
    }

    function _utcToServerAndLocal(sDate, sInputFormatMask) {
        var bIsUTC = (typeof sDate == "string" && sDate.indexOf("T") == 10);
        sInputFormatMask = (bIsUTC) ? null : (sInputFormatMask || 'MM/DD/YYYY HH:mmA');
        var dte = moment.utc(sDate, sInputFormatMask);
        var serverDte = dte.format("MM/DD/YYYY h:mmA") + " UTC";
        var localDte = dte.local().format("MM/DD/YYYY h:mmA") + " " + lTimezoneAbb;
        var res = localDte + " (" + serverDte + ")";

        return (res.indexOf("Invalid date") > -1) ? "N/A" : res;
    }
    function _deTc(sTitle) {
        var re = /([a-z])([A-Z])/g;
        return sTitle.replace(re, "$1 $2");
    }
    function _ajaxCall(url, data, callback, method, successMessage) {
        method = (method == null) ? "GET" : method;
        successMessage = (successMessage == null) ? "" : successMessage;
        if (method !== "GET") {
            data = JSON.stringify(data);
        }
        $.ajax({
            url: url,
            data: data,
            type: method,
            withCredentials: true,
            contentType: "application/json",
            success: function (res, status, xhr) {
                if (successMessage.length > 0) {
                    //SiteUtil.ShowMessage(successMessage, "Success", SiteUtil.AlertImages.success);
                    notifySuccess("Success", successMessage);
                }
                if (callback) callback(res, xhr);
            }
        });
    }

    function getFormObjects(obj, excludeClasses) {
        obj = $(obj);
        excludeClasses = (excludeClasses == null) ? "" : "," + excludeClasses;
        var oForm = obj
            .find("input,textarea,select")
            .not(":submit, :button, :reset, :image, [disabled]" + excludeClasses);

        var radios = $.grep(oForm, function (el) { return el.type == "radio"; });
        var newForm = $.grep(oForm, function (el) { return el.type == "radio"; }, true);
        var newradios=radios.unique()
        //newradios = $.map(radios, function (o,i) {
        //    return ($.inArray(o.name, newradios) < 0) ? o.name : null;
        //});
        $(newradios).each(function (i,o) {
            var r = $('input[name=' + this + ']:checked');
            if (r.length == 0) r = $('input[name=' + this + ']');   //they're all false, so get all and will return the 1st one by default
            r = r.clone()[0];
            if (r.length == 0) $(r).val("");   //they're all false, so Set to "" to no changes made
            r.id = this;
            newForm.push(r);
        });
        return $(newForm);
    }

    function getDataObjectOrg(obj, fnMod) {
        var oForm = getFormObjects(obj);
        var sOut = "{";
        oForm.each(function () {
            var o = $(this).clone(true, true)[0];
            o.value = this.value;   //sanity check on the clone method, which appears to not do what I want on SELECT objects
            if ((o.tagName == "INPUT") && (o.type == "checkbox") && (!o.checked)) return;

            var s = $(o).val();
            if (s == "") return;
            sOut += '"' + o.id + '":"' + ((s == null) ? "" : s.replace(/"(?:[^"\\]|\\.)*"/g, "\{0}")) + '",';
        });
        sOut = sOut.substring(0, sOut.length - 1) + '}';
        try {
            return $.parseJSON(sOut);
        }
        catch (ex) {
            throw new Error("Unable to parse form value (" + ex.toString() + "): \n\r" + sOut);
        }
    }
    function getDataObject2(obj, fnMod) {
        var oForm = getFormObjects(obj);
        var oOut = {};
        oForm.each(function () {
            var o = $(this).clone(true, true)[0];
            o.value = this.value;   //sanity check on the clone method, which appears to not do what I want on SELECT objects
            if ((o.tagName == "INPUT") && (o.type == "checkbox")) {
                oOut[o.name] = o.checked;
                return;
            }
            var s = $(o).val();
            var val = ((s == null) ? "" : s.replace(/"(?:[^"\\]|\\.)*"/g, "\{0}"));
            if (oOut[o.name]) {
                if (oOut[o.name].constructor!==Array) {
                    var t = oOut[o.name];
                    oOut[o.name] = [];
                    oOut[o.name].push(t);
                }
                oOut[o.name].push(val);
            } else {
                oOut[o.name] = val;
            }
        });
        return oOut;
    }

    function getDataObject(obj, fnMod) {
        var oForm = getFormObjects(obj);
        var oOut = [];
        oForm.each(function () {
            var x = {};
            var o = $(this).clone(true, true)[0];
            o.value = this.value;   //sanity check on the clone method, which appears to not do what I want on SELECT objects
            if ((o.tagName == "INPUT") && (o.type == "checkbox") && (!o.checked)) return;
            x[o.id] = $(o).val();
            oOut.push(x);
        });
        return oOut;
    }
    var alertImages = {
        info: "info.png",
        error: "error.png",
        warning: "warning.png",
        success: "success.png"
    }
    function _showMessage(text, title, image, displayTimeMs) {
        title = title || "Information";
        image = image || alertImages.info;

        var msg = {
            title: title,
            text: text,
            image: '/content/images/' + image,
        };
        if (displayTimeMs) msg.time = displayTimeMs;
        $.gritter.add(msg);
    }
    var cookie = {
        Set: function (name, value, expires, path, domain, secure) {
            var res = name + "=" + escape(value) +
                ((expires == null) ? "" : ("; expires=" + expires.toGMTString())) +
                    ((path == null) ? "" : ("; path=" + path)) +
                        ((domain == null) ? "" : ("; domain=" + domain)) +
                            ((secure == null) ? "" : "; secure");  //always secure
            document.cookie = res;
        },
        Get: function (name) {
            var arg = name + "=";
            var alen = arg.length;
            var clen = document.cookie.length;
            var i = 0;
            while (i < clen) {
                var j = i + alen;
                if (document.cookie.substring(i, j) == arg) {
                    var endstr = document.cookie.indexOf(";", j);
                    if (endstr == -1) endstr = document.cookie.length;
                    return unescape(document.cookie.substring(j, endstr));
                }
                i = document.cookie.indexOf(" ", i) + 1;
                if (i == 0) break;
            }
            return null;
        },
        Clear: function (name) {
            cookie.Set(name, "", new Date("1/1/1970"));
        }
    };
    function getOrdinal(num) {
        var num2 = num.slice(-1);
        switch (num2) {
            case "1":
                return (num.slice(-2) == "11" ? "th" : "st");
            case "2":
                return (num.slice(-2) == "12" ? "th" : "nd");
            case "3":
                return (num.slice(-2) == "13" ? "th" : "rd");
            default:
                return "th";
        }
    }
    function addCommas(nStr) {
        nStr += '';
        x = nStr.split('.');
        x1 = x[0];
        x2 = x.length > 1 ? '.' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1)) {
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
        }
        return x1 + x2;
    }
    return {
        AddCommas: addCommas,
        GetOrdinal: getOrdinal,
        DeTC: _deTc,
        ShowModal: _showModal,
        GetModal: _getModal,
        ShowMessage: _showMessage,
        AlertImages: alertImages,
        Cookie: cookie,
        AjaxCall: _ajaxCall,
        ErrorImages: ErrorImages,
        UtcToLocal: _utcToLocal,
        UtcToServerAndLocal: _utcToServerAndLocal,
        GetShortDate: _getShortDate,
        GetFormObjects: getFormObjects,
        GetDataObject: getDataObject2,
        IsValidEmailAddress: isValidEmailAddress,
    };
}();
Array.prototype.unique = function() {
    var o = {}, i, l = this.length, r = [];
    for(i=0; i<l;i+=1) o[this[i].name] = this[i].name;
    for(i in o) r.push(o[i]);
    return r;
};
$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};