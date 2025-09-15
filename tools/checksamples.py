import re
import sys

def check_against_map(expect, output):
    found = {}

    lines = []
    with open(output, "r") as samples:
        for line in samples.readlines():
            lines.append(line.rstrip())

    while len(lines) > 0:
        line = lines[0]
        lines = lines[1:]
        title = re.match(r"^====\s*(.+?)\s*====\s*$", line)
        if title:
            found[title.group(1)] = 1
            lines = check(expect, title.group(1), lines)
        else:
            pass

    for title in expect:
        if title not in found:
            raise RuntimeError(f"Expected sample not found: {title}")


def check_jaxpexamples(output):
    check_against_map(
        {"ParseOnly": ["", "Parsing complete"],
         "Simple1": [r"^<!DOCTYPE HTML", r"^<html>", r"^is here-->", r"\s*<head>"],
         "Simple2": ["", "Output written to Simple2.out"],
         "FromStream": [r"^<!DOCTYPE HTML", r"^<html>", r"^is here-->", r"\s*<head>"],
         "FromReader": [r"^<!DOCTYPE HTML", r"^<html>", r"^is here-->", r"\s*<head>"],
         "UseTemplatesObj": ["", "", r"-+ transform of data",
                             r"^<!DOCTYPE HTML", r"^<html>", r"^is here-->", r"\s*<head>"],
         "UseTemplatesHandler": [r"^<\?xml.*[Uu][Tt][Ff]-8",
                                 r"<XXhtml>", r"^is here-->", r"\s*<XXhead>"],
         "SAXResult": ["setDocumentLocator", "startDocument", "startElement:"],
         "ContentHandlerToContentHandler": ["setDocumentLocator", "startDocument", "startElement:"],
         "XMLReader": ["setDocumentLocator", "startDocument", "startElement:"],
         "XMLFilter": ["setDocumentLocator", "startDocument", "startElement:"],
         "XMLFilterChain": ["setDocumentLocator", "startDocument", "startElement: , summary, summary"],
         "DOMsubtree": [r"^<\?xml.*[Uu][Tt][Ff]-8.*<BOOKS xmlns",
                        r"\s*<ITEM CAT=.MMP.>", r"\s*<TITLE>"],
         "DOMtoDOM": [r"^<!DOCTYPE HTML", r"^<html>", r"^is here-->", r"\s*<head>"],
         "Param": ["", "Transform 1 and 2 use different transformers", "", "",
                   "----- Transform 1: set top-author parameter"],
         "TransformerReuse": ["", "Transform 1 and 2 use the same transformer", "", "",
                              "----- Transform 1: set top-author parameter"],
         "OutputProperties": [r"^<!DOCTYPE HTML", r"^is here-->", r"\s*\$14.78"],
         "UseAssociated": [r"^<!DOCTYPE HTML", r"^<html>", r"^is here-->", r"\s*<head>"],
         "ContentHandlertoDOM": [r"^<!DOCTYPE HTML", r"^<html>", r"^is here-->", r"\s*<head>"],
         "AsSerializer": [r"^<\?xml-stylesheet.*><BOOKLIST", "", r"\s*<BOOKS>"],
         "NewDOMSerializer": [r"^<\?xml.*[Uu][Tt][Ff]-8", r"^<x xmlns=.nsx\.uri./>"],
         "UsingURIResolver": [r"^<!DOCTYPE HTML", r"^<html>", r"^is here-->", r"\s*<head>"],
         "done!": []
     }, output)

def check(expect, title, lines):
    #print(title)
    #for line in lines:
    #    print(line)
    if title in expect:
        for regex in expect[title]:
            line = lines[0]
            lines = lines[1:]
            if not re.match(regex, line):
                raise RuntimeError(f"Unexpected output for {title} ([{regex}] != [{line}])")
    else:
        raise RuntimeError(f"Unexpected sample: {title}!")

    return lines

def check_applyxpathjaxp_dom(output):
    check_applyxpathjaxp(output, "dom")

def check_applyxpathjaxp_saxon(output):
    check_applyxpathjaxp(output, "saxon")

def check_applyxpathjaxp(output, om):
    lines = []
    with open(output, "r") as samples:
        for line in samples.readlines():
            lines.append(line.rstrip())

    expect = {"n/a": [r"^Loading classes,", f"^Querying {om}",
                      r"^<output>", r"String Value => Pride and Prejudice",
                      r"^</output>"]}

    lines = check(expect, "n/a", lines)


def check_examplecontenthandler(output):
    lines = []
    with open(output, "r") as samples:
        for line in samples.readlines():
            lines.append(line.rstrip())

    expect = {"n/a": [r"^setDocumentLocator", r"^startDocument",
                      r"^processingInstruction: xml-stylesheet",
                      r"^startElement.*BOOKLIST"]}

    lines = check(expect, "n/a", lines)


def check_s9apiexamplesom_pe(output):
    check_against_map(
        {"XPathDOM": ["", "Pride and Prejudice", "Wuthering Heights"],
         "XPathJDOM": ["", "Pride and Prejudice", "Wuthering Heights"],
         "XPathXOM": ["", r"^<TITLE>Pride and Prejudice<", r"<TITLE>Wuthering Heights<"],
         "XPathDOM4J": ["", r"^<TITLE>Pride and Prejudice<", r"<TITLE>Wuthering Heights<"],
         }, output)


def check_s9apiexamplesom_ee(output):
    check_against_map(
        {"XPathDOM": ["", "Pride and Prejudice", "Wuthering Heights"],
         "XPathJDOM": ["", "Pride and Prejudice", "Wuthering Heights"],
         "XPathXOM": ["", r"^<TITLE>Pride and Prejudice<", r"<TITLE>Wuthering Heights<"],
         "XPathDOM4J": ["", r"^<TITLE>Pride and Prejudice<", r"<TITLE>Wuthering Heights<"],
         }, output)


def check_s9apiexamples_he(output):
    check_against_map(
        {"QueryA": ["", r"^<a b=.c.>7<"],
         "QueryB": ["", r"Total: 385"],
         "QueryC": ["", "apple: true", "banana: false", "cherry: true"],
         "QueryD": ["", r"<\?xml.*<out><e>11<.*<e>12<.*<e>13"],
         "QueryE": ["", r"Start element {}copy", r"^Start element {}ITEM"],

         "TransformA": ["", "Output written to books.html"],
         "TransformB": ["", "Output written to tour.html"],
         "TransformC": ["", "apple: true", "banana: false", "cherry: true"],
         "TransformD": ["", "103"],
         "TransformE": ["", r"^<!DOCTYPE HTML.*", r"^<html>", r"is here", r"^.*<head>"],
         "TransformF": ["", r"^<\?xml", r"^.*<exists>false<"],
         "TransformG": ["", "BOOKLIST/BOOKS/ITEM/TITLE", "BOOKLIST/BOOKS/ITEM/AUTHOR"],
         "XPathA": ["", r"^TITLE\(7\): Pride and Prejudice", r"^TITLE\(20\): Wuthering Heights"],
         "XPathB": ["", "Result: 6.11"],
         "XPathDOM": ["", "Pride and Prejudice", "Wuthering Heights"],
         "SerializeA": ["", r"^<a ", r"^.*<z xmlns=../>"],
         "SerializeB": ["", r"<d>4</d>", r"<c>3</c>", r"<b>2</b>"],
         "SerializeC": ["", r"Serialized result: 5 6 7 8 9 10 & 20"],
         "SerializeD": ["", r"Serialized result: <\?xml.*", r"<doc xmlns", r"^.*<heading"],
         "NavigationA": ["", r"^TITLE\(7\): Pride and Prejudice", r"^TITLE\(20\): Wuthering Heights"],
         "NavigationD": ["", r"^Tess of the .*", r"^Jude the Obscure"]
         }, output)


def check_s9apiexamples_pe(output):
    check_against_map(
        {"QueryA": ["", r"^<a b=.c.>7<"],
         "QueryB": ["", r"Total: 385"],
         "QueryC": ["", "apple: true", "banana: false", "cherry: true"],
         "QueryD": ["", r"<\?xml.*<out><e>11<.*<e>12<.*<e>13"],
         "QueryE": ["", r"Start element {}copy", r"^Start element {}ITEM"],
         "QueryF": ["", "Locale: Italian"],

         "TransformA": ["", "Output written to books.html"],
         "TransformB": ["", "Output written to tour.html"],
         "TransformC": ["", "apple: true", "banana: false", "cherry: true"],
         "TransformD": ["", "103"],
         "TransformE": ["", r"^<!DOCTYPE HTML.*", r"^<html>", r"is here", r"^.*<head>"],
         "TransformF": ["", r"^<\?xml", r"^.*<exists>false<"],
         "TransformG": ["", "BOOKLIST/BOOKS/ITEM/TITLE", "BOOKLIST/BOOKS/ITEM/AUTHOR"],
         "XPathA": ["", r"^TITLE\(7\): Pride and Prejudice", r"^TITLE\(20\): Wuthering Heights"],
         "XPathB": ["", "Result: 6.11"],
         "XPathDOM": ["", "Pride and Prejudice", "Wuthering Heights"],
         "SerializeA": ["", r"^<a ", r"^.*<z xmlns=../>"],
         "SerializeB": ["", r"<d>4</d>", r"<c>3</c>", r"<b>2</b>"],
         "SerializeC": ["", r"Serialized result: 5 6 7 8 9 10 & 20"],
         "SerializeD": ["", r"Serialized result: <\?xml.*", r"<doc xmlns", r"^.*<heading"],
         "NavigationA": ["", r"^TITLE\(7\): Pride and Prejudice", r"^TITLE\(20\): Wuthering Heights"],
         "NavigationD": ["", r"^Tess of the .*", r"^Jude the Obscure"]
         }, output)


def check_s9apiexamples_ee(output):
    check_against_map(
        {"QueryA": ["", r"^<a b=.c.>7<"],
         "QueryB": ["", r"Total: 385"],
         "QueryC": ["", "apple: true", "banana: false", "cherry: true"],
         "QueryD": ["", r"<\?xml.*<out><e>11<.*<e>12<.*<e>13"],
         "QueryE": ["", r"Start element {}copy", r"^Start element {}ITEM"],
         "QueryF": ["", "Locale: Italian"],
         "QueryG": ["", "Result1: 12", "Result2: 8"],

         "QueryUpdateA": ["", r"^.*<?xml-stylesheet.*",
                          r" *xsi:noNamespaceSchemaLocation=.books.xsd.>",
                          r"^ *<BOOKS>", r"^ *<ITEM CAT"],
         # FIXME: more to QueryUpdateA surely?
         
         "TransformA": ["", "Output written to books.html"],
         "TransformB": ["", "Output written to tour.html"],
         "TransformC": ["", "apple: true", "banana: false", "cherry: true"],
         "TransformD": ["", "103"],
         "TransformE": ["", r"^<!DOCTYPE HTML.*", r"^<html>", r"is here", r"^.*<head>"],
         "TransformF": ["", r"^<\?xml", r"^.*<exists>false<"],
         "TransformG": ["", "BOOKLIST/BOOKS/ITEM/TITLE", "BOOKLIST/BOOKS/ITEM/AUTHOR"],
         "SchemaA": ["", "First schema validation succeeded.*", "Second schema validation failed.*",
                     "In.*ITEM.*OLD-TITLE.*TITLE.*"],
         "SchemaB": ["", "Validation succeeded"],
         "XPathA": ["", r"^TITLE\(7\): Pride and Prejudice", r"^TITLE\(20\): Wuthering Heights"],
         "XPathB": ["", "Result: 6.11"],
         "XPathC": ["", r"^TITLE\(7\): Pride and Prejudice", r"^TITLE\(20\): Wuthering Heights"],
         "XPathDOM": ["", "Pride and Prejudice", "Wuthering Heights"],
         "SerializeA": ["", r"^<a ", r"^.*<z xmlns=../>"],
         "SerializeB": ["", r"<d>4</d>", r"<c>3</c>", r"<b>2</b>"],
         "SerializeC": ["", r"Serialized result: 5 6 7 8 9 10 & 20"],
         "SerializeD": ["", r"Serialized result: <\?xml.*", r"<doc xmlns", r"^.*<heading"],
         "NavigationA": ["", r"^TITLE\(7\): Pride and Prejudice", r"^TITLE\(20\): Wuthering Heights"],
         "NavigationB": ["", "Result: 6.11"],
         "NavigationC": ["", r"^TITLE\(7\): Pride and Prejudice", r"^TITLE\(20\): Wuthering Heights"],
         "NavigationD": ["", r"^Tess of the .*", r"^Jude the Obscure"]
         }, output)


def check_jdom2example(output):
    lines = []
    with open(output, "r") as samples:
        for line in samples.readlines():
            lines.append(line.rstrip())

    expect = {"n/a": [r"^<!DOCTYPE HTML.*", r"^<html>", r"^is here.*",
                      r"^.*<head>"]}

    lines = check(expect, "n/a", lines)


def check_pullexamples(output):
    check_against_map(
        {"Serialize the input to the output": ["", r"^<\?xml.*", r"<\?xml-stylesheet",
                                               "<BOOKLIST .*"],
         "Validate the input": ["", "Done."],
         "Transform the input to the output": ["", "<!DOCTYPE HTML.*", r"^<html>", "^is here", "^.*<head>"],
         "Run XQuery against the input": ["", r"<\?xml.*", "<book>", ".*<author>"],
         "Remove PRICE elements from the input": ["", r"^<\?xml.*", r"<\?xml-stylesheet",
                                                  r"<BOOKLIST.*", r"^.*xsi:", r"^\s*<BOOKS>",
                                                  r"\s*<ITEM.*", r"\s*<TITLE>", r"\s*<AUTHOR>",
                                                  r"\s*<PUBLISHER>", r"\s*<PUB-DATE>", r"\s*<LANGUAGE>",
                                                  r"\s*<QUANTITY>", r"\s*<ISBN>", r"\s*<PAGES>", 
                                                  r"\s*<DIMENSIONS", r"\s*<WEIGHT"],
         "Compute average of PRICE elements in the input": ["", r"<result>11.1749.*"],
         "Obtain query results using a pull iterator": ["", r"<\?xml.*", r"^\s*<e .*", r"^\s*<f "],
         "Obtain query results using a pull iterator on a 'standard' tree": ["", r"<\?xml .*",
                                                                             "<CATEGORIES "],
         }, output)


def check_schemavalidatorhandler(output):
    lines = []
    with open(output, "r") as samples:
        for line in samples.readlines():
            lines.append(line.rstrip())

    expect = {"n/a": [r"^Element BOOKLIST of type .*BOOKLIST_anonymous",
                      r"^\s*Attribute xsi:noNamespaceSchemaLocation \(specified\).*",
                      r"^\s*Element BOOKS of type .*BOOKS_anonymous"]}

    lines = check(expect, "n/a", lines)


def check_xpathexamplesa(output):
    lines = []
    with open(output, "r") as samples:
        for line in samples.readlines():
            lines.append(line.rstrip())

    expect = {"n/a": ["Pride and Prejudice", "Wuthering Heights", "The Big Over Easy"]}

    lines = check(expect, "n/a", lines)


def check_xqjexamples(output):
    check_against_map(
        {"TestA": ["", r"^<a b=.c.>7<"],
         "TestB": ["", r"Total: 385"],
         "TestC": ["", "banana: false", "apple: true", "orange: true", "^Failed as expected.*"],
         "TestD": ["", "banana: false", "apple: true", "orange: true"],
         "TestE": ["", "DOM node name: a", "DOM node namespace: http://a/uri", "DOM node children: 1"],
         "TestF": ["", "setDocumentLocator", "startDocument",
                   "startPrefixMapping: .*", "startPrefixMapping: .*",
                   "startElement:.*"],
         "TestG": ["", "<a xmlns=.*", r"^\s*<b>4<"],
         "TestH": ["", "1 peach 1951-10-11 PT5M 93.7 <the end>"],
         "TestI": ["", r"^start {.*", r"^\s*start namespace .*", r"^\s*start namespace .*"],
         "TestJ": ["", "count: 24"],
         "TestK": ["", r"<BOOKS>", r"^\s*<ITEM "],
         "TestL": ["", r"Locale: Italian"],
         "TestZ": ["", r"Output written to testZ.out"]
         }, output)


def check_xmarkbenchmark(output):
    lines = []
    with open(output, "r") as samples:
        for line in samples.readlines():
            lines.append(line.rstrip())

    expect = {"n/a": [r"^<xmark-results .*", r"^\s*<file name=.xmark1.xml'"]}

    lines = check(expect, "n/a", lines)

