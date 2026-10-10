#version 4
One = "SFX_windgusts.swf";
Two = "blank.swf";
Three = "SFX_desertbugs.swf";
Four = "blank.swf";
Five = "SFX_desertbugs.swf";
tellTarget ("/") {
    SFX_One = "SFX_windgusts.swf";
    SFX_Two = "blank.swf";
    SFX_Three = "SFX_desertbugs.swf";
    SFX_Four = "blank.swf";
    SFX_Five = "SFX_desertbugs.swf";
    Sound_OneLength = "6000";
    Sound_TwoLength = "2000";
    Sound_ThreeLength = "8000";
    Sound_FourLength = "2000";
    Sound_FiveLength = "8000";
}
stop();
