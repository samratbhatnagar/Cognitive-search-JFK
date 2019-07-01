// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

var apiUrl = '';

//Initialize Fabric elements
var SpinnerElements = document.querySelectorAll(".ms-Spinner");
for (var i = 0; i < SpinnerElements.length; i++) {
    new fabric['Spinner'](SpinnerElements[i]);
}

var SearchBoxElements = document.querySelectorAll(".ms-SearchBox");
for (var j = 0; j < SearchBoxElements.length; j++) {
    new fabric['SearchBox'](SearchBoxElements[j]);
}

var isGridInitialized = false;
var $grid = $('#doc-details-div');

$(document).ready(function () {
    if (q) {
        document.getElementById('q').value = q;
    }
    if (q !== null && q !== 'undefined') {
        Search();
    }
});

function InitLayout() {

    if (isGridInitialized === true) {
        $grid.masonry('destroy'); // destroy
    }

    $grid.masonry({
        itemSelector: '.results-div',
        columnWidth: '.results-sizer'
    });

    $grid.imagesLoaded().progress(function () {
        $grid.masonry('layout');
    });

    isGridInitialized = true;
}

function FabricInit() {
    var CheckBoxElements = document.querySelectorAll(".ms-CheckBox");
    for (var i = 0; i < CheckBoxElements.length; i++) {
        new fabric['CheckBox'](CheckBoxElements[i]);
    }
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}

function highlight(text, term) {
    var terms = term.split(' ');
    var highlightedText = text;
    terms.forEach(function (t) {
        var regex = new RegExp(t, 'gi');
        highlightedText = highlightedText.replace(regex, function (str) {
            return "<span class='highlight'><b>" + str + "</b></span>";
        });
    });
    
    return highlightedText;
}

function generateSummary(text, term) {
    if (text.length <= 500) {
        return text;
    }
    var termLower = term.toLowerCase();
    var textLower = text.toLowerCase().replace(/\\n/g, ' ');

    var textToSummarize = text.replace(/\\n/g, ' ').trim();

    var firstTermInstancePos = textLower.indexOf(termLower);

    if (firstTermInstancePos > 0) {
        var sub = textToSummarize.substring(0, firstTermInstancePos);
        var lastSentenceEndBeforeTerm = sub.lastIndexOf(". ");
        if (lastSentenceEndBeforeTerm === -1) {
            lastSentenceEndBeforeTerm = 0;
        }
        textToSummarize = textToSummarize.substring(lastSentenceEndBeforeTerm + 2).trim();
    }
    
    var summary = textToSummarize.substring(0, 500).trim() + '...';

    return summary;
}