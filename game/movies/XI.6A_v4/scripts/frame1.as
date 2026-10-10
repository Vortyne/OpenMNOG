#version 4
tellTarget ("/") {
    Startedloading = "1";
}
Language = eval("/:Language");
if (eval("/:Quest") < "6") {
    if (eval("/:cKapura") eq "1") {
        set("/:TaipuCanGo", "1");
        if (eval("/:cTaipu") eq "1") {
            TextToLoad = "Text_XI.6A_5_" add Language add ".txt";
        } else {
            if (eval("/:Taipu") eq "cBar") {
                Taipu = eval("/:TaipuWas");
            } else {
                Taipu = eval("/:Taipu");
            }
            if (Taipu eq "Free") {
                TextToLoad = "Text_XI.6A_3_" add Language add ".txt";
            } else {
                TextToLoad = "Text_XI.6A_4_" add Language add ".txt";
            }
            tellTarget ("/") {
                TaipuWas = Taipu;
                Taipu = "cBar";
            }
        }
    } else {
        TextToLoad = "Text_XI.6A_1_" add Language add ".txt";
    }
} else {
    TextToLoad = "Text_XI.6A_6_" add Language add ".txt";
}
