function convertDateToTimeStamp(date) {
    return convertDateToString_ddMMyyyy(date, "/") + " " + convertDateTimeToString_HHmmss(date, ":");
}

function openNewPhoneNoWindow(actionUrl, isNewTab) {
    var params = [];
    params.push({ Name: "email", Value: $jq("#txtEmail").val() });
    params.push({ Name: "phoneNo", Value: $jq("#txtPhoneNo").val() });
    openWindowWithParams(actionUrl, isNewTab, params);
}

function openEditPhoneNoWindow(actionUrl, isNewTab, id) {
    openWindowWithParams(actionUrl, isNewTab, [{ Name: "id", Value: id }]);
}

function openEditCustomerWindow(actionUrl, isNewTab, id) {
    openWindowWithParams(actionUrl, isNewTab, [{ Name: "id", Value: id }]);
}

function openNewWindow(actionUrl, isNewTab) {
    var form = '<form action="' + actionUrl + '" method="POST" class="hidden"';
    if (isNewTab) form += ' target="_blank"';
    form += '></form>';
    $jq('#dvTarget').html('');
    var inputToken = $jq("<input>").attr("type", "hidden").attr("name", "__RequestVerificationToken").val(getAntiForgeryToken());
    $jq('#dvTarget').append(form);
    $jq('#dvTarget form').append($jq(inputToken));
    $jq('#dvTarget form').submit();
}

function openWindowWithParams(actionUrl, isNewTab, params) {
    var form = '<form action="' + actionUrl + '" method="POST" class="hidden"';
    if (isNewTab) form += ' target="_blank"';
    form += '></form>';
    $jq('#dvTarget').html('');
    var inputToken = $jq("<input>").attr("type", "hidden").attr("name", "__RequestVerificationToken").val(getAntiForgeryToken());
    $jq('#dvTarget').append(form);
    $jq('#dvTarget form').append($jq(inputToken));

    for (var i = 0; i < params.length; i++) {
        var param = params[i];
        var item = $jq("<input>").attr("type", "hidden").attr("name", param.Name).val(param.Value);
        $jq('#dvTarget form').append($jq(item));
    }

    $jq('#dvTarget form').submit();
}
