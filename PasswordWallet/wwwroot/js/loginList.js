var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/attempt/getall",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "date", "width": "20%" },
            { "data": "time", "width": "20%" },
            { "data": "addressIp", "width": "20%" },
            { "data": "successful", "width": "20%" },
        ],
        "language": {
            "emptyTable": "no data found"
        },
        "width": "100%"
    });
}