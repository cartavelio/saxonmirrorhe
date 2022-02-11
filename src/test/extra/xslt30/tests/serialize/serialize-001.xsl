<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon xs" expand-text="yes">

<!-- PURPOSE:  Test saxon:recognize-binary effect on text output method -->
<?dependency saxon?>

  <xsl:output name="one" method="text"/>
  <xsl:output name="two" method="html"/>
  <xsl:output name="three" method="xml"/>
  <xsl:output name="saxon:four" method="xml" omit-xml-declaration="yes"/>

<xsl:variable name="in">
  <html>
    <head>
      <title>Spaghetti</title>
    </head>
    <body>
      <h1>Spaghetti Western</h1>
      <p>with Parmesan Cheese</p>
    </body>
  </html>
</xsl:variable>

<xsl:template name="main">
  <out>
    <one>{saxon:serialize($in, "one")}</one>
    <two>{saxon:serialize($in, "two")}</two>
    <three>{saxon:serialize($in, "three")}</three>
    <four>{saxon:serialize($in, "saxon:four")}</four>
  </out>
</xsl:template>
</xsl:stylesheet>