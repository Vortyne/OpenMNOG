#version 4
if (AllowFire eq "1") {
    if ("0" < yMouse - yPohatue) {
        LiveBalls = LiveBalls + "1";
        tellTarget ("Pohatu/Pohatu") {
            gotoAndPlay("Kick");
        }
        duplicateMovieClip("BoulderBase", "Boulder" add BoulderCount, "16384" + ("20" + BoulderCount));
        setProperty("Boulder" add BoulderCount, _x, xPohatu);
        setProperty("Boulder" add BoulderCount, _y, yPohatu - "40");
        set("BoulderSet" add BoulderCount, "1");
        AllowFire = "0";
        setTarget("TimeMovie");
        tCurrentTime = getTimer();
        tTimePassed = tCurrentTime - tShootTime;
        trace(tTimePassed);
        if ("800" < tTimePassed) {
            tTimePassed = "800";
        }
        set("../:tTimePassed", tTimePassed);
        setTarget("");
        tellTarget ("Boulder" add BoulderCount) {
            tTimePassed = eval("../:tTimePassed");
            BoulderNum = eval("../:BoulderCount");
            xMouse = eval("../:xMouse");
            yMouse = eval("../:yMouse");
            tShootTime = "0";
            Velocity = "-1" * (tTimePassed / "20");
        }
        BoulderCount = BoulderCount + "1";
    }
}
