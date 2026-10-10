#version 4
if (eval("../:AllowChange") eq "1" and eval("../:cTextOpen") eq "1") {
    MovieName = eval("/:MovieName");
    Name = eval("../../:Name");
    if (MovieName eq "V.2-N.swf" and Name eq "MAKU") {
        tellTarget ("../") {
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
    } else if (MovieName eq "XXII.5-N.swf" and Name eq "TAMARU") {
        tellTarget ("../") {
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
    } else if (MovieName eq "XXII.9-N.swf") {
        tellTarget ("../") {
            gotoAndPlay("Close cText");
        }
        if (Name eq "HAFU") {
            if (eval("/holder/:Hafu") eq "1") {
            } else {
                set("/holder/:Hafu", "1");
            }
        } else if (Name eq "TAIPU") {
            if (eval("/holder/:Taipu") eq "1") {
            } else {
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
    } else if (MovieName eq "XXII.14-N.swf" and Name eq "KOPEKE") {
        tellTarget ("../") {
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
        tellTarget ("../") {
            gotoAndPlay("Close cText");
        }
    }
}
