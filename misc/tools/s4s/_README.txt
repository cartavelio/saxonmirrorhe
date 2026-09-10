s4s
===

The files in this directory are used to process the schema-for-schemas.

Some Java modules in the Saxon schema processor include Java code to implement a finite state machine
for the more complex XSD elements. These finite state machines are generated from the schema for
schema documents (s4sd) in a two-step process:

(a) convert the XSD file (published by W3C) into an SCM file, using the Saxon schema processor itself
(from the command line)

(b) apply the stylesheet generate-s4s-code.xsl to the SCM file (with appropriate parameters)
to generate fragments of Java code.

This only needs to be done if the schema for schema documents changes, which happens once in a blue moon,
so the process is not automated. The generated Java fragments are copied into the source code by hand.

MHK 2021-01-20 