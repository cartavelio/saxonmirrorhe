var dScroll = false;
/* disableScroll set when navigating to a new page using a click 
 see jstree.xsl template in mode handle-itemclick */
var disableScroll = function() {
    dScroll = true;
}
/* enableScroll used to bring search matches on page into view */
var enableScroll = function() {
    dScroll = false;
}

/* item is the currently highlighted item in the nav list
    i.e. the span element with @class= "hot" */
var item;
/* swapItem used in highlight-item template in jstree.xsl
    highlighting removed from previous highlighted item */
var swapItem = function(newItem) {
    if (!(dScroll)) {
        newItem.scrollIntoView(true);
    }
    var prevItem = item;
    item = newItem;
    return prevItem;
};

// Encapsulation for the saved scroll state
const createScrollState = () => {
  let scrollTop = -1;
  return ({
    "set": (ypos) => {
      scrollTop = ypos;
    },
    "get": () => {
      return scrollTop;
    }
  });
};
const scrollState = createScrollState();

// Need a top-level function in order to access it from within
// SaxonJS using window.getViewerScrollState
function getViewerScrollState() {
  return scrollState.get();
}

// Update the scroll state when the window history is popped.
// If we're navigating with the "back" button, then the state
// saved by f:anchor-navigation in jstree.xsl will be
// available and the saved scrollTop position will be >= 0.
// If that's not the case, store -1 in the saved state to
// tell the viewer application to ignore the saved state.
window.onpopstate = function(event) {
  if (event.state != null && 'scrollTop' in event.state) {
    scrollState.set(event.state.scrollTop);
  } else {
    scrollState.set(-1);
  }
};
