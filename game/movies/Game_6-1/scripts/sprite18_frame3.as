#version 4
setProperty("Nest", _xscale, eval("../../:NestScale"));
setProperty("Nest", _yscale, eval("../../:NestScale"));
setProperty("Hill", _xscale, eval("../../:NestScale") + "30");
setProperty("Hill", _yscale, eval("../../:NestScale") + "30");
