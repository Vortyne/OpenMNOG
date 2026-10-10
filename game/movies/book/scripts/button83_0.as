#version 4
// (Clicks while the list is moving are queued, not lost; clicking the other way takes one back.)
if (Shifting eq "0") {
    ShiftTextUp = "1";
    Shifting = "1";
    DoFadeout = "1";
    ShiftCounter = "0";
} else if (QueuedDown > "0") {
    QueuedDown = QueuedDown - "1";
} else if (QueuedUp < MaxSlots) {
    QueuedUp = QueuedUp + "1";
}
