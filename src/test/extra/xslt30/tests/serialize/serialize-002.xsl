<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:saxon="http://saxon.sf.net/" 
  exclude-result-prefixes="saxon" expand-text="yes">

  <!-- PURPOSE:  Test saxon:serialize with xsl:output declaration as second arg -->
  <?dependency saxon?>
  
  <xsl:include href="serialize-002a.xsl"/>

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
      <one>{saxon:serialize($in, $one)}</one>
      <two>{saxon:serialize($in, $two)}</two>
      <three>{saxon:serialize($in, $three)}</three>
      <four>{saxon:serialize($in, $four)}</four>
    </out>
  </xsl:template>
</xsl:stylesheet>
