#version 4
tellTarget ("TextBar") {
    NumStatements = "0";
}
Language = eval("/:Language");
if (Name ne "") {
    MovieName = eval("/:MovieName");
    Quest = eval("/:Quest");
    if (MovieName eq "V.2-N.swf" and Quest < "1") {
        TextToLoad = "Text_cBar_" add Name add "_V2-N_" add Language add ".txt";
    } else if (MovieName eq "XXII.5-N.swf" and Quest < "2") {
        TextToLoad = "Text_cBar_" add Name add "_XXII5-N_" add Language add ".txt";
    } else if (MovieName eq "XXII.9-N.swf" and Quest < "3") {
        TextToLoad = "Text_cBar_" add Name add "_XXII9-N_" add Language add ".txt";
    } else if (MovieName eq "XXII.14-N.swf" and Quest < "4") {
        TextToLoad = "Text_cBar_" add Name add "_XXII14-N_" add Language add ".txt";
    } else if (MovieName eq "XXIV.1.swf") {
        TextToLoad = "Text_cBar_" add Name add "_XXIV1_" add Language add ".txt";
    } else if (MovieName eq "XXIV.2.swf") {
        TextToLoad = "Text_cBar_" add Name add "_XXIV2_" add Language add ".txt";
    } else if (MovieName eq "XXIV.3.swf") {
        TextToLoad = "Text_cBar_" add Name add "_XXIV3_" add Language add ".txt";
    } else if (MovieName eq "XXIV.4.swf") {
        TextToLoad = "Text_cBar_" add Name add "_XXIV4_" add Language add ".txt";
    } else if (MovieName eq "VIII.5-E.swf" and Name eq "MAKU") {
        TextToLoad = "Text_cBar_MAKU_VIII5-E_" add Language add ".txt";
    } else if (MovieName eq "XII.11-E.swf" and Name eq "KAPURA") {
        TextToLoad = "Text_cBar_KAPURA_XII11-E_" add Language add ".txt";
    } else if (MovieName eq "XV.5-S.swf" and Name eq "KOPEKE") {
        TextToLoad = "Text_cBar_KOPEKE_XV5-S_" add Language add ".txt";
    } else if (MovieName eq "IX.4-E.swf" and Name eq "HAFU") {
        TextToLoad = "Text_cBar_HAFU_IX4-E_" add Language add ".txt";
    } else if ((MovieName eq "IV.8-N.swf" or MovieName eq "IV.8A.swf" or MovieName eq "XIX.1-N.swf" or MovieName eq "XIX.1B.swf") and Name eq "TAMARU") {
        TextToLoad = "Text_cBar_TAMARU_IV8-N_" add Language add ".txt";
    } else if ((MovieName eq "XVII.1-S.swf" or MovieName eq "XVII.1-N.swf" or MovieName eq "XVII.2-S.swf" or MovieName eq "XVII.2-N.swf" or MovieName eq "XVII.3-S.swf" or MovieName eq "XVII.3-N.swf" or MovieName eq "XVII.4-S.swf" or MovieName eq "XVII.4-N.swf" or MovieName eq "XVII.5-S.swf" or MovieName eq "XVII.5-N.swf" or MovieName eq "XVIII.1-S.swf" or MovieName eq "XVIII.1-N.swf") and Name eq "TAIPU") {
        TextToLoad = "Text_cBar_TAIPU_XVII_" add Language add ".txt";
    } else {
        TextToLoad = "Text_cBar_" add Name add "_" add Language add ".txt";
    }
    loadVariables(TextToLoad, "TextBar");
}
if (eval("TextBar/:cTextOpen") eq "0" and eval("TextBar/:AllowChange") eq "1") {
    if (MovieName eq "XXII.9-N.swf") {
        if (eval("/holder/:Taipu") eq "1" and Name eq "TAIPU") {
            tellTarget ("/holder/Middle-Ground/Taipu/Taipu") {
                gotoAndPlay("talk");
            }
        } else if (eval("/holder/:Hafu") eq "1" and Name eq "HAFU") {
            tellTarget ("/holder/Middle-Ground/Hafu/Hafu") {
                gotoAndPlay("talk");
            }
        } else {
            tellTarget ("TextBar") {
                gotoAndPlay("Open cText");
            }
        }
    } else {
        tellTarget ("TextBar") {
            gotoAndPlay("Open cText");
        }
    }
}
if (eval("TextBar/:AllowChange") eq "1" and eval("TextBar/:cTextOpen") eq "1") {
    MovieName = eval("/:MovieName");
    if (MovieName eq "V.2-N.swf" and Name eq "MAKU" and Quest < "1") {
        tellTarget ("TextBar") {
            gotoAndPlay("Close cText");
        }
        tellTarget ("/") {
            MovieName = "Episode_8-1.swf";
            Sound_ChangeBackgroundMovie = "1";
            Sound_Background = "blank.swf";
            Sound_ChangeSFXMovie = "1";
            Sound_SFXMovie = "blank.swf";
            Music_ChangeMusic = "1";
            Music_Movie = "blank.swf";
            VerticalShift = "1";
            HorizontalShift = "0";
            BeginLoadMovie = "1";
        }
    } else if (MovieName eq "XXII.5-N.swf" and Name eq "TAMARU" and Quest < "2") {
        tellTarget ("TextBar") {
            gotoAndPlay("Close cText");
        }
        tellTarget ("/") {
            MovieName = "Episode_8-2.swf";
            Sound_ChangeBackgroundMovie = "1";
            Sound_Background = "blank.swf";
            Sound_ChangeSFXMovie = "1";
            Sound_SFXMovie = "blank.swf";
            Music_ChangeMusic = "1";
            Music_Movie = "blank.swf";
            VerticalShift = "0";
            HorizontalShift = "1";
            BeginLoadMovie = "1";
        }
    } else if (MovieName eq "XXII.9-N.swf" and Quest < "3") {
        tellTarget ("TextBar") {
            gotoAndPlay("Close cText");
        }
        if (Name eq "HAFU") {
            if (eval("/holder/:Hafu") ne "1") {
                set("/holder/:Hafu", "1");
            }
        } else if (Name eq "TAIPU") {
            if (eval("/holder/:Taipu") ne "1") {
                set("/holder/:Taipu", "1");
            }
        }
        if (eval("/holder/:Hafu") eq "1" and eval("/holder/:Taipu") eq "1") {
            tellTarget ("/") {
                MovieName = "Episode_8-3.swf";
                Sound_ChangeBackgroundMovie = "1";
                Sound_Background = "blank.swf";
                Sound_ChangeSFXMovie = "1";
                Sound_SFXMovie = "blank.swf";
                Music_ChangeMusic = "1";
                Music_Movie = "blank.swf";
                VerticalShift = "0";
                HorizontalShift = "1";
                BeginLoadMovie = "1";
            }
        }
    } else if (MovieName eq "XXII.14-N.swf" and Name eq "KOPEKE" and Quest < "4") {
        tellTarget ("TextBar") {
            gotoAndPlay("Close cText");
        }
        tellTarget ("/") {
            MovieName = "Episode_8-4.swf";
            Sound_ChangeBackgroundMovie = "1";
            Sound_Background = "blank.swf";
            Sound_ChangeSFXMovie = "1";
            Sound_SFXMovie = "blank.swf";
            Music_ChangeMusic = "1";
            Music_Movie = "blank.swf";
            VerticalShift = "0";
            HorizontalShift = "1";
            BeginLoadMovie = "1";
        }
    } else {
        tellTarget ("TextBar") {
            gotoAndPlay("Close cText");
        }
    }
}
