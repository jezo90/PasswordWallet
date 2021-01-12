var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/passwd/getall",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "login", "width": "10%" },
            { "data": "password", "width": "20%" },
            { "data": "webAddress", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                        <a class='btn btn-success text-white' style='cursor:pointer; width:80px;'
                            onclick=Delete('/passwd/read?id='+${data})>
                            Edit
                        </a>
                        <a class='btn btn-primary text-white' style='cursor:pointer; width:80px;'
                            href="/passwd/share?id=${data}">
                            Share
                        </a>
                        <a class='btn btn-warning text-white' style='cursor:pointer; width:80px;'
                            href="/passwd/decrypt?id=${data}">
                            Decrypt
                        </a>
                        <a class='btn btn-danger text-white' style='cursor:pointer; width:80px;'
                            onclick=Delete('/passwd/read?id='+${data})>
                            Delete
                        </a>
                        <a class='btn btn-dark text-white' style='cursor:pointer; width:80px;'
                            onclick=Delete('/passwd/read?id='+${data})>
                            History
                        </a>
                        </div>`;
                }, "width": "40%"
            }
        ],
        "language": {
            "emptyTable": "no data found"
        },
        "width": "100%"
    });
}

function All(url) {
    $.ajax({
        type: "GET",
        url: url
    },
        toastr.success(data.message),
        dataTable.ajax.reload(),
        toastr.error(data.message))

}


function Delete(url) {
    $.ajax({
        type: "DELETE",
        url: url,
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    })
}
