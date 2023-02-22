<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xpath-default-namespace="http://www.w3.org/2010/09/qt-fots-catalog"
  xmlns="http://www.w3.org/2010/09/qt-fots-catalog" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math" exclude-result-prefixes="xs math"
  version="3.0">
  <xsl:import href="../tools/test-expand.xsl"/>
  <xsl:template match="expand[@name]/test-case[@UCA]" mode="common" expand-text="yes">
    <xsl:param name="common" as="element()*" tunnel="yes"/>
    <xsl:variable name="collation"
      select="if(@collation)
      then ''''||@collation||''''
      else if(@keys) then 'concat($collation,'||''''||@keys||''')'
      else '$collation'"/>
    <test-case>
      <xsl:attribute name="name" select="concat(../@name,format-number(position(),'-000'))"/>
      <xsl:sequence select="@xml:lang"/>
      <description>{@UCA}</description>
      <xsl:sequence select="$common[not(name() = current()/*/name())]"/>
      <test>compare('{@a}','{@b}',{$collation})</test>
      <result>
        <xsl:choose>
          <xsl:when test="@UCA and @result='ge'">
            <any-of>
              <assert-eq>0</assert-eq>
              <assert-eq>1</assert-eq>
            </any-of>
          </xsl:when>
          <xsl:when test="@result">
            <assert-eq>{@result}</assert-eq>
          </xsl:when>
          <xsl:when test="@error">
            <error code="{@error}"/>
          </xsl:when>
        </xsl:choose>
      </result>
    </test-case>
  </xsl:template>

  <xsl:template match="*:alphabet"/>

</xsl:stylesheet>
