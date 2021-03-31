<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="saxon xs">

  <!-- streamed processing using xsl:iterate and saxon:stream() - early exit. This time using an attribute -->
  
  <xsl:template name="main">
    <account>
      <xsl:iterate select="saxon:stream(doc('../../docs/transactions.xml')/account/@nr)">
        <xsl:attribute name="nr" select="."/>
        <xsl:break/>
      </xsl:iterate>
    </account>
  </xsl:template>
  
  <xsl:output indent="yes"/>
  
</xsl:stylesheet>