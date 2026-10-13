#version 6
TaVisit = "0";
Quest = "0";
GaKoro = "0";
GaKoroVisited = "0";
Sound_BG_Volume = "High";
Sound_ChangeBackgroundVolume = "0";
loadMovie("I.1-S.swf", "holder");
loadMovie("Sound_Waves.swf", "waves");
leaving = 0;
fade = 0;
// The logo and buttons fade in over 2 seconds (36 frames).
ui._alpha = 0;
// Continue is there when the selected save slot has a save (ui.canContinue: the engine keeps it hidden otherwise,
// as it shows and hides the menu around the settings dialog).
refreshSlot = function() {
    ui.canContinue = _level0.__matanui.hasSave();
    ui.continueButton._visible = ui.canContinue;
};
refreshSlot();
ui.versionText = System.mnogVersion != undefined && System.mnogVersion != "" ? "OpenMNOG v" + System.mnogVersion : "";
