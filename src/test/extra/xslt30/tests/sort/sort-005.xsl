<xsl:stylesheet version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:udf="user-defined"
    xmlns:saxon="http://saxon.sf.net/"
    exclude-result-prefixes="xs udf saxon">
    
    <!-- Saxon bug submitted by David Rudel, April 2012: test case released as public domain -->

    <xsl:output method="xml" indent="yes"/>
    
    <xsl:template match="/">
      <out>
       	<xsl:variable name="seq1" select="saxon:sort(changes/item, udf:expression('@price',?))"/>
       	<xsl:copy-of select="$seq1"/>
      </out>
    </xsl:template>
  
    <xsl:function name="udf:expression">
      <xsl:param name="expr" as="xs:string"/>
      <xsl:param name="context-item" as="node()"/>
      <xsl:for-each select="$context-item">
        <xsl:evaluate xpath="$expr" context-item="."/>
      </xsl:for-each>
    </xsl:function>
  
  <xsl:function name="saxon:sort">
    <xsl:param name="in" as="item()*"/>
    <xsl:param name="by" as="function(*)"/>
    <xsl:sequence select="sort($in, (), $by)"/>
  </xsl:function>
</xsl:stylesheet>