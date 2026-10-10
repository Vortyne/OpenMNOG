#version 4
if (QueuePick eq "1") {
    count = "0";
    RahiAlive = "0";
    while (count < NumRahi) {
        count = count + "1";
        if ("0" < eval("Rahi" add count add "/:Health")) {
            RahiAlive = "1";
        }
    }
    if (RahiAlive eq "0") {
        gotoAndPlay(10);
    }
}
if (QueuePick eq "1") {
    count = "0";
    TohungaAlive = "0";
    while (count < NumTohunga) {
        count = count + "1";
        if ("0" < eval("Tohunga" add count add "/:Health")) {
            TohungaAlive = "1";
        }
    }
    if (TohungaAlive eq "0") {
        gotoAndPlay(24);
    }
}
if (QueuePick eq "1") {
    AllowAttack = "1";
    Turn = "nobody";
    AttackTime = "100000";
    if (AllowQueue eq "1") {
        count = "0";
        while (count < NumTohunga) {
            count = count + "1";
            AskAttacking = eval("Tohunga" add count add "/:Attacking");
            if (AskAttacking eq "1") {
                AskTime = eval("Tohunga" add count add "/:AttackTime");
                if (AskTime < AttackTime and AskTime < FramesPassed) {
                    AttackTime = AskTime;
                    Turn = "Tohunga" add count;
                }
            }
        }
        count = "0";
        while (count < NumRahi) {
            count = count + "1";
            AskAttacking = eval("Rahi" add count add "/:Attacking");
            if (AskAttacking eq "1") {
                AskTime = eval("Rahi" add count add "/:AttackTime");
                if (AskTime < AttackTime and AskTime < FramesPassed) {
                    AttackTime = AskTime;
                    Turn = "Rahi" add count;
                }
            }
        }
        goto L246;
    }
}
L246:
if (Turn eq "nobody") {
} else {
    QueuePick = "0";
    if (AllowAttack eq "1") {
        AllowAttack = "0";
        tellTarget (Turn) {
            Attack = "1";
        }
    }
}
if (HitCalc eq "1") {
    HitCalc = "0";
    AA = eval(Turn add "/:agility");
    DA = eval(Target add "/:agility");
    ToHit = "50" - AA + DA;
    DiceRoll = random("100") + "1";
    TurnName = eval(Turn add "/:realname");
    TargetName = eval(Target add "/:realname");
    if (DiceRoll < ToHit) {
        TargetHit = "1";
    } else {
        tellTarget (Target) {
            Miss = "1";
        }
    }
}
if (TargetHit eq "1") {
    TargetHit = "0";
    tellTarget (Target) {
        Hit = "1";
    }
    BaseDMG = eval(Turn add "/:BaseDMG");
    VarDMG = eval(Turn add "/:VarDMG");
    TurnType = eval(Turn add "/:Type");
    TargetType = eval(Target add "/:Type");
    ModDMG = eval(Turn add "/:Mod" add TargetType);
    DMG = BaseDMG + random(VarDMG) + ModDmg;
    Bonus = eval(Turn add "/:Bonus");
    if (Bonus eq "1") {
        DMG = DMG * "1.5";
    }
    Defend = eval(Target add "/:Defend");
    if (Defend eq "1") {
        DMG = DMG * "0.6";
    }
    Dmg = int(Dmg);
    if (Dmg < "1") {
        Dmg = "1";
    }
    tellTarget (Target) {
        DmgRecieved = eval("../:Dmg") * "-1";
        Health = Health - eval("../:Dmg");
    }
}
if (ThrowName eq "" and ThrowName2 eq "" and ThrowName3 eq "") {
    xThrowDistance = "0";
    yThrowDistance = "0";
} else {
    if (ThrowName3 eq "") {
    } else {
        setProperty(ThrowName3, _x, xLastThrow2);
        setProperty(ThrowName3, _y, yLastThrow2);
    }
    if (ThrowName2 eq "") {
    } else {
        setProperty(ThrowName2, _x, xLastThrow);
        setProperty(ThrowName2, _y, yLastThrow);
        xLastThrow2 = getProperty(ThrowName2, _x);
        yLastThrow2 = getProperty(ThrowName2, _y);
    }
    if (ThrowName eq "") {
    } else {
        if (xThrowDistance eq "0" and yThrowDistance eq "0") {
            xThrowDistance = (getProperty(ThrowName, _x) - getProperty(ThrowTarget, _x)) / ThrowFrames;
            yThrowDistance = (getProperty(ThrowName, _y) - getProperty(ThrowTarget, _y)) / ThrowFrames;
        }
        setProperty(ThrowName, _x, getProperty(ThrowName, _x) - xThrowDistance);
        setProperty(ThrowName, _y, getProperty(ThrowName, _y) - yThrowDistance);
        xLastThrow = getProperty(ThrowName, _x);
        yLastThrow = getProperty(ThrowName, _y);
    }
}
if (ReturnName eq "") {
    xReturnDistance = "0";
    yReturnDistance = "0";
} else {
    if (xReturnDistance eq "0" and yReturnDistance eq "0") {
        xReturnDistance = (xReturnTarget - getProperty(ReturnName, _x)) / ReturnFrames;
        yReturnDistance = (yReturnTarget - getProperty(ReturnName, _y)) / ReturnFrames;
    }
    setProperty(ReturnName, _x, getProperty(ReturnName, _x) + xReturnDistance);
    setProperty(ReturnName, _y, getProperty(ReturnName, _y) + yReturnDistance);
}
if (LastClick eq "") {
    if (("0" < eval("Tohunga1/:Attacking") or eval("Tohunga1/:Health") < "1") and ("0" < eval("Tohunga2/:Attacking") or eval("Tohunga2/:Health") < "1") and ("0" < eval("Tohunga3/:Attacking") or eval("Tohunga3/:Health") < "1") and ("0" < eval("Tohunga4/:Attacking") or eval("Tohunga4/:Health") < "1") and ("0" < eval("Tohunga5/:Attacking") or eval("Tohunga5/:Health") < "1") and ("0" < eval("Tohunga6/:Attacking") or eval("Tohunga6/:Health") < "1")) {
        tellTarget ("Indicator") {
            gotoAndPlay("NoText");
        }
    } else {
        tellTarget ("Indicator") {
            gotoAndPlay("SelectTohunga");
        }
    }
} else {
    tellTarget ("Indicator") {
        gotoAndPlay("SelectTarget");
    }
}
