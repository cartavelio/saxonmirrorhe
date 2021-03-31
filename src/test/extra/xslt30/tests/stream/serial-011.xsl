<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="saxon xs">

  <!-- streamed processing using xsl:iterate and saxon:stream() - simple count -->
  
  <xsl:template name="main">
    <out>
      <xsl:iterate select="saxon:stream(doc('xserial001.xml')/*/customer)">
        <xsl:param name="count" as="xs:integer" select="0"/>
        <xsl:on-completion>
          <xsl:value-of select="$count"/>
        </xsl:on-completion>
        <xsl:next-iteration>
          <xsl:with-param name="count" select="$count + 1"/>
        </xsl:next-iteration>
      </xsl:iterate>
    </out>
  </xsl:template>
  
</xsl:stylesheet>