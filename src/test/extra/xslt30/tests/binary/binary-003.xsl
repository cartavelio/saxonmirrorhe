<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon xs">

<!-- PURPOSE:  Test saxon:base64Binary output method -->
<?dependency saxon?>

<xsl:output method="xml" encoding="iso-8859-1"/>

<xsl:template name="main">
  <html>
    <head>
      <title>An image</title>
    </head>
    <body>
      <h1>An image</h1>
      <p><img src="image003.gif"/></p>
    </body>
  </html>
  <xsl:result-document method="saxon:base64Binary" href="image003.gif" media-type="image/gif">
    <xsl:text>iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAYAAACNMs+9AAAABGdBTUEAALGPC/xhBQAAAAlwSFlz
        AAALEwAACxMBAJqcGAAAAAd0SU1FB9YGARc5KB0XV+IAAAAddEVYdENvbW1lbnQAQ3JlYXRlZCB3aXRoIF
        RoZSBHSU1Q72QlbgAAAF1JREFUGNO9zL0NglAAxPEfdLTs4BZM4DIO4C7OwQg2JoQ9LE1exdlYvBBeZ7jq
        ch9//q1uH4TLzw4d6+ErXMMcXuHWxId3KOETnnXXV6MJpcq2MLaI97CER3N0vr4MkhoXe0rZigAAAABJRU
        5ErkJggg==</xsl:text>
  </xsl:result-document>
</xsl:template>
</xsl:stylesheet>