<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- Attempt to use saxon:threads on xsl:for-each with streamed input. See issue 2442. -->
  
  <!-- Current behaviour (Saxon 9.9, 2018-10-05: the test works, but the request for multithreading is ignored -->

  <xsl:output indent="yes"/>

  <xsl:mode streamable="yes"/>

  <xsl:template match="/">
    <out>
      <xsl:for-each select="outermost(//ITEM)" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/"
        exclude-result-prefixes="saxon">
        <title pos="{position()}">
          <xsl:value-of select="copy-of(.)!(TITLE, 'by', AUTHOR)"/>
        </title>
      </xsl:for-each>
    </out>
  </xsl:template>

</xsl:stylesheet>
