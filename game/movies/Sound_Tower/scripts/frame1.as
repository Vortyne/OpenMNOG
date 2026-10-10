#version 4
One = "SFX_GullFlock1.swf";
Two = "SFX_SeaSpray1.swf";
Three = "SFX_GullFlock2.swf";
Four = "SFX_SeaSpray2.swf";
Five = "SFX_SeaSpray3.swf";
tellTarget ("/") {
    SFX_One = "SFX_GullFlock1.swf";
    SFX_Two = "SFX_SeaSpray1.swf";
    SFX_Three = "SFX_GullFlock2.swf";
    SFX_Four = "SFX_SeaSpray2.swf";
    SFX_Five = "SFX_SeaSpray3.swf";
    Sound_OneLength = "4000";
    Sound_TwoLength = "5000";
    Sound_ThreeLength = "7000";
    Sound_FourLength = "6000";
    Sound_FiveLength = "10000";
}
stop();
