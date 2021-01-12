var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/history/GetAllActions",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "action", "width": "20%" },
            { "data": "date", "width": "20%" },   
        ],
        "language": {
            "emptyTable": "no data found"
        },
        "width": "100%"
    });
}