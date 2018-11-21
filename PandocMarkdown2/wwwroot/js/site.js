// Constants
const RESIZE_BAR_WIDTH = 5;

// variables
var inputBoxRelativeSize = 0.5;

// initialize the editor
var editor = CodeMirror.fromTextArea(document.getElementById("input-box"), {
    mode: "markdown",
    lineNumbers: true,
    theme: "default",
    extraKeys: { "Enter": "newLineAndIndentContinueMarkdownList" },
    keyMap: "vim"
});


// jQuery document loads here
$(function () {

    // bind the editor to recompute output on keypress(every time a new character is entered)
    editor.on("change", convertInput);

    // update the view initially.
    updateViews();

    // update on resizing
    $(window).on('resize', resizeViews);

    // run convert input initially in case the editor has something loaded from backend on page load.
    convertInput();

    if ($("#view-data-document-title").text() != "") {
        setTitle($("#view-data-document-title").text());
    }

    // bind save button
    $("#save-md").click(function () {
        let fileName = $("#document-title-display").text();

        var blob = new Blob([editor.getValue()], { type: "text/plain;" });
        saveAs(blob, fileName + ".md");
    });

    // bind open file button
    $("#open-md").on("click", function () {
        $("#markdown-file-input").trigger("click");
    });

    // Handle load document from local
    $("#markdown-file-input").change(function () {
        console.log("file opened");
        let input = document.getElementById("markdown-file-input");
        if (!input.files || !input.files[0]) {
            alert("Error loading file");
        } else {
            let file = input.files[0];
            let fr = new FileReader();
            fr.onload = function () {
                console.log(fr.result);
                editor.setValue(fr.result);
            };

            fr.readAsText(file);
        }
    });

    // TODO: move to separate function (event binding function)
    // Event handler for sharing the link
    $("#share-link-write").click(function () {
        let perm = $("#view-data-perm").text();
        if (perm === "write") {
            alert("Use current URL to access current document again:\n" + document.URL);
        } else {
            alert("You don't have permission to edit this document. You can edit a copy instead!\nAlternatively contact the owner and ask for the edit link");
        }
    });


    // bind event handlers to document on page load
    bindSave();
    bindTitleRename();
    bindResizeBar();
    bindMenu();

});

// function to reset the views upon resize
function updateViews() {

    // get window width and compute where the resize bar should be.
    let windowWidth = $(window).width();
    let resizeBarOffset = inputBoxRelativeSize * windowWidth;

    // input and output box widths are set relative to current position of resize bar.
    $("#input").width(inputBoxRelativeSize * windowWidth);
    $("#output").width(windowWidth - RESIZE_BAR_WIDTH - inputBoxRelativeSize * windowWidth);

    // resize bar is set based on offset and height is updated.
    $(".resize-bar").css("left", resizeBarOffset);
    $(".resize-bar").css("height", $(".main").height());
    
}

// change views on resize
function resizeViews() {
    // output width is set relative to input width and window width.
    $("#output").width($(window).width() - $("#input").width() - RESIZE_BAR_WIDTH);
}

// Converts input using backend Pandoc converter
function convertInput() {
    let inText = editor.getValue();

    console.log(inText);

    // posts the contents of the editor to converter and gets
    // converted HTML back.
    $.ajax({
        url: "/api/Convert",
        method: "POST",
        contentType: "application/json;charset=utf-8",
        data: JSON.stringify(inText)
    }).error(function (xhr, textStatus, errorThrown) {

        console.log(JSON.stringify(errorThrown));
        console.log(textStatus);

    }).done(function (data) {   

        console.log(JSON.stringify(data));

        // set output container to render generated html
        $(".html-container").html(data);

    });
}

// code to handle the menu
function bindMenu() {

    // set menu initially hidden on the side
    let menuWidth = $(".menu").outerWidth();
    $(".menu").css("left", -1*menuWidth);

    // bind click to shifting menu left (close)
    $(".menu-close-button").click(function () {
        let menuWidth = $(".menu").outerWidth();
        $(".menu").animate({ left: -1 * menuWidth });
    });

    // bind click to shifting menu right (open)
    $(".menu-open-button").click(function () {
        $(".menu").animate({ left: 0 });
    })
}

// title renaming function
function bindTitleRename() {
    // TODO: highlight text upon begin edit for easier modifications

    // Hide display, show input box instead.
    function showInput() {

        $("#document-title-display").hide();
        $("#document-title-input").show();
        $("#document-title-input").focus();

    }

    // Hide input box, show display (update text for display too)
    function showDisplay() {

        $("#document-title-display").show();
        $("#document-title-input").hide();
        $("#document-title-display").text($("#document-title-input").val())

    }

    // binding of events
    $("#document-title-display").click(showInput);

    $("#document-title-input").blur(showDisplay);
    $("#document-title-input").bind('keypress', function (e) {
        // to handle enter key
        if (e.keyCode === 13) {
            showDisplay();
        }
    });
}

// change the title by changing both input and display
function setTitle(title) {
    $("#document-title-input").val(title);
    $("#document-title-display").text(title);
}

// add slashes so escaped backslashes and backslashed characters save correctly.
function addSlashes(str) { 
    // don't need apostrophe's because we use quotes for strings and JS handles apostrophe's correctly
    //str = str.replace(/'/g, "\\'");
    str = str.replace(/\\/g, "\\\\");
    str = str.replace(/\"/g, "\\\"");
    return str;
}

// resize bar functionality
function bindResizeBar() {
    modes = {
        VIEW: "view",
        EDIT: "edit",
        NORMAL: "normal"
    };

    let currentMode = modes.VIEW;

    // Use jquery UI to adjust positions of items in document based on resize bar location.
    $(".resize-bar").draggable({
        drag: function (event, ui) {
            ui.position.top = $(".main").position().top;
            ui.position.left = Math.max(0, ui.position.left);
            ui.position.left = Math.min($(window).width() - RESIZE_BAR_WIDTH, ui.position.left);
            $("#input").width(ui.position.left);
            $("#output").width($(window).width() - ui.position.left - RESIZE_BAR_WIDTH);

            if (ui.position.left > $(window).width() - 50) {
                console.log("trying to snap");
                setMode(modes.EDIT, ui);
                $("#input").width($(window).width());
                $("#output").hide();
                return;
            } else if (ui.position.left < 50) {
                console.log("trying to snap (left)");
                setMode(modes.VIEW, ui);
                $("#input").hide();
                $("#output").width($(window).width());
            } else {
                setMode(modes.NORMAL, ui);
                $("#input").show();
                $("#output").show();
            }
        },
        scroll: false
    });

    // snapping to sides
    function setMode(mode, ui) {
        console.log("set mode");
        console.log("setting mode to: " + mode);
        currentMode = mode;
        if (mode === modes.EDIT) {
            $(".resize-bar").width(30);
            $(".resize-text").text("VIEW 🢃");
            ui.position.left = $(window).width() - 30;
        } else if (mode === modes.VIEW) {
            console.log("setting to edit mode");
            $(".resize-bar").width(30);
            $(".resize-text").text("EDIT 🢁");
            ui.position.left = 0;
        } else if (mode === modes.NORMAL) {
            $(".resize-bar").width(5);
            $(".resize-text").text("");
        }
    }
    
}

// popus up the flash-message div briefly (can be used for notifications, like for saving);
function flashMessage(str) {
    $("#flash-message").text(str);
    $("#flash-message").show();
    $("#flash-message").delay(300).fadeOut(400, "swing");
}

// binds save buttons
function bindSave() {
    function saveSuccess() {
        flashMessage("Saved!");
    }

    function saveError() {

    }
    
    function save() {
        // check if we are currently editing a document
        let title = $("#document-title-display").text();
        let text = addSlashes(editor.getValue());
        console.log("Sending: " + text);
        if ($("#view-data-id").text() !== "") {
            let id = $("#view-data-id").text();
            // post to save page endpoint the data
            $.ajax({
                url: "/Home/SaveDocument",
                method: "POST",
                contentType: "application/json;charset=utf-8",
                // post id (for auth), title, then text. Backend will parse this.
                data: '"' + id + '\n' + title + '\n' + text + '"'
            }).error(function (xhr, textStatus, errorThrown) {
                console.log(JSON.stringify(errorThrown));
                console.log(textStatus);
            }).done(function (data) {
                console.log("successfully saved" + data);
                saveSuccess()
            });
        } else {
            // post to new document endpoint to actually generate new file.
            $.ajax({
                url: "/Home/SaveNewDocument",
                method: "POST",
                contentType: "application/json;charset=utf-8",
                data: '"' + title +'\n' + text + '"'
            }).error(function (xhr, textStatus, errorThrown) {
                console.log(JSON.stringify(errorThrown));
                console.log(textStatus);
            }).done(function (data) {
                console.log(JSON.stringify(data));
                let base_url = window.location.origin;
                let path = "/Home/EditDocument/" + data;
                window.location.assign(base_url + path);
                alert("New document created in the cloud!\nLink to edit: " + base_url + path);
                saveSuccess();
            });
        }
    }

    $("#save-cloud").click(save);
    $("#top-save-button").click(save);
}