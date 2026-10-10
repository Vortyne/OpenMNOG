#version 4
DownloadMovieName = eval("TextSlot" add CurrentTextSlot) add ".zip";
getURL(DownloadMovieName, "_self");
