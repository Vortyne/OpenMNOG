#version 5
StateLoaded = "0";
vars = "0";
notloggedin = "0";
nostate = "0";
servererror = "0";
if (_level0 && _level0.__matanui) {
    _level0.__matanui.getstate(eval("/"));
} else if (_root && typeof _root.getstate_url == "string" && _root.getstate_url.length) {
    loadVariables(_root.getstate_url, "/");
} else {
    loadVariables("getstate.asp", "/");
}
