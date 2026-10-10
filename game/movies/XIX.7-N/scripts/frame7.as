#version 4
Break = "0";
count1 = "0";
while (count1 < "6") {
    count1 = count1 + "1";
    count2 = "0";
    while (count2 < "2") {
        count2 = count2 + "1";
        if (eval("Placed-" add count2 add "-" add count1) eq "0") {
            Break = "1";
            count1 = "6";
        }
    }
}
if (Break eq "1") {
    gotoAndPlay(6);
} else {
    set("/:Kokoro", "5");
    gotoAndPlay(11);
}
