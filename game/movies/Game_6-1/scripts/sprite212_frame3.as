#version 4
count = count + "1";
setProperty("Disk", _x, (eval("../:xFireTween") - xOffset) / "4" * count);
setProperty("Disk", _y, (eval("../:yFireTween") - yOffset) / "4" * count);
setProperty("Disk2", _x, (eval("../:xFireTween") - xOffset) / "4" * count - (eval("../:xFireTween") - xOffset) / "16" * "2");
setProperty("Disk2", _y, (eval("../:yFireTween") - yOffset) / "4" * count - (eval("../:yFireTween") - yOffset) / "16" * "2");
setProperty("Disk", _xscale, "100" - (("100" - DiskScale) / "4" * count + "1"));
setProperty("Disk", _yscale, "100" - (("100" - DiskScale) / "4" * count + "1"));
setProperty("Disk2", _xscale, "100" - (("100" - DiskScale) / "4" * count + "1") + ("100" - DiskScale) / "8");
setProperty("Disk2", _yscale, "100" - (("100" - DiskScale) / "4" * count + "1") + ("100" - DiskScale) / "8");
