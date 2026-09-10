<xsl:stylesheet version="2.0" 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema">
   
   <!-- This was an experimental stylesheet trying to convert the .NET code to Java 
        so that Javadoc could be used to generate the documentation -->
   
<xsl:output method="text"/>

<xsl:variable name="names-doc" select="doc('names.xml')"/> 
<xsl:variable name="api-doc" select="/"/> 

<xsl:key name="n-class-by-name" match="interface | class | enum" use="@name"/>

<xsl:key name="a-class-by-name" match="member[startswith(@name, 'T:')]
      
<xsl:template match="/">
  <xsl:apply-templates select="$names-doc/package/*"/>
</xsl:template>   

<xsl:template match="class | interface | enum">
  <xsl:variable name="shortname" select="@name"/>
  <xsl:variable name="fullname" select="concat('Saxon.Api.', $shortname)"/>
  <xsl:result-document href="{static-base-uri()}/src/{replace($fullname, '.', '/')}.java">
    <xsl:text>package Saxon.Api;&#xa;</xsl:text>
    <xsl:text>&#xa;</xsl:text>
    <xsl:if test="@public = 'true'">public </xsl:if>
    <xsl:if test="@abstract = 'true'">abstract </xsl:if>
    <xsl:value-of select="name()"/>
    <xsl:if test="string(@base)">
      <xsl:variable name="class" select="key('n-class-by-name')"/>
      <xsl:choose>
        <xsl:when test="$class/self::class"> extends </xsl:when>
        <xsl:otherwise> implements </xsl:otherwise>
      </xsl:choose>
      <xsl:value-of select="@base"/>
    <xsl:text> {&#xa;</xsl:text>
    <xsl:apply-templates select="*"/>
  </xsl:result-document>
</xsl:template>
   
 