<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:test="http://example.com/ns/test"
                xmlns:test1="http://example.com/ns/test1">
    
  <xsl:function name="test:function1" visibility="public" as="xs:string">
    <xsl:sequence select="'test:function1'"/>
  </xsl:function>
    
  <xsl:function name="test:function2" visibility="public" as="xs:string">
    <xsl:param name="par" as="xs:string" />
    <xsl:sequence select="test:function1() || '-' || $par"/>
  </xsl:function>
    
  <xsl:function name="test:private-function" visibility="private" as="xs:string">
    <xsl:sequence select="'test:private-function'"/>
  </xsl:function>

  <xsl:function name="test1:function1" as="xs:string">
    <xsl:sequence select="test:private-function() || '-' || 'called'"/>
  </xsl:function>
</xsl:stylesheet>
