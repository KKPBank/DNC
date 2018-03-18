function deleteAllGridProducts(tableId, productList) {
    // remove ll temp rows
    var tempRow = $jq("#" + tableId + " > tbody > tr.tempData");
    tempRow.remove();

    // update active row display
    var deleteTime = new Date();
    var exsitingRows = $jq("#" + tableId + " > tbody > tr.existingData");
    $jq.each(exsitingRows, function () {
        var cell = $jq(this).find(".delete-active").parent();
        updateDeleteExisingRow(cell, deleteTime);
    });

    // update block list data
    for (var i = productList.length - 1; i >= 0; i--) {
        var item = productList[i];
        if (item.ActivityProductId == 0) {
            productList.splice(i, 1);
        }
        else if (!item.IsDeleted) {
            item.IsDeleted = true;
            item.UpdateDate = deleteTime;
        }
    };
}

function updateDeleteExisingRow(cell, deleteTime) {
    var timestamp = convertDateToTimeStamp(deleteTime);
    // remove detele button
    cell.html('');
    // set status to Deleted
    var statusCell = cell.next().next();
    statusCell.text("Deleted");
    // set update by
    var updateByCell = statusCell.next();
    updateByCell.text($jq("#hidCurrentUserFullName").val());
    // set update date
    var updateDateCell = updateByCell.next();
    updateDateCell.text(timestamp);
}

function deleteTempProduct(cell, productId, productList) {
    // remove from row
    var row = cell.parent();
    row.remove();
    // remove from list
    $jq.each(productList, function (i, el) {
        if (this.Id == productId && this.ActivityProductId == 0) {
            productList.splice(i, 1);
        }
    });
}

function deleteExistingProduct(cell, activityProductId, productList) {
    var deleteTime = new Date();
    // update row display
    updateDeleteExisingRow(cell, deleteTime);
    // update list data
    $jq.each(productList, function (i, el) {
        if (this.ActivityProductId == activityProductId) {
            this.IsDeleted = true;
            this.UpdateDate = deleteTime;
        }
    });
}

function hasActiveProducts(productList) {
    var hasActive = false;
    $jq.each(productList, function () {
        if (!this.IsDeleted) {
            hasActive = true;
            return;
        }
    });
    return hasActive;
}

function isExistingProduct(productId, productList) {
    var exists = false;
    $jq.each(productList, function (i, el) {
        if (this.Id == productId && !this.IsDeleted) {
            exists = true;
            return;
        }
    });
    return exists;
}

function createNewProductModel(id, name, createDate) {
    return {
        Id: id,
        Name: name,
        CreateDate: createDate,
        UpdateDate: createDate,
        ActivityProductId: 0,
        IsDeleted: false
    }
}

function generateNewProductRow(productId, productName, createDate, onClickFuncString) {
    var timestamp = convertDateToTimeStamp(createDate);
    var actionColumn = "<td class='center'><a href='javascript:;' class='delete-active' title='delete' onClick='" + onClickFuncString + "(this, " + productId + ", 0)'></a></td>";
    var valColumn = "<td>" + productName + "</td>";
    var statusCol = "<td class='center'>Active</td>";
    var updateByCol = "<td>" + $jq("#hidCurrentUserFullName").val() + "</td>";
    var dateColumn = "<td class='center'>" + timestamp + "</td>";

    var html = "<tr class='tempData'>" + actionColumn + valColumn + statusCol + updateByCol + dateColumn + "</tr>";

    return html;
}

function onDeleteProduct(element, productId, activityProductId, productList) {
    var productName = $jq(element).parent().next().text();
    if (confirm('ต้องการลบข้อมูล ' + productName + ' ใช่หรือไม่')) {
        var cell = $jq(element).parent();
        var isExisitngData = activityProductId > 0;
        if (!isExisitngData) {
            deleteTempProduct(cell, productId, productList);
        }
        else {
            deleteExistingProduct(cell, activityProductId, productList);
        }
    }
}

function initAutoCompleteProduct(txtElement, getExistingIds) {
    txtElement.select2({
        placeholder: autoCompletePlaceHolder,
        minimumInputLength: autoCompleteMinLength,
        language: "th",
        allowClear: true,
        ajax: {
            url: url_AutoCompleteSearchProductWithExceptions,
            dataType: 'json',
            contentType: "application/json",
            type: "post",
            params: { traditional: true },
            quietMillis: 150,
            data: function (keyword) {
                var model = {
                    Keyword: keyword,
                    ExceptProductIds: getExistingIds()
                };

                return model;
            },
            results: function (data) {
                if (data.RedirectUrl != undefined) {
                    topLocation(data.RedirectUrl);
                    return;
                }
                return {
                    results: $jq.map(data, function (item) {
                        return {
                            text: item.ProductName,
                            id: item.ProductId
                        }
                    })
                };
            },
            error: handleAjaxError
        }
    });
}