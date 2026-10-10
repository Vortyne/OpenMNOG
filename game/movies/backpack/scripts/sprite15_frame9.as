#version 4
if ("0" < NumStatements) {
    RandNum = random(NumStatements) + "1";
    Text = eval("Statement_" add RandNum);
    tellTarget ("TextBar") {
        Text = eval("../:Text");
    }
} else {
    tellTarget ("TextBar") {
        Text = "";
    }
}
