var viz, workbook, activeSheet, options, placeholderDiv;
var url = '';
var tableauServer = '';
var tableauSite = '';
var tableauPath = '';
var ticket = '';

function initializeViz() {
    url = tableauServer + '/trusted/' + ticket + "/t" + tableauSite + tableauPath;
    placeholderDiv = document.getElementById("InputViz");
    options = {
        width: placeholderDiv.offsetWidth,
        height: placeholderDiv.offsetHeight,
        hideTabs: true,
        hideToolbar: true,
        onFirstInteractive: completeLoad 
    };

    viz = new tableau.Viz(placeholderDiv, url, options);
}

function exportPDF() {
    viz.showExportPDFDialog();
    $(".tab-dialog").css({ left: 100 });
}

function exportData() {
    viz.showExportDataDialog();
}

function showVizButtons() {
    var sheets = workbook.getPublishedSheetsInfo();
    var divIndividualButtons = $('#vizButtons');

    // First clear any buttons that may have been added on a previous load
    divIndividualButtons.html("");

    // Show 'standard' controls, common to all vizzes
    //divIndividualButtons.append('<button type="button" onclick="exportPDF()" class="btn btn-primary" style="min-width:135px; margin-right: 5px; margin-top: 5px;">Export PDF</button>');
    divIndividualButtons.append('<button type="button" onclick="exportData()" class="btn btn-primary" style="min-width:135px; margin-right: 5px; margin-top: 5px;">Export Data</button>');
}

function showInputElements() {
    var sheets = workbook.getPublishedSheetsInfo();
    var divTextBoxContainers = $('#InputText');
    divTextBoxContainers.show();
}

function submitInput()
{
    var InputElements = $('#InputText .form-control');

    var inputArray = {};
    var Items = []
    inputArray.Items = Items;

    // Iterate over the textboxes and form a JSON object with the values added by the user
    for (var inputIndex = 0; inputIndex < InputElements.length; inputIndex++) {

        if (InputElements[inputIndex].value != "") {
            var inputValue = { "ItemName": InputElements[inputIndex].id, "Quantity": InputElements[inputIndex].value };
            inputArray.Items.push(inputValue);
        }
    }

    console.log(JSON.stringify(inputArray));

    // With the JSON object, make a call to the controller to save the data in the database
    $.post("TableauWriteBack", { inputArray: JSON.stringify(inputArray) }, function (data, code, xhr) {
        if (xhr.getResponseHeader("Error") != null) {
            //setError(xhr.getResponseHeader("Error"));
        } else {
            viz.refreshDataAsync(); 
            ClearInputData(InputElements);
        }
    });
}

function ClearInputData(elements)
{
    for (var inputIndex = 0; inputIndex < elements.length; inputIndex++) {
        elements[inputIndex].value = "";
    }
}

function completeLoad(e) {
    
    // Once the workbook & viz have loaded, assign them to global variables
    workbook = viz.getWorkbook();
    activeSheet = workbook.getActiveSheet();

    // Load custom controls based on the vizzes published to the server
    showVizButtons();
    showInputElements();
}

$(document).ready(initializeViz); 


