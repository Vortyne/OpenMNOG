#version 4
RandAnnounce = random("100");
if (RandAnnounce eq "37") {
    trace("(Random Update) - NestScale is " add NestScale);
}
if (Shooting eq "1") {
    Shooting = "0";
    AllowFire = "0";
    if (MagNum eq "1") {
        MagNum = "2";
    } else if (MagNum eq "2") {
        MagNum = "1";
    }
    tellTarget ("Hawk/Cannon/SetDisk") {
        gotoAndPlay("Fire");
    }
    tellTarget ("Hawk/Cannon/Sling" add MagNum) {
        gotoAndPlay("Fire");
    }
    tellTarget ("Hawk/Cannon/Magazine" add MagNum) {
        nextFrame();
        play();
    }
}
if (EnemyActiveNum < "0") {
    EnemyActiveNum = "0";
}
if (TransitionRequest eq "1") {
    EnemySpawnAllowed = "0";
    if (KillMsg eq "1") {
        trace("Transition request recieved, Killing all new spawns. Num active:" add EnemyActiveNum);
        KillMsg = "0";
    }
    if (EnemyActiveNum < "1" or "100" < Safetyloop) {
        TransitionRequest = "0";
        tellTarget ("Forward") {
            gotoAndPlay(eval("../:BGAction"));
        }
        KillMsg = "1";
        trace("Transition Successful");
        trace("---------------------------------------");
        trace("BG Section: " add BGAction);
        trace("---------------------------------------");
        Safetyloop = "0";
        EnemyActiveNum = "0";
    } else {
        if (SafetyLoop eq "99") {
            trace("Safetyloop triggered transition, something is wrong.");
        }
        Safetyloop = Safetyloop + "1";
    }
}
if (Lose eq "1") {
    gotoAndPlay(82);
} else if (Win eq "1") {
    gotoAndPlay(77);
} else {
    gotoAndPlay(67);
}
