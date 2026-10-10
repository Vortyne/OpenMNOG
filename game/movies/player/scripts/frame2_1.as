#version 5
if (_level0 && _level0.__matanui) {
    _level0.__matanui.getlanguage(eval("/"));
} else if (_root && typeof _root.getlanguage_url == "string" && _root.getlanguage_url.length) {
    loadVariables(_root.getlanguage_url, "/");
} else {
    loadVariables("getlanguage.asp", "/");
}
if (_level0 && _level0.__matanui) {
    _level0.__matanui.getdate(eval("/"));
} else if (_root && typeof _root.getdate_url == "string" && _root.getdate_url.length) {
    loadVariables(_root.getdate_url, "/");
} else {
    loadVariables("getdate.asp", "/");
}
loadMovie("blank.swf", "Backpack");
