<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.1"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="saxon xs">

  <!-- streamed processing using xsl:iterate and saxon:stream() - early break using xsl:break -->
  
  <xsl:template name="main">
    <out>
      <xsl:iterate select="saxon:stream(doc('../../docs/big-transaction-file.xml')/*/transaction)">
        <xsl:param name="count" as="xs:integer" select="0"/>
        <xsl:param name="total" as="xs:decimal" select="0.00"/>
        <xsl:choose>
          <xsl:when test="$total gt 10.00">
            <count><xsl:value-of select="$count"/></count>
            <total><xsl:value-of select="$total"/></total>
            <date><xsl:value-of select="@date"/></date>
            <xsl:break/>
          </xsl:when>
          <xsl:otherwise>          
            <xsl:next-iteration>
              <xsl:with-param name="count" select="$count + 1"/>
              <xsl:with-param name="total" select="$total + xs:decimal(@value)"/>
            </xsl:next-iteration>
          </xsl:otherwise>
        </xsl:choose>  
      </xsl:iterate>
    </out>
  </xsl:template>
  
</xsl:stylesheet>