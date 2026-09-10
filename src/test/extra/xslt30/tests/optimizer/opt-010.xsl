<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test rewrite of a/b/c | a/b/d -->
  
  <!-- MHK 2011-03-04 Not happening in 9.4 because each branch gets rewritten as an index lookup, which is better -->

  <xsl:template match="/">
    <out>
      <xsl:copy-of select="/doc/section/para[@code='A'] | /doc/section/para[@code=('B', 'C')]"/>
    </out>
  </xsl:template>

</xsl:stylesheet>