#version 4
// (Clicks while the list is moving are queued, not lost; clicking the other way takes one back.)
if (Shifting eq "0") {
    ShiftTextDown = "1";
    Shifting = "1";
    DoFadeout = "1";
    ShiftCounter = "0";
} else if (QueuedUp > "0") {
    QueuedUp = QueuedUp - "1";
} else if (QueuedDown < MaxSlots) {
    QueuedDown = QueuedDown + "1";
}
