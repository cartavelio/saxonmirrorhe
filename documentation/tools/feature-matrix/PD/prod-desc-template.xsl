<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:doc="http://www.saxonica.com/ns/documentation"
  xmlns:jwl="http://jwlresearch.net/" expand-text="yes">
  
  <doc:doc title="Saxon Product Description sheet HTML template"/>
  
  <xsl:template match="doc" as="element()" mode="doc"
    doc:doc="The main template for the HTML document">
    <xsl:variable name="header" select="*[@class = 'header']"/>
    <xsl:variable name="footer" select="*[@class = 'footer']"/>
    <html>
      <head>
        <xsl:copy-of select="(.//title)[1]"/>
        <link rel="stylesheet" href="../../{$CSS}" type="text/css"/>
        <!-- The HTMLs are two levels lower -->
      </head>
      <body>
        <xsl:if test="$header">
          <div class="header">
            <xsl:apply-templates select="$header" mode="#current"/>
          </div>
        </xsl:if>
        <!--    <h1 class="license">
          <xsl:value-of select="title"/>
        </h1>-->
        <xsl:apply-templates select="
            * except ($header,
            $footer)"
          mode="#current"/>
        <xsl:if test="$footer">
          <div class="footer">
            <xsl:apply-templates select="$footer" mode="#current"/>
          </div>
        </xsl:if>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>