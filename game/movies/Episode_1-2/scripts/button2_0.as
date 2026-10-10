#version 4
BookOpen = eval("/:BookOpen");
if (BookOpen eq "1") {
    tellTarget ("/mousedrag") {
        gotoAndPlay("mouseturnright");
    }
}
