if (!window.console) { window.console = {}; }
if (!console.log) { console.log = function () { }; }
var $jq = jQuery.noConflict();
var DELAY = 300, clicks = 0, timer = null;

$jq(document).ready(function () {
    $jq('.main-menu.selected').on("click", function (e) {
        clicks++;  //count clicks
        if (clicks === 1) {
            timer = setTimeout(function () {
                //console.log("Single Click");  //perform single-click action  
                $jq('#pnlSubmenu').toggleClass("show");
                if ($jq('#pnlSubmenu').is(':visible')) {
                    $jq('.content-panel').css("margin-top", "130px");
                } else {
                    $jq('.content-panel').css("margin-top", "65px");
                }
                clicks = 0;             //after action performed, reset counter
            }, DELAY);
        }
        else {
            clearTimeout(timer);    //prevent single-click action
            //console.log("Double Click");  //perform double-click action
            var targetUrl = $jq(this).find('a').data('url');
            location.replace(targetUrl);
            clicks = 0;             //after action performed, reset counter
        }
    })
        .on("dblclick", function (e) {
            e.preventDefault();  //cancel system double-click event
        });
});

String.format = function (text) {
    //check if there are two arguments in the arguments list
    if (arguments.length <= 1) {
        //if there are not 2 or more arguments there's nothing to replace
        //just return the original text
        return text;
    }
    //decrement to move to the second argument in the array
    var tokenCount = arguments.length - 2;
    for (var token = 0; token <= tokenCount; token++) {
        //iterate through the tokens and replace their placeholders from the original text in order
        text = text.replace(new RegExp("\\{" + token + "\\}", "gi"), arguments[token + 1]);
    }
    return text;
};

String.prototype.trim = function () {
    return this.replace(/^\s+|\s+$/g, "");
};

(function () {
    if (!document.getElementsByClassName) {
        var indexOf = [].indexOf || function (prop) {
            for (var i = 0; i < this.length; i++) {
                if (this[i] === prop) return i;
            }
            return -1;
        };
        getElementsByClassName = function (className, context) {
            var elems = document.querySelectorAll ? context.querySelectorAll("." + className) : (function () {
                var all = context.getElementsByTagName("*"),
                    elements = [],
                    i = 0;
                for (; i < all.length; i++) {
                    if (all[i].className && (" " + all[i].className + " ").indexOf(" " + className + " ") > -1 && indexOf.call(elements, all[i]) === -1) elements.push(all[i]);
                }
                return elements;
            })();
            return elems;
        };
        document.getElementsByClassName = function (className) {
            return getElementsByClassName(className, document);
        };

        try {
            Element.prototype.getElementsByClassName = function (className) {
                return getElementsByClassName(className, this);
            };
        } catch (e) { }
    }
})();

function getUrlParameter(url, sParam) {
    var sPageURL = url.split('?')[1];
    var sURLVariables = sPageURL.split('&');
    for (var i = 0; i < sURLVariables.length; i++) {
        var sParameterName = sURLVariables[i].split('=');
        if (sParameterName[0] == sParam) {
            return sParameterName[1];
        }
    }
}

function handleAjaxError(xhr) {
    var handleMsg = SYSTEM_ERROR_MSG;
    ajaxErrorHandling(xhr, handleMsg, url_Login, url_AccessDenied);
}

function ajaxErrorHandling(xhr, errMessage, loginUrl, accessDeniedUrl) {
    //console.log('errMessage: ' + errMessage);
    if (xhr.status === 401) {
        topLocation(loginUrl);
        return;
    } else if (xhr.status === 403) {
        topLocation(accessDeniedUrl);
        return;
    } else {
        //console.log(errMessage);
        doModal('dvAlertMsg', 'Message Dialog', String.format('<strong>Error:</strong>&nbsp;{0}', errMessage), '', '');
    }
}

//function validateDigit(e) {
//    var valid = true;
//    //var keyCode = (window.event) ? window.event.keyCode : e.which;
//    var keyCode = e.charCode ? e.charCode : (window.event) ? window.event.keyCode : e.which;

//    // Allow: backspace, delete, tab
//    if (keyCode == 8 || keyCode == 9) {
//        // let it happen, don't do anything
//        valid = true;
//    }
//    else {
//        // Ensure that it is a number and stop the keypress
//        if ((keyCode < 48 || keyCode > 57)) {
//            //keyCode = 0;
//            valid = false;
//        }
//    }
//    return (valid);
//}

function setupAllowNumberInputOnly(element) {
    // digit only
    element.on('keypress', validateDigit);
    // disable paste
    disableInputPasteEvent(element);
}

function disableInputPasteEvent(element) {
    element.on('paste', function (e) {
        e.preventDefault();
    });
}

function bindRegexOnElementKeypress(element, regExStr) {
    var regex = new RegExp(regExStr);
    element.on("keypress", function (event) {
        var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
        if (!regex.test(key)) {
            event.preventDefault();
            return false;
        }
    });
    // disable paste
    disableInputPasteEvent(element);
}

function validateDigit(e) {
    var valid = true;
    //var keyCode = (window.event) ? window.event.keyCode : e.which;
    var keyCode = e.charCode ? e.charCode : (window.event) ? window.event.keyCode : e.which;

    // Allow: backspace, delete, tab
    if (keyCode == 8 || keyCode == 9) {
        // let it happen, don't do anything
        valid = true;
    }
    else {
        // Ensure that it is a number and stop the keypress
        if ((keyCode < 48 || keyCode > 57)) {
            //keyCode = 0;
            valid = false;
        }
    }
    return (valid);
}

function redirectToUrl(url) {
    window.location = url;
}

function doAlertModal(elementId, header, body, onOkString) {
    var html = '<div id="modalWindow" class="modal fade">';
    html += '<div class="modal-dialog">';
    html += '<div class="modal-content">';
    html += '<div class="modal-header">';
    html += '<h4>' + header + '</h4>';
    html += '</div>';
    html += '<div class="modal-body">';
    html += '<p>';
    html += body;
    html += '</p>';
    html += '</div>';
    html += '<div class="modal-footer">';
    html += '<span class="btn btn-blue btn-xsmall btn-sm"';
    html += ' onClick="' + onOkString + '"> Ok';
    html += '</span>';
    html += '</div>';  // content
    html += '</div>';  // dialog
    html += '</div>';  // footer
    html += '</div>';  // modalWindow
    $jq("#" + elementId).html(html);
    $jq("#modalWindow").modal('show');
}

function doModal(placementId, heading, formContent, strSubmitFunc, btnText) {
    hideModal(placementId);

    var html = '<div id="modalWindow" class="modal fade" data-backdrop="static" data-keyboard="false">';
    html += '<div class="modal-dialog">';
    html += '<div class="modal-content">';
    html += '<div class="modal-header">';
    html += '<a class="close" data-dismiss="modal">×</a>';
    html += '<h4>' + heading + '</h4>';
    html += '</div>';
    html += '<div class="modal-body">';
    html += '<p>';
    html += formContent;
    html += '</p>';
    html += '</div>';
    html += '<div class="modal-footer">';
    if (btnText != '') {
        html += '<span class="btn btn-blue btn-xsmall btn-sm"';
        html += ' onClick="' + strSubmitFunc + '">' + btnText;
        html += '</span>';
    }
    html += '<span class="btn btn-gray btn-xsmall btn-sm" data-dismiss="modal">';
    html += btnText != '' ? 'Cancel' : 'Close';
    html += '</span>'; // close button
    html += '</div>';  // content
    html += '</div>';  // dialog
    html += '</div>';  // footer
    html += '</div>';  // modalWindow
    $jq("#" + placementId).html(html);
    $jq("#modalWindow").modal('show');
}

function hideModal(placementId) {
    // Using a very general selector - this is because $('#modalDiv').hide
    // will remove the modal window but not the mask
    $jq('#' + placementId).modal('hide');
    $jq('.modal-backdrop').remove();
}

function showServiceErrors(result) {
    if (result.RedirectUrl != undefined) {
        topLocation(result.RedirectUrl);
        return;
    }

    if (result.Error != '') {
        var errorMsg = "<span class='badge badge-warning'><strong>Error:</strong>&nbsp;" + result.Error + '</span>';
        $jq('#dvServiceMsg').html(errorMsg);
        scrollToTop();
    }
    else {
        var divCtrl;
        var spanCtrl;
        var inputCtrl;

        $jq.each(result.Errors, function (key, val) {
            var controlId = key.replace(/\./g, '_');
            inputCtrl = $jq('[id$=' + controlId + ']');

            if (inputCtrl.length > 0) {
                if (inputCtrl.parent().hasClass('date')) {
                    spanCtrl = inputCtrl.parent().parent().find('span.field-validation-valid');
                } else {
                    spanCtrl = inputCtrl.parent().find('span.field-validation-valid');
                }

                inputCtrl.addClass('input-validation-error');
                spanCtrl.html(val).removeClass('field-validation-valid').addClass('field-validation-error');
            }

            divCtrl = $jq('.' + controlId);
            if (divCtrl.length > 0) {
                divCtrl.parent().find('input').addClass('input-validation-error');
                divCtrl.html(val).removeClass('field-validation-valid').addClass('field-validation-error');
            }
        });
    }
}

function showLoading() {
    $jq("#dvLoading").show();
}

function hideLoading() {
    $jq("#dvLoading").hide();

}

function showServerErrors(result) {
    if (result.RedirectUrl != undefined) {
        topLocation(result.RedirectUrl);
        return;
    }

    if (result.Error != '') {
        var errorMsg = "<strong>Error:</strong>&nbsp;" + result.Error;
        doModal('dvAlertMsg', 'Message Dialog', errorMsg, '', '');
        scrollToTop();
    }
    else {
        var divCtrl;
        var spanCtrl;
        var inputCtrl;

        $jq.each(result.Errors, function (key, val) {
            var controlId = key.replace(/\./g, '_');
            inputCtrl = $jq('[id$=' + controlId + ']');

            if (inputCtrl.length > 0) {
                if (inputCtrl.parent().hasClass('date')) {
                    spanCtrl = inputCtrl.parent().parent().find('span.field-validation-valid');
                } else {
                    spanCtrl = inputCtrl.parent().find('span.field-validation-valid');
                }

                inputCtrl.addClass('input-validation-error');
                spanCtrl.html(val).removeClass('field-validation-valid').addClass('field-validation-error');
            }

            divCtrl = $jq('.' + controlId);
            if (divCtrl.length > 0) {
                divCtrl.parent().find('input').addClass('input-validation-error');
                divCtrl.html(val).removeClass('field-validation-valid').addClass('field-validation-error');
            }
        });
    }
}

function toggelElementValidationError(element, valid) {
    if (valid) {
        element.removeClass("input-validation-error");
    } else {
        element.addClass("input-validation-error");
    }
}

function clearServerErrors() {
    $jq('.input-validation-error').removeClass('input-validation-error');
    $jq('.field-validation-error').addClass('field-validation-valid').removeClass('field-validation-error');
}

function scrollToTop() {
    $jq("html, body").animate({ scrollTop: '0px' }, 'slow');
}

function logout() {
    document.getElementById('logoutForm').submit();
}

function checkSpecialKeys(e) {
    if (e.keyCode != 8 && e.keyCode != 46
        && e.keyCode != 37 && e.keyCode != 38
        && e.keyCode != 39 && e.keyCode != 40) {
        return false;
    } else {
        return true;
    }
}

function checkTextAreaMaxLength(textBox, e, length) {
    var mLen = textBox["MaxLength"];
    if (null == mLen) {
        mLen = length;
    }
    var maxLength = parseInt(mLen, 10);
    if (!checkSpecialKeys(e)) {
        if (textBox.value.length > maxLength - 1) {
            if (window.event) { //IE
                e.returnValue = false;
            } else { //Firefox 
                e.preventDefault ? e.preventDefault() : e.returnValue = false;
            }
        }
    }
}

function stringLimit(textBox, str, maxChars) {
    var inputStr = textBox.value;
    textBox.value = inputStr.substring(0, maxChars);
}

function validateDateValue(control) {
    $jq('#dvAlertMsg').html('');
    var selectDate = control.val();
    if (selectDate != '')
        selectDate = setDateFormat(selectDate);
    control.val(selectDate);
    control.parent().datepicker("update");
}

function initializeDatePicker(elementIdentifier, onDateChange, endDate) {
    var dateFormat = 'dd/mm/yyyy';
    var end = endDate != null && endDate != undefined ? endDate: "0d";
    $jq(elementIdentifier).datepicker({
        format: dateFormat,
        endDate: end,
        orientation: 'auto top',
        showOnFocus: "false",
        language: 'th'
    }).on('changeDate', onDateChange);
}

function dateToString_MMddyyyy(d, splitter) {
    var day = getPadZeroDateTime(d.getDate());
    var month = getPadZeroDateTime(d.getMonth() + 1);
    var year = d.getFullYear();
    var date = month  + splitter + day + splitter + year;
    return date;
}

function convertDateToString_ddMMyyyy(d, dateSplitter) {
    var day = getPadZeroDateTime(d.getDate());
    var month = getPadZeroDateTime(d.getMonth() + 1);
    var year = d.getFullYear();
    var date = day + dateSplitter + month + dateSplitter + year;
    return date;
}

function convertDateTimeToString_HHmmss(d, timeSplitter) {
    var hr24 = getPadZeroDateTime(d.getHours());
    var minutes = getPadZeroDateTime(d.getMinutes());
    var seconds = getPadZeroDateTime(d.getSeconds());
    var time = hr24 + timeSplitter + minutes + timeSplitter + seconds;
    return time;
}

function getPadZeroDateTime(inputNumber) {
    if (inputNumber < 10) {
        inputNumber = "0" + inputNumber;
    }
    return inputNumber;
}

function setDateFormat(date) {
    var no_error = true;
    var strArray = date.split('/');
    var formatDate;
    if (strArray.length != 3) {
        if (date.length != 8)
            no_error = false;
        else
            formatDate = date.substring(0, 2) + '/' + date.substring(2, 4) + '/' + date.substring(4, 8);
    }
    else {
        if (strArray[0].length == 1)
            strArray[0] = '0' + strArray[0];
        if (strArray[1].length == 1)
            strArray[1] = '0' + strArray[1];
        if (strArray[0].length > 2 || strArray[1].length > 2)
            no_error = false;
        else
            formatDate = strArray[0] + '/' + strArray[1] + '/' + strArray[2];
    }

    if (!no_error) {
        doModal('dvAlertMsg', 'Message Dialog', 'กรุณากรอกวันที่ในรูปแบบ ddmmyyyy หรือ dd/mm/yyyy', '', '');
        return '';
    }

    no_error = no_error && validateDateFormat(formatDate);

    if (!no_error) {
        doModal('dvAlertMsg', 'Message Dialog', 'กรุณาระบุวันที่ให้ถูกต้อง', '', '');
        return '';
    }
    else {
        if (validateYearRang(formatDate)) {
            return formatDate;
        } else {
            return '';
        }
    }
}

function validateDateFormat(date) {
    var no_error = true;
    var strArray = date.split('/');
    if (strArray[1] > 12 || strArray[1] < 1 || strArray[0] < 1) {
        no_error = false;
    }
    //ตรวจสอบเดือนที่มี 31 วัน
    else if (strArray[1] == 1 || strArray[1] == 3 || strArray[1] == 5 || strArray[1] == 7 || strArray[1] == 8
        || strArray[1] == 10 || strArray[1] == 12) {
        no_error = (strArray[0] <= 31);
    }
    // ตรวจสอบเดือนที่มี 30 วัน
    else if (strArray[1] == 4 || strArray[1] == 6 || strArray[1] == 9 || strArray[1] == 11) {
        no_error = (strArray[0] <= 30);
    }
    //ตรวจสอบ leap year สำหรับเดือน กพ.
    else if (new Date(strArray[2], 1, 29).getMonth() == 1) {
        no_error = (strArray[0] <= 29);
    }
    else {
        no_error = (strArray[0] <= 28);
    }

    return no_error;
}
// Same function as function validateDateFormat
function validateRegexParsedDate(inputString, resultDate) {
    // Regex normally auto resut date:
    //    '31/01/2099'(31 Nov 2099) to 1 Dec 2099 
    // or '29/01/2017'(29 Feb 2017) to 1 Mar 2017
    // This validation check if auto date reset above happenned
    var digits = inputString.replace(new RegExp('/', 'g'), "");
    var month = parseInt(digits.substring(2, 4));
    var resultMonth = resultDate.getMonth() + 1;
    return month == resultMonth;
}

function tryConvertStringToDate(inputString) {
    var invalidDate = "Invalid Date";
    // format: dd/MM/yyyy
    var resultDate = new Date(inputString.replace(/(\d{2})\/(\d{2})\/(\d{4})/, "$2/$1/$3"));
    if (resultDate == invalidDate) {
    // format: ddMMyyyy
        resultDate = new Date(inputString.replace(/(\d{2})(\d{2})(\d{4})/, "$2/$1/$3"));
    }
    return resultDate;
}

function validateParsedDate(inputString, resultDate) {
    if (resultDate == "Invalid Date") {
        // date format
        doModal('dvAlertMsg', 'Message Dialog', 'กรุณากรอกวันที่ในรูปแบบ ddmmyyyy หรือ dd/mm/yyyy', '', '');
        return false;
    }
    if (!validateRegexParsedDate(inputString, resultDate)) {
        // impossible dates like 30Feb or 31Nov
        doModal('dvAlertMsg', 'Message Dialog', 'กรุณาระบุวันที่ให้ถูกต้อง', '', '');
        return false;
    }
    // validate year range
    var today = new Date();
    var thisYear = today.getFullYear();
    var targetYear = resultDate.getFullYear();
    var maxYear = thisYear + 50;
    var minYear = thisYear - 50;
    if (targetYear > maxYear || targetYear < minYear) {
        doModal('dvAlertMsg', 'Message Dialog', 'กรุณาระบุข้อมูล ปี ค.ศ. ในช่วง ' + minYear + ' ถึง ' + maxYear + '', '', '');
        return false;
    }

    return true;
}

function validateYearRang(date) {
    //ไว้สำหรับยกเว้นการตรวจปีของการกรอกข้อมูลที่ไม่รู้วันที่
    if (date == '09/09/9999' || date == '01/01/2999') {
        return true;
    }

    var today = new Date();
    var strArray = date.split('/');

    var currentYear = parseInt(strArray[2], 10);
    var maxYear = parseInt(today.getFullYear(), 10) + 50;
    var minYear = parseInt(today.getFullYear(), 10) - 50;
    if (currentYear <= maxYear && currentYear >= minYear) {
        return true;
    } else {
        doModal('dvAlertMsg', 'Message Dialog', 'กรุณาระบุข้อมูล ปี ค.ศ. ในช่วง ' + minYear + ' ถึง ' + maxYear + '', '', '');
        return false;
    }
}

function getAntiForgeryToken() {
    return $jq('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
}

function addAntiForgeryToken(data) {
    data.__RequestVerificationToken = getAntiForgeryToken();
    return data;
}

// For nested input data that use JSON.strinify 
function callAjaxJsonAntiForgery(url, requestType, inputData, onSuccess, onError) {
    var handleError = handleAjaxError;
    if (onError) handleError = onError;
    var headers = {};
    headers['__RequestVerificationToken'] = getAntiForgeryToken();

    $jq.ajax({
        url: url,
        type: requestType,
        headers: headers,
        dataType: 'json',
        contentType: "application/json",
        data: JSON.stringify(inputData),
        beforeSend: showLoading,
        complete: hideLoading,
        success: onSuccess,
        error: handleError
    });
}

function bindClickElementOnPressEnter(element, targetClickElement) {
    element.keypress(function (event) {
        if (event.which == 13) {
            targetClickElement.click();
        }
    });
}

// For one-layer input data without JSON.stringify
function callAjaxPost(url, inputData, onSuccess, onError) {
    var handleError = handleAjaxError;
    if (onError) handleError = onError;

    $jq.ajax({
        url: url,
        type: "POST",
        data: addAntiForgeryToken(inputData),
        beforeSend: showLoading,
        complete: hideLoading,
        success: onSuccess,
        error: handleError
    });
}

function formatTime(o) {
    try {
        var time = o.val();
        time = time.replace(/[^0-9]+/g, '');

        if (time.length == 0) {
            o.val(time);
            return;
        } else if (time.length > 4) {
            time = time.substring(time.length - 4);
        }

        time = padLeft(parseInt(time, 10), 4);
        o.val(time.substring(0, 2) + ":" + time.substring(2, 4));
    } catch (ex) {
        //console.log(ex);
        o.val('');
    }
}

function padLeft(o, length) {
    var r = "" + o;
    while (r.length < length) {
        r = "0" + r;
    }
    return r;
}

function validateNumber(e) {
    var valid = true;
    var keyCode = e.charCode ? e.charCode : (window.event) ? window.event.keyCode : e.which;

    // Allow: backspace, delete, tab, escape, and enter
    if (keyCode == 46 || keyCode == 8 || keyCode == 9 ||
        // Allow: home, end, left, right
        (keyCode >= 35 && keyCode <= 39)
    ) {
        // let it happen, don't do anything
        valid = true;
    }
    else {
        // Ensure that it is a number and stop the keypress
        if ((keyCode < 48 || keyCode > 57)) {
            //keyCode = 0;
            valid = false;
        }
    }

    return (valid);
}

function validateDigitAndSlash(e) {
    var valid = true;
    //var keyCode = (window.event) ? window.event.keyCode : e.which;
    var keyCode = e.charCode ? e.charCode : (window.event) ? window.event.keyCode : e.which;

    // Allow: backspace, delete, tab
    if (keyCode == 8 || keyCode == 9) {
        // let it happen, don't do anything
        valid = true;
    }
    else {
        // Ensure that it is a number or slash  stop the keypress
        if ((keyCode < 47 || keyCode > 57)) {
            //keyCode = 0;
            valid = false;
        }
    }

    return (valid);
}

function validatePaste(mode) {
    try {

        if (mode == "DigitAndSlash") {
            if (isNaN(this.clipboardData.getData("Text").replace(/\//gi, ''))
                || (this.clipboardData.getData("Text")).indexOf('-') > -1
                || (this.clipboardData.getData("Text")).indexOf(' ') > -1) { //text
                return false;
            }
        } else if (mode == "Digit") {
            if (isNaN(this.clipboardData.getData("Text")) || (this.clipboardData.getData("Text")).indexOf('.') > -1
                || (this.clipboardData.getData("Text")).indexOf('-') > -1
                || (this.clipboardData.getData("Text")).indexOf(' ') > -1) { //text
                return false;
            }
        } else if (mode == "DigitAndDot") {
            if (isNaN(this.clipboardData.getData("Text").replace(/[,\.]/gi, ''))
                || (this.clipboardData.getData("Text")).indexOf('-') > -1
                || (this.clipboardData.getData("Text")).indexOf(' ') > -1) { //text
                return false;
            }
        }
    } catch (e) {
        //return false;
    }

    return true;
}

function showMore(obj, colSpanNo) {
    setTimeout(function () {
        var spContent = obj.parent();
        var content = spContent.data('content');
        if (obj.hasClass('content-show')) {
            obj.closest('tr').next().remove();
            obj.removeClass('content-show');
        } else {
            obj.addClass('content-show');
            $jq('<tr class="content-show"><td colspan=' + colSpanNo + '>' + content + '</td></tr>').insertAfter(obj.closest('tr'));
        }
    }, 200);
}

function topLocation(url) {
    // make sure its a relative url
    if (url[0] === '/' && url[1] !== '/') {
        // x-broswer window.location.origin 
        if (!window.location.origin) window.location.origin = window.location.protocol + "//" + window.location.host;
        window.top.location = window.location.origin + url;
    } else {
        window.top.location = url;
    }
}

function supportFormData() {
    return !!window.FormData;
}

function setupFormSubmitWithFileUpload(formElement) {
    formElement.ajaxForm({
        beforeSend: showLoading,
        success: hideLoading,
        complete: onUploadSuccess,
        error: handleAjaxError
    });
}

function submitFormWithFileUpload(e, formElement, onSuccess) {
    e.preventDefault();
    if (supportFormData()) {
        submitFormWithFormData(formElement, onSuccess);
    } else {
        formElement.submit();
    }
    return false;
}

function submitFormWithFormData(formElement, onSuccess) {
    clearServerErrors();
    var formData = new FormData(formElement.get(0));
    $jq.ajax({
        url: url_SubmitUploadFile,
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        beforeSend: showLoading,
        complete: hideLoading,
        success: onSuccess,
        error: handleAjaxError
    });
}
