<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="saxon xs">

  <!-- streamed processing using saxon:iterate and saxon:stream() - cumulative account balance -->
  
  <xsl:template name="main">
    <account>
      <xsl:iterate select="saxon:stream(doc('../../docs/transactions.xml')/account/transaction)">
        <xsl:param name="balance" as="xs:decimal" select="0.00"/>
        <xsl:variable name="new-balance" as="xs:decimal" select="$balance + xs:decimal(@value)"/>
        <transaction balance="{$new-balance}">
           <xsl:copy-of select="@*"/>
        </transaction>
        <xsl:next-iteration>
          <xsl:with-param name="balance" select="$new-balance"/>
        </xsl:next-iteration>
      </xsl:iterate>
    </account>
  </xsl:template>
  
  <xsl:output indent="yes"/>
  
</xsl:stylesheet>