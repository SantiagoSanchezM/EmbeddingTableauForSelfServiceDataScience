var viz, workbook, activeSheet;
var tableauServer = '';
var tableauSite = '';
var tableauPath = '';
var ticket = '';

function initializeViz() {
    var placeholderDiv = document.getElementById("tableauViz");
    var url = 'http://' + tableauServer + '/trusted/' + ticket + tableauPath;
    var options = {
        width: placeholderDiv.offsetWidth,
        height: placeholderDiv.offsetHeight,
        hideTabs: true,
        hideToolbar: true,
        onFirstInteractive: function () {
            workbook = viz.getWorkbook();
            activeSheet = workbook.getActiveSheet();
        }
    };
    viz = new tableau.Viz(placeholderDiv, url, options);
}

function exportPDF() {
    viz.showExportPDFDialog();
}

function exportData() {
    viz.showExportDataDialog();
}

$(initializeViz);
