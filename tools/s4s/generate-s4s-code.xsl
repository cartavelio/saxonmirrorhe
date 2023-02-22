<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:scm="http://ns.saxonica.com/schema-component-model"
xmlns:xs="http://www.w3.org/2001/XMLSchema"
xmlns:f="http://local-functions/" exclude-result-prefixes="xs scm f">


<xsl:param name="type" required="yes"/>

<xsl:template match="/">
  <xsl:apply-templates select="//scm:complexType[@name=$type or @id=$type]/scm:finiteStateMachine"/>
</xsl:template>

<xsl:template match="*|@*">
  <xsl:copy>
  <xsl:apply-templates select="*|@*"/>
  </xsl:copy>
</xsl:template>

<xsl:template match="@term">
  <xsl:attribute name="term" select="//scm:element[@id=current()]/@name"/>
</xsl:template>

<xsl:template match="scm:finiteStateMachine">
  <xsl:apply-templates/>
  <xsl:value-of select="'private final static int[][] stateTerms = {&#xa;'"/>
  <xsl:value-of select="scm:state/concat('state', @nr, 'terms')" separator=", "/>
  <xsl:value-of select="'};&#xa;'"/>
  <xsl:value-of select="'private final static int[][] stateTargets = {&#xa;'"/>
  <xsl:value-of select="scm:state/concat('state', @nr, 'targets')" separator=", "/>
  <xsl:value-of select="'};&#xa;'"/>
  <xsl:value-of select="'protected boolean isFinalState(int state) { return&#xa;'"/>
  <xsl:for-each select="scm:state[@final='true']">
    <xsl:value-of select="'state ==', @nr"/>
    <xsl:if test="position() ne last()"> || </xsl:if>
  </xsl:for-each>
  <xsl:value-of select="'&#xa;;}&#xa;'"/>       
</xsl:template>
   
   
<xsl:template match="scm:state">
    <xsl:value-of select="concat('private final static int[] state', @nr, 'terms = {')"/>
    <xsl:for-each select="scm:edge">
       <!--<xsl:text>"</xsl:text>-->
       <xsl:value-of select="f:mapNameToConstant(//*[@id=current()/@term])"/>
       <!--<xsl:text>"</xsl:text>-->
       <xsl:if test="position() ne last()">, </xsl:if>
    </xsl:for-each>
    <xsl:text>};&#xa;</xsl:text>
    <xsl:value-of select="concat('private final static int[] state', @nr, 'targets = {')"/>
    <xsl:value-of select="scm:edge/@to" separator=", "/>
    <xsl:text>};&#xa;</xsl:text>    
</xsl:template>

<xsl:function name="f:mapNameToConstant" as="xs:string">
  <xsl:param name="decl" as="element()"/> <!-- element or wildcard -->
  <xsl:choose>
    <xsl:when test="$decl/self::scm:element">  
      <xsl:variable name="name" select="$decl/@name" as="xs:string"/>
      <xsl:sequence select="concat('StandardNames.XS_', upper-case(replace($name, '([A-Z])', '_$1')))"/>
    </xsl:when>
     <xsl:when test="$decl/self::scm:wildcard">  
       <xsl:sequence select="'-1'"/>
     </xsl:when>
    <xsl:otherwise>
        <xsl:message terminate="yes">Expected element or wildcard</xsl:message>
    </xsl:otherwise>
  </xsl:choose>      
</xsl:function>

      
    
  

</xsl:stylesheet>