var viz, workbook, activeSheet, selectedMarks, options, placeholderDiv;
var url = '';
var tableauServer = '';
var tableauSite = '';
var tableauPath = '';
var ticket = '';
var nameOfVizToInteract = 'Scatterplot';

function initializeViz() {
    url = tableauServer + '/trusted/' + ticket + "/t" + tableauSite + tableauPath;
    placeholderDiv = document.getElementById("tableauViz");
    options = {
        width: placeholderDiv.offsetWidth,
        height: placeholderDiv.offsetHeight,
        hideTabs: true,
        hideToolbar: true,
        ":refresh": "yes",
        onFirstInteractive: completeLoad 
    };

    viz = new tableau.Viz(placeholderDiv, url, options);
}

function exportPDF() {
    viz.showExportPDFDialog();
    $('.tab-dialog')[0].animate({ 'marginLeft': "-=50px" });
}

function exportData() {
    viz.showExportDataDialog();
}

function resetViz() {
    viz.revertAllAsync();
}

function showVizButtons() {
    var sheets = workbook.getPublishedSheetsInfo();
    var divIndividualButtons = $('#vizButtons');

    // First clear any buttons that may have been added on a previous load
    divIndividualButtons.html("");

    // Show 'standard' controls, common to all vizzes
    divIndividualButtons.append('<button type="button" onclick="resetViz()" class="btn btn-primary" style="min-width:135px; margin-right: 5px; margin-top: 5px;">Reset Filters</button>');
    divIndividualButtons.append('<button type="button" onclick="exportPDF()" class="btn btn-primary" style="min-width:135px; margin-right: 5px; margin-top: 5px;">Export PDF</button>');
    divIndividualButtons.append('<button type="button" onclick="exportData()" class="btn btn-primary" style="min-width:135px; margin-right: 5px; margin-top: 5px;">Export Data</button>');
    divIndividualButtons.append('<button type="button" onclick="launch_edit()" class="btn btn-primary" style="min-width:135px; margin-right: 5px; margin-top: 5px;">Edit</button>');

    // Only show buttons to switch vizzes if there's more than one
    if (sheets.length > 1) {
        for (var sheetIndex = 0; sheetIndex < sheets.length; sheetIndex++) {
            var sheet = sheets[sheetIndex];
            divIndividualButtons.append('<button type="button" onclick="switchToViz(\'' + sheet.getName() + '\')" class="btn btn-primary" style="min-width:135px; margin-right: 5px; margin-top: 5px;">See ' + sheet.getName() + '</button>')
        }
    }
}

function switchToViz(vizName) {
    workbook.activateSheetAsync(vizName).then(function (dashboard) {
        dashboard.changeSizeAsync({
            behavior: tableau.SheetSizeBehavior.AUTOMATIC
        });
    });
}

function onMarksSelection(marksEvent) {
    //filter sheets of selected marks because we dont need to hear events on all of our sheets
    if (marksEvent.getWorksheet().getName() == nameOfVizToInteract) {
       
        //get,marksAsync() is a method in the API that will retun a set of the marks selected
        return marksEvent.getMarksAsync().then(handleSelectedMarks);
    }
}

function handleSelectedMarks(marks) {

    // If selection has been cleared, no need to show a message
    if (marks.length == 0) {
        $('#eventBox').hide(600);
        return;
    }

    // Save selected marks in memory so they can be submitted later
    selectedMarks = marks;

    $('#eventPanel').html("");
    $('#eventBox').show(600);

    // Logic for Equities Dashboard is specialized, any other scatterplots also are enabled but in a general sense
    if (workbook.getActiveSheet().getName() == 'Individual Equities Dashboard') {

        //loop through all the selected marks
        var noOrders = 0;
        var company = "";
        var fixedClose = 0;
        var fixedCloseLabel = "";
        var changeFromPriorClose = 0;
        var changeFromPriorCloseLabel = "";
        var date = "";

        for (var markIndex = 0; markIndex < marks.length; markIndex++) {

            //getPairs gets tuples of data for the mark. one mark has multiple tuples
            var pairs = marks[markIndex].getPairs();

            for (var pairIndex = 0; pairIndex < pairs.length; pairIndex++) {

                switch (pairs[pairIndex].fieldName) {
                    case "Company":
                        company = pairs[pairIndex].value;
                        break;
                    case "Date":
                        date = pairs[pairIndex].formattedValue;
                        break;
                    case "SUM(Fixed Close)":
                        fixedClose += pairs[pairIndex].value;
                        fixedCloseLabel = pairs[pairIndex].formattedValue;
                        break;
                    case "AGG(Change from Prior Close)":
                        changeFromPriorClose += pairs[pairIndex].value;
                        changeFromPriorCloseLabel = pairs[pairIndex].formattedValue;
                        break;

                }

            }
        }

        // With all values in memory, let's produce the UI
        if (marks.length == 1) {
            // When we select a single mark, we can show the individual details
            $('#eventPanel').html("Submit <b>" + company + "</b>'s " + date + " trading period for research. The fixed close price was <b>$" + fixedCloseLabel + "</b> with a variance of <b>" + changeFromPriorCloseLabel + "</b>.");
        }
        else {
            // But if more that one mark is selected, we show a summary (average)
            var avgFixedClose = Number((fixedClose / marks.length).toFixed(2));
            var avgChangeFromPriorCloseLabel = Number((changeFromPriorClose * 100 / marks.length).toFixed(2));
            $('#eventPanel').html("Submit <b>" + marks.length + " " + company + "</b>'s trading periods for research. The average fixed close price was <b>$" + avgFixedClose + "</b> & the average variance of <b>" + avgChangeFromPriorCloseLabel + "%</b>.");
        }
    }
    else {
        // Save selection in memory and give the user the option to submit
        var plural = ((marks.length == 1) ? "it" : "them");
        var pluralS = ((selectedMarks.length == 1) ? "" : "s");
        $('#eventPanel').html("You've selected <b>" + marks.length + "</b> outlier" + pluralS + "." + " Would you like to submit " + plural + " them for research?");
    }

}

function submitMarks()
{
    var referrer = viz.getWorkbook().getActiveSheet();

    if (referrer.getSheetType() == "dashboard") {
        // The active sheets is a dashboard, which is made of several sheets
        var sheets = referrer.getWorksheets();

        // Iterate over the sheets until we find the correct one and clear the marks
        for (var sheetIndex = 0; sheetIndex < sheets.length; sheetIndex++) {
            if (sheets[sheetIndex].getName() == nameOfVizToInteract) {
                sheets[sheetIndex].clearSelectedMarksAsync();
            }
        }
    }
    else {
        // This is not a dashboard so just clear the sheet's selection
        referrer.clearSelectedMarksAsync();
    }

    tableauWriteBack(selectedMarks);

    //var plural = ((selectedMarks.length == 1) ? "" : "s");
    //$('#eventPanel').html("Success! <b>" + selectedMarks.length + "</b> selection" + plural + " submitted for research.");
    //$('#eventBox').hide(2000);
}

function resetAllMarks() {
    
    var referrer = viz.getWorkbook().getActiveSheet();

    if (referrer.getSheetType() == "dashboard") {
        // The active sheets is a dashboard, which is made of several sheets
        var sheets = referrer.getWorksheets();

        // Iterate over the sheets until we find the correct one and clear the marks
        for (var sheetIndex = 0; sheetIndex < sheets.length; sheetIndex++) {
            if (sheets[sheetIndex].getName() == nameOfVizToInteract) {
                sheets[sheetIndex].clearSelectedMarksAsync();
            }
        }
    }
    else {
        // This is not a dashboard so just clear the sheet's selection
        referrer.clearSelectedMarksAsync();
    }

    $('#eventBox').hide(800);
    $('#eventPanel').html("");
}

function launch_edit() {

    // Adjust UI: Hide Buttons & navigation menu, increase size for edit mode
    $('#VizToolbar').hide();
    $('body').addClass("sidebar-collapse");
    $(".content-wrapper").css("height","1200px");
    $("#tableauViz").hide();

    // If the URL happens to have a ticket on it, clean it up before loading the edit window
    var url_parts = url.split('/t/');
    url = tableauServer + '/t/' + url_parts[1]; 
    var edit_location = tableauServer + '/en/embed_wrapper.html?src=' + url + '?:embed=y'; 

    edit_iframe = document.createElement('iframe');
    edit_iframe.src = edit_location;

    // This makes it not look like an iframe
    edit_iframe.style.padding = '0px';
    edit_iframe.style.border = 'none';
    edit_iframe.style.margin = '0px';

    // Also set these with the same values in the embed_wrapper.html page
    edit_iframe.style.width = '100%';
    edit_iframe.style.height = '100%';

    $('#editViz').html(edit_iframe);
    $('#editViz').show();
}

function iframe_change(new_url) {

    console.log("Old URL received in iframe_change: " + url);
    console.log("New URL received in iframe_change: " + new_url);

    // Destroy the original edit_iframe so you can build another one later if necessary
    $(edit_iframe).remove();

    // Destroy the original Tableau Viz object so you can create new one with URL of the Save(d) As version
    viz.dispose();

    // Reset the global vizURL at this point so that it all works circularly
    // But first remove any embed/authoring attributes from the URL
    var url_parts = new_url.split('?');
    url = url_parts[0].replace('/authoring', '/views');

    // Handle site
    if (url.search('/site/') !== -1) {
        url_parts = url.split('#/site/');
        url = url_parts[0] + "t/" + url_parts[1];
        vizUrlForWebEdit = url;
        console.log("URL updated in iframe_change: " + url);
    }

    // Adjust UI: Show buttons & navigation menu, decrease size post-edit mode
    $('#VizToolbar').show();
    $('body').removeClass("sidebar-collapse");
    $(".content-wrapper").css("height", "");
    $("#tableauViz").show();
    $("#editViz").hide();

    // Create a new Viz object
    viz = null;
    viz = new tableau.Viz(placeholderDiv, url, options);

}

function completeLoad(e) {
    
    // Once the workbook & viz have loaded, assign them to global variables
    workbook = viz.getWorkbook();
    activeSheet = workbook.getActiveSheet();

    // Load custom controls based on the vizzes published to the server
    showVizButtons();
    
    viz.addEventListener(tableau.TableauEventName.MARKS_SELECTION, onMarksSelection);
}

$(document).ready(initializeViz); 


