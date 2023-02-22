<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:doc="http://www.saxonica.com/ns/documentation"
  xmlns:H="http://www.w3.org/1999/xhtml" exclude-result-prefixes="xs doc" version="3.0"
  expand-text="yes">

  <xsl:function name="doc:self-doc" as="element()" xmlns="http://www.w3.org/1999/xhtml">
    <xsl:param name="doc" as="document-node()"/>
    <xsl:sequence select="doc:self-doc($doc,())"/>
  </xsl:function>
  <xsl:function name="doc:self-doc" as="element()" xmlns="http://www.w3.org/1999/xhtml">
    <xsl:param name="doc" as="document-node()"/>
    <xsl:param name="CSS" as="xs:string?"/>
    <html>
      <head>
        <title>{($doc//doc:doc/@title)[1]}</title>
        <xsl:if test="$CSS">
          <link rel="stylesheet" href="{$CSS}" type="text/css"/>
        </xsl:if>
      </head>
      <body>
        <img src="saxonica_logo.gif"/>
        <p>{format-dateTime(current-dateTime(),'[D1o] [MNn] [Y] @ [H01]:[m01]')}</p>
        <xsl:apply-templates select="$doc//doc:doc/*" mode="doc:self"/>
      </body>
    </html>
  </xsl:function>
  <xsl:mode name="doc:self" on-no-match="shallow-copy"/>
  <xsl:template match="H:eval" mode="doc:self">
    <xsl:evaluate xpath="@xpath" context-item="."/>
  </xsl:template>

</xsl:stylesheet>
