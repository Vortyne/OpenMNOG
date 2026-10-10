#version 4
tellTarget ("/") {
    Movieisloaded = "1";
}
xPohatu = "385";
yPohatu = "400";
tCounter = "1";
BoulderCount = "1";
while (tCounter < "3") {
    duplicateMovieClip("Pohatu", "PohatuFade" add tCounter, "16384" + ("10" + tCounter));
    setProperty("PohatuFade" add tCounter, _alpha, "50");
    tellTarget ("PohatuFade" add tCounter) {
        gotoLabel("00");
    }
    tCounter = tCounter + "1";
}
NumBricks = "9";
NumKnockedOver = "0";
CollObjs = "13";
CollObj1 = "Scorpion/Tail";
CollObj2 = "Scorpion/LArm";
CollObj3 = "Scorpion/RArm";
CollObj4 = "Brick1_1";
CollObj5 = "Brick2_1";
CollObj6 = "Brick3_1";
CollObj7 = "Brick1_2";
CollObj8 = "Brick2_2";
CollObj9 = "Brick3_2";
CollObj10 = "Brick1_3";
CollObj11 = "Brick2_3";
CollObj12 = "Brick3_3";
CollObj13 = "Scorpion/Face";
CollObjHeight1 = "200";
CollObjHeight2 = "30";
CollObjHeight3 = "30";
CollObjHeight4 = "200";
CollObjHeight5 = "200";
CollObjHeight6 = "200";
CollObjHeight7 = "200";
CollObjHeight8 = "200";
CollObjHeight9 = "200";
CollObjHeight10 = "200";
CollObjHeight11 = "200";
CollObjHeight12 = "200";
CollObjHeight13 = "30";
CollObjScorp1 = "1";
CollObjScorp2 = "1";
CollObjScorp3 = "1";
CollObjScorp13 = "1";
tCounter = "4";
while (tCounter < "13") {
    tellTarget (eval("CollObj" add tCounter)) {
        MyNum = eval("../:tCounter");
    }
    tCounter = tCounter + "1";
}
tCounter = "1";
while (tCounter < CollObjs + "1") {
    tellTarget (eval("CollObj" add tCounter)) {
        CollNum = eval("../:tCounter");
    }
    tCounter = tCounter + "1";
}
tCounter = "1";
RemoveCollObj = "0";
AllowShoot = "1";
tShootTime = "1";
xScorpionShift = "5";
yScorpionShift = "1";
yScorpion = getProperty("Scorpion", _y);
LiveBalls = "0";
xPohatuOld5 = xPohatu;
xPohatuOld4 = xPohatu;
xPohatuOld3 = xPohatu;
xPohatuOld2 = xPohatu;
xPohatuOld1 = xPohatu;
