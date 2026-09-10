Streaming
=========

This is a utility (produced by John Lumley and documented in a Balisage paper) for analyzing the streamability
of an XSLT stylesheet, by applying the W3C streamability rules, producing a graphical browser-based display
of the streamability properties (posture and sweep) of every construct.

It's probably not completely up to date with the latest XSLT 3.0 specification, but it could certainly be
productised at some stage without too much effort. We would need to add some (Selenium-based?) test scripts.

The application as written used Saxon-CE, and the directory did include a copy of Saxon-CE, but I have
dropped this. I have also dropped some of the larger data files (all of which exist elsewhere, e.g. in the
XSLT 3.0 test suite or in XMark), and the file stream.xaw which I assume was a built copy of the application
in some format that I don't recognise.

MHK 2021-01-20 