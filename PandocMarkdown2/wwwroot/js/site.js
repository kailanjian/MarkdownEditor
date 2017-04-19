﻿// Constants
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

// jQuery document load here
$(function () {
    // $("#compile-button").click(convertInput);
    // $("#input-box").bind('input propertychange', convertInput);
    editor.on("change", convertInput);

    updateViews();
    $(".resize-bar").draggable({
        drag: function (event, ui) {
            ui.position.top = $(".main").position().top;
            ui.position.left = Math.max(0, ui.position.left);
            ui.position.left = Math.min($(window).width() - RESIZE_BAR_WIDTH, ui.position.left);
            console.log("dragged to x " + ui.position.left);
            $("#input").width(ui.position.left);
            $("#output").width($(window).width() - ui.position.left - RESIZE_BAR_WIDTH);
        },
        scroll: false
    });

    $(window).on('resize', resizeViews);

    bindMenu();

    // bind save button
    $("#save-md").click(function () {
        //download(editor.getValue(), "file.md", "text/plain");w:w

        let fileName = $("#document-title-display").text();

        var blob = new Blob([editor.getValue()], { type: "text/plain;" });
        saveAs(blob, fileName + ".md");
    });

    // bind open button
    $("#open-md").on("click", function () {
        $("#markdown-file-input").trigger("click");
    });

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

    //bind title rename
    bindTitleRename();

});

function updateViews() {
    let windowWidth = $(window).width();
    let resizeBarOffset = inputBoxRelativeSize * windowWidth;
    $("#input").width(inputBoxRelativeSize * windowWidth);
    $("#output").width(windowWidth - RESIZE_BAR_WIDTH - inputBoxRelativeSize * windowWidth);
    $(".resize-bar").css("left", resizeBarOffset);
    $(".resize-bar").css("height", $(".main").height());
}

function resizeViews() {
    $("#output").width($(window).width() - $("#input").width() - RESIZE_BAR_WIDTH);
}



function convertInput() {
    //let inText = $("#input-box").val();
    let inText = editor.getValue();
    console.log(inText);
    $.ajax({
        url: "/api/Convert",
        method: "POST",
        contentType: "application/json;charset=utf-8",
        data: '"' + inText + '"'
    }).error(function (xhr, textStatus, errorThrown) {
        console.log(JSON.stringify(errorThrown));
        console.log(textStatus);
    }).done(function (data) {
        console.log(JSON.stringify(data));
        $(".html-container").html(data);
    });
}

function bindMenu() {

    // set menu initially hidden on the side
    let menuWidth = $(".menu").outerWidth();
    $(".menu").css("left", -1*menuWidth);

    $(".menu-close-button").click(function () {
        let menuWidth = $(".menu").outerWidth();
        $(".menu").animate({ left: -1 * menuWidth });
    });

    $(".menu-open-button").click(function () {
        $(".menu").animate({ left: 0 });
    })
}
function bindTitleRename() {
    function showInput() {
        $("#document-title-display").hide();
        $("#document-title-input").show();
        $("#document-title-input").focus();

    }

    function showDisplay() {
        $("#document-title-display").show();
        $("#document-title-input").hide();
        $("#document-title-display").text($("#document-title-input").val())
    }

    $("#document-title-display").click(showInput);

    $("#document-title-input").blur(showDisplay);
    $("#document-title-input").bind('keypress', function (e) {
        if (e.keyCode == 13) {
            showDisplay();
        }
    });
}