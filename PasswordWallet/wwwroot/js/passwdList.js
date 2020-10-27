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
            { "data": "login", "width": "20%" },
            { "data": "password", "width": "20%" },
            { "data": "webAddress", "width": "20%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                        <a class='btn btn-warning text-white' style='cursor:pointer; width:90px;'
                            href="/passwd/decrypt?id=${data}">
                            Decrypt
                        </a>
                        <a class='btn btn-danger text-white' style='cursor:pointer; width:70px;'
                            onclick=Delete('/passwd/delete?id='+${data})>
                            Delete
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
    swal({
        title: "Are You sure?",
        text: "Once deleted, you will not be able to recover",
        icon: "warning",
        buttons: true,
        dangermode: true
    }).then((willDelete) => {
        if (willDelete) {
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
    })
}
