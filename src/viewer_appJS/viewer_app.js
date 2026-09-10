var dScroll = false;
/* disableScroll set when navigating to a new page using a click 
 see viewer_app.xsl template in mode handle-itemclick */
var disableScroll = function() {
    dScroll = true;
}
/* enableScroll used to bring search matches on page into view */
var enableScroll = function() {
    dScroll = false;
}

/* scrollToElement used for javadoc and dotnetdoc links */
var scrollToElement = function(elementItem) {
    if (elementItem != null) {
        elementItem.scrollIntoView(true);
    }
};

/* item is the currently highlighted item in the nav list
    i.e. the span element with @class= "hot" */
var item;
/* swapItem used in highlight-item template in viewer_app.xsl
    highlighting removed from previous highlighted item */
var swapItem = function(newItem) {
    if (!(dScroll)) {
        newItem.scrollIntoView(true);
    }
    var prevItem = item;
    item = newItem;
    return prevItem;
};

/* goback links used when classes not in javadoc and dotnetdoc */
var goback = function() {
    window.history.back();
};

