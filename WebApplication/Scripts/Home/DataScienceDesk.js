function ClearInputData(elements) {
    for (var inputIndex = 0; inputIndex < elements.length; inputIndex++) {
        elements[inputIndex].value = "";
    }
}

function updateProgress() {
    setTimeout(function () {
        progress = $(".progress-bar").width();
        if (progress <= 85 ) {
            $(".progress-bar").css("width", progress + 5 + "%");
            updateProgress();
        }
    }, 1000);
}

function submitInput() {

    // Reset the Loading Dialog
    $('#modelOpenWorkbook').addClass("disabled");
    $("#modelLoading").html("Loading...");
    $(".progress-bar").css("width", "0%");
    $(".progress-bar").addClass("active");
    updateProgress();
    

    var InputElements = $('#PriceInput .form-control');
    var inputArray = {};
    var Items = [];
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
    $.post("Home/RunDataScienceModel", { inputArray: JSON.stringify(inputArray) }, function (data, code, xhr) {
        if (xhr.getResponseHeader("Error") != null) {
            setError(xhr.getResponseHeader("Error"));
            $("#modelLoading").html("Sorry, there's been an error. Please try again later.");
        } else {
            // Once the model is done, stop the loading bar and enable a button to go to the viz
            $('#modelOpenWorkbook').removeClass("disabled");
            $("#modelLoading").html("Done!");
            $(".progress-bar").css("width", "100%");
            $(".progress-bar").removeClass("active");
            ClearInputData(InputElements);
        }
    });
}