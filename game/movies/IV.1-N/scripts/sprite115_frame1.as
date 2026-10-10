#version 4
Language = eval("/:Language");
TextToLoad = "Text_IV1_Guard_" add Language add ".txt";
loadVariables(TextToLoad, "Bubble1");
loadVariables(TextToLoad, "Bubble2");
tellTarget ("Forwards/TurnStop") {
    WhichBubble = "../../Bubble1";
}
tellTarget ("Backwards/TurnStop") {
    WhichBubble = "../../Bubble2";
}
