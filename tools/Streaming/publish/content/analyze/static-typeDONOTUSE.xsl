<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:s="StreamAnalysis" exclude-result-prefixes="xs math fos" version="3.0">


  <xsl:mode name="s:staticType" on-no-match="shallow-copy"/>

  <xsl:template match="*" mode="s:staticType">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:attribute name="s:staticType"
        select="concat('element(Q{',namespace-uri(.),'}',local-name(.),')')"/>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <!-- <xsl:function name="xp:sequence-constructor" as="xs:boolean">
    <xsl:param name="item" as="node()"/>
    <xsl:sequence
      select="$item/empty(self::xp:*|
            self::xsl:param|self::xsl:with-param|
            self::xsl:sort|
            self::xsl:next-iteration|self::xsl:on-completion)"
    />
  </xsl:function>
-->
  <xsl:template match="xsl:*| s:sequence-constructor" mode="s:staticType">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:variable name="children" as="item()*">
        <xsl:apply-templates select="*|text()" mode="#current"/>
      </xsl:variable>
      <xsl:attribute name="s:staticType">item()</xsl:attribute>
      <!--<xsl:attribute name="s:staticType"
                select="xp:composite-type-cardinal(if(self::s:sequence-constructor) then
                $children/@s:staticType
                else $children[self::s:sequence-constructor]/@s:staticType)"/>-->
      <xsl:copy-of select="$children"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xsl:iterate | xsl:for-each" mode="s:staticType">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:variable name="children" as="item()*">
        <xsl:apply-templates select="*|text()" mode="#current"/>
      </xsl:variable>
      <xsl:attribute name="s:staticType"
        select="xp:composite-type-multiple($children[xp:is.sequence-constructor(.)]/@s:staticType)"/>
      <xsl:copy-of select="$children"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xsl:stylesheet|xsl:transform" mode="s:staticType">
    <xsl:copy>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xsl:text | xsl:value-of" mode="s:staticType">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:attribute name="s:staticType">xs:string</xsl:attribute>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xsl:copy-of|xsl:param|xsl:with-param" mode="s:staticType">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:variable name="children" as="item()*">
        <xsl:apply-templates select="*|text()" mode="#current"/>
      </xsl:variable>
      <xsl:attribute name="s:staticType" select="xp:composite-type($children/@s:staticType)"/>
      <xsl:sequence select="$children"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xsl:if" mode="s:staticType">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:variable name="children" as="item()*">
        <xsl:apply-templates select="*|text()" mode="#current"/>
      </xsl:variable>
      <xsl:attribute name="s:staticType"
        select="xp:composite-type-possible($children[xp:sequence-constructor(.)]/@s:staticType)"/>
      <xsl:sequence select="$children"/>
    </xsl:copy>
  </xsl:template>




  <xsl:template match="xp:*" mode="s:staticType">
    <xsl:copy>
      <xsl:attribute name="s:staticType" select="$staticTypes(local-name(.))" on-empty="()"/>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xp:Root" mode="s:staticType">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="s:staticType">document-node()</xsl:attribute>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xp:Literal" mode="s:staticType">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="s:staticType" select="@type"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xp:FunctionCall" mode="s:staticType">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:try>
        <xsl:attribute name="s:staticType" select="$functions(@name)('returnType')"/>
        <xsl:catch>
          <xsl:attribute name="noTypeFor" select="@name||'returnType'"/>
        </xsl:catch>
      </xsl:try>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:UnionExpr" mode="s:staticType">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="s:staticType" select="xp:composite-type($temp/@s:staticType)"/>
      <xsl:sequence select="$temp"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:Expr" mode="s:staticType">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="s:staticType" select="xp:composite-type-cardinal($temp/@s:staticType)"/>
      <xsl:sequence select="$temp"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xp:IfExpr" mode="s:staticType">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="s:staticType"
        select="xp:composite-type($temp[@s:op.role=('then','else')]/@s:staticType)"/>
      <xsl:sequence select="$temp"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:AxisStep" mode="s:staticType">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="$temp[1]/@s:staticType,$temp"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template
    match="xp:RelativePathExpr | xp:PathExpr |
        xp:ForwardStep | xp:ReverseStep |
        xp:StepExpr"
    mode="s:staticType">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="s:staticType"
        select="(@nodeTest,($temp[not(self::xp:PredicateList)])[last()]/@s:staticType)[1]"/>
      <xsl:sequence select="$temp"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xp:LetExpr" mode="s:staticType">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="s:staticType" select="$temp[last()]/@s:staticType"/>
      <xsl:sequence select="$temp"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xp:ForExpr|xp:SimpleMapExpr" mode="s:staticType">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="s:staticType"
        select="let $t := $temp[last()]/@s:staticType return if(matches($t,'\*$|\?$')) then $t else concat($t,'*')"/>
      <xsl:copy-of select="$temp"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xp:IntersectExceptExpr" mode="s:staticType">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:sequence select="$temp[1]/@s:staticType,$temp"/>
    </xsl:copy>
  </xsl:template>
  <!--    <xsl:template match="xp:axis" mode="s:staticType">
        <xsl:copy>
            <xsl:sequence select="@*"/>
            <xsl:attribute name="s:staticType" select="@nodeTest"/>
        </xsl:copy>
    </xsl:template>-->

  <!--    <xsl:template match="xsl:value-of" mode="s:staticType">
        <xsl:copy>
            <xsl:attribute name="s:staticType">xs:string</xsl:attribute>
            <xsl:apply-templates select="@*|*|text()" mode="#current"/>
        </xsl:copy>
    </xsl:template>-->


</xsl:stylesheet>
