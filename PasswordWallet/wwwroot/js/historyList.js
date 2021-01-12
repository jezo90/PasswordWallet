var dataTable;
const queryString = window.location.search;
const urlParams = new URLSearchParams(queryString);
const passwordId = urlParams.get('id');


$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": `getone?id=+${passwordId}`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "oldPasswd", "width": "20%" },
            { "data": "newPasswd", "width": "20%" },
            { "data": "date", "width": "20%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                        <a class='btn btn-warning text-white' style='cursor:pointer; width:80px;'
                            href="/passwd/recover?id=${data}&passwordId=${passwordId}">
                            Recover
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
