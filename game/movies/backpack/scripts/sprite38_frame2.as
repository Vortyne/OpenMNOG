#version 4
if ("0" < cNum) {
    set("../:cLoadNum", cNum);
    loadMovie(eval("/:Comrade_" add cNum), "cHolder");
}
