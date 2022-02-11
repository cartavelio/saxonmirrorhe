<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="saxon xs">

  <!-- streamed processing using xsl:iterate and saxon:stream(). 
  This time mixing attributes and elements -->
  
  <xsl:template name="main">
    <account>
      <xsl:iterate select="saxon:stream(doc('../../docs/transactions.xml')/account/(@nr | transaction))">
      <xsl:param name="accountNr" as="xs:string?"/>
        <xsl:choose>
          <xsl:when test="self::attribute(nr)">
            <xsl:next-iteration>
              <xsl:with-param name="accountNr" select="string(.)"/>
            </xsl:next-iteration>
          </xsl:when>
          <xsl:otherwise>
            <transaction value="{@value}" date="{@date}" account="{$accountNr}"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:iterate>
    </account>
  </xsl:template>
  
  <xsl:output indent="yes"/>
  
</xsl:stylesheet>