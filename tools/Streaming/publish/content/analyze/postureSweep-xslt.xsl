<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:X="IndirectXSLT" xmlns:s="StreamAnalysis" exclude-result-prefixes="xs math fos"
  version="3.0">


  <xsl:template name="init.xsl" doc:doc="Initialise the attribute properties for an XSL element">
    <xsl:apply-templates select="@*" mode="#current"/>
    <xsl:call-template name="stream-rule-xsl"/>
    <xsl:call-template name="record-contextPosture"/>
  </xsl:template>

  <xsl:function name="xp:role.AVT" as="xs:boolean">
    <xsl:param name="val" as="xs:string?"/>
    <xsl:sequence select="starts-with($val,'AVT')"/>
  </xsl:function>
  <xsl:function name="xp:role.select" as="xs:boolean">
    <xsl:param name="node" as="node()"/>
    <xsl:sequence select="$node/@s:role='select'"/>
  </xsl:function>
  <xsl:function name="xp:role" as="xs:boolean">
    <xsl:param name="node" as="node()"/>
    <xsl:param name="role" as="xs:string"/>
    <xsl:sequence select="$node/@s:role=$role"/>
  </xsl:function>
  <xsl:function name="xp:active" as="element()*"
    doc:doc="Return the active (@select, sequence-constructor) elements for a sequence of elements">
    <xsl:param name="nodes" as="element()*"/>
    <xsl:sequence select="$nodes/(*[xp:role.select(.)]|s:sequence-constructor)"/>
  </xsl:function>
  <xsl:function name="xp:AVT" as="element()*"
    doc:doc="Return the active AVT elements for a sequence of elements">
    <xsl:param name="nodes" as="element()*"/>
    <xsl:sequence select="$nodes/*[xp:role.AVT(.)]"/>
  </xsl:function>



  <xsl:template match="xsl:stream|xsl:template" mode="addPosture" priority="1.5">
    <xsl:variable name="temp" as="element()">
      <xsl:next-match/>
    </xsl:variable>
    <xsl:for-each select="$temp">
      <xsl:variable name="grounded-sequence"
        select="if(s:sequence-constructor) then s:sequence-constructor/@s:posture='grounded' else true()"/>
      <xsl:variable name="motionless-match"
        select="if(@match) then xp:*/@s:sweep='motionless' else true()"/>
      <xsl:copy>
        <xsl:sequence select="@*"/>
        <xsl:attribute name="s:streamable" select="$grounded-sequence and $motionless-match"/>
        <xsl:if test="self::xsl:stream">
          <xsl:attribute name="s:posture">grounded</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:if>
        <xsl:sequence select="*|text()"/>
      </xsl:copy>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="xsl:stylesheet|xsl:transform" mode="addPosture">
    <xsl:param name="streaming-modes" as="xs:string*" select="()" tunnel="yes"/>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:apply-templates select="*|text()" mode="#current">
        <xsl:with-param name="streaming-modes"
          select="$streaming-modes,
          .//xsl:mode[@streamable='yes']/string((@name,'#default')[1])"
          as="xs:string*" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xsl:template" mode="addPosture">
    <xsl:param name="streaming-modes" as="xs:string*" select="()" tunnel="yes"/>
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:call-template name="stream-rule-xsl"/>
      <xsl:apply-templates select="*|text()" mode="#current">
        <xsl:with-param name="contextPosture"
          select="if((@mode,'#default')[1]=$streaming-modes) then 'striding' else 'roaming'"
          tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>



  <xsl:template
    match="
    xsl:variable |
    xsl:copy|
    xsl:copy-of|
    xsl:value-of|
    xsl:result-document|
    xsl:element |
    xsl:attribute |
    xsl:namespace |
    xsl:comment |
    xsl:text |
    xsl:sequence |
    xsl:if"
    mode="addPosture">
    <xsl:call-template name="general-streamability">
      <xsl:with-param name="spec-rule" as="attribute()">
        <xsl:call-template name="stream-rule-xsl"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="xsl:choose" mode="GS-find-operands">
    <xsl:sequence select="*/(*[@s:role='test']|s:sequence-constructor)"/>
  </xsl:template>
  <xsl:template match="xsl:choose" mode="addPosture">
    <xsl:copy>
      <xsl:call-template name="init.xsl"/>
      <!--      <xsl:variable name="operands" select="*/*[@s:role='test' or self::s:sequence-constructor]/@id"/>
-->
      <xsl:call-template name="general-streamability.operands.ids">
        <!--        <xsl:with-param name="operands" select="$operands"/>
 -->
      </xsl:call-template>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xsl:when|xsl:otherwise" mode="addPosture">
    <xsl:copy>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>




  <!-- TODO - more general sequence constructor -->
  <xsl:template match="s:sequence-constructor[count(* except xsl:variable)=1]" priority="1" mode="addPosture">
    <xsl:copy>
      <xsl:call-template name="init.xsl"/>
      <xsl:variable name="children" as="item()*">
        <xsl:apply-templates select="*|text()" mode="#current"/>
      </xsl:variable>
      <xsl:sequence select="$children[not(self::xsl:variable)]/(@s:posture|@s:sweep)"/>
      <xsl:copy-of select="$children"/>
    </xsl:copy>
  </xsl:template>


  <xsl:template match="xsl:for-each" mode="GS-find-operands">
    <xsl:sequence select="xp:active(.)|xp:active(xsl:sort)|xp:AVT(xsl:sort)"/>
  </xsl:template>
  <xsl:template match="xsl:for-each" mode="GS-higher-order-operands">
    <xsl:sequence select="s:sequence-constructor|xp:active(xsl:sort)"/>
  </xsl:template>

  <xsl:template match="xsl:for-each" mode="addPosture">
    <xsl:param name="debug" as="xs:string?" tunnel="yes"/>
    <xsl:copy>
      <xsl:call-template name="init.xsl"/>
      <xsl:variable name="select" select="*[xp:role.select(.)]"/>
      <xsl:assert test="count($select) eq 1"/>
      <xsl:variable name="select.postured" as="element()">
        <xsl:apply-templates select="$select" mode="#current"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$select.postured/@s:posture='grounded'">
          <xsl:call-template name="general-streamability.operands.ids">
            <xsl:with-param name="context-postures" as="map(xs:string,item()*)"
              select="map:new((map:entry(s:sequence-constructor/@id,'grounded'),
                  xp:active(xsl:sort)/@id ! map:entry(.,'grounded')))"
            />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="xsl:sort">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="s" as="element()">
            <xsl:apply-templates select="s:sequence-constructor" mode="#current">
              <xsl:with-param name="contextPosture" select="$select.postured/@s:posture"
                tunnel="yes"/>
            </xsl:apply-templates>
          </xsl:variable>
          <xsl:attribute name="s:posture" select="$s/@s:posture"/>
          <xsl:if test="$select.postured/@s:sweep and $s/@s:sweep">
            <xsl:try>
              <xsl:attribute name="s:sweep" select="$wider($select.postured/@s:sweep)($s/@s:sweep)"/>
              <xsl:catch>
                <xsl:message
                  select="$debug,'Error in for-each sweep widening -','select='||$select.postured/@s:sweep,
              'seq='||$s/@s:sweep"
                />
              </xsl:catch>
            </xsl:try>
          </xsl:if>
          <xsl:copy-of select="$select.postured,xsl:sort,$s"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xsl:for-each-group" mode="addPosture">
    <xsl:param name="debug" as="xs:string?" tunnel="yes"/>
    <xsl:copy>
      <xsl:call-template name="init.xsl"/>
      <xsl:variable name="select" select="*[xp:role.select(.)]"/>
      <xsl:variable name="group-adjacent" select="*[xp:role(.,'group-adjacent')]"/>
      <xsl:assert test="count($select) eq 1"/>
      <xsl:variable name="select.postured" as="element()">
        <xsl:apply-templates select="$select" mode="#current"/>
      </xsl:variable>
      <xsl:variable name="group-adjacent.postured" as="element()?">
        <xsl:apply-templates select="$group-adjacent" mode="#current"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$select.postured/@s:posture='grounded'">
          <xsl:call-template name="general-streamability.operands.ids">
            <xsl:with-param name="context-postures" as="map(xs:string,item()*)"
              select="map:new((map:entry(s:sequence-constructor/@id,'grounded'),
              xp:active(xsl:sort)/@id ! map:entry(.,'grounded')))"
            />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="@group-by">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:when>
        <xsl:when test="xp:not-motionless($group-adjacent.postured)">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:when>
        <xsl:when test="xsl:sort">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:when>
        <xsl:when test="empty(@bind-group)">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:when>
        <xsl:when test="exists(@group-adjacent) and empty(@bind-grouping-key)">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="s" as="element()">
            <xsl:apply-templates select="s:sequence-constructor" mode="#current">
              <xsl:with-param name="contextPosture" select="$select.postured/@s:posture"
                tunnel="yes"/>
            </xsl:apply-templates>
          </xsl:variable>
          <xsl:attribute name="s:posture" select="$s/@s:posture"/>
          <xsl:if test="$select.postured/@s:sweep and $s/@s:sweep">            
            <xsl:try>
              <xsl:attribute name="s:sweep" select="$wider($select.postured/@s:sweep)($s/@s:sweep)"/>
              <xsl:catch>
                <xsl:message
                  select="$debug,'Error in for-each-group sweep widening -','select='||$select.postured/@s:sweep,
                  'seq='||$s/@s:sweep"
                />
              </xsl:catch>
            </xsl:try>
          </xsl:if>
          <xsl:copy-of select="$select.postured,xsl:sort,$s"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xsl:apply-imports|xsl:next-match" mode="GS-find-operands">
    <xsl:sequence select="xp:active(.)|xp:active(xsl:with-param)"/>
  </xsl:template>

  <xsl:template match="xsl:apply-templates" mode="GS-find-operands">
    <xsl:sequence
      select="xp:active(.)|xp:active(xsl:with-param)|xp:active(xsl:sort)|xp:AVT(xsl:sort)"/>
  </xsl:template>

  <xsl:template match="xsl:apply-templates" mode="addPosture">
    <xsl:param name="streaming-modes" as="xs:string*" select="()" tunnel="yes"/>
    <xsl:copy>
      <xsl:call-template name="init.xsl"/>
      <xsl:variable name="select" select="*[@s:role='select']"/>
      <xsl:variable name="select.postured" as="element()">
        <xsl:apply-templates select="$select" mode="#current"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$select.postured/@s:posture='grounded'">
          <xsl:call-template name="general-streamability.operands.ids">
            <xsl:with-param name="context-postures" as="map(xs:string,item()*)"
              select="map:new((xp:active(xsl:sort)/@id ! map:entry(.,'grounded')))"/>
          </xsl:call-template>
          <!--<xsl:call-template name="general-streamability.operands">
            <xsl:with-param name="children" as="item()*"
              select="$select.postured,xsl:sort|xsl:param">
              <!-\-<xsl:apply-templates select="*|text()" mode="addPosture"/>-\->
            </xsl:with-param>
          </xsl:call-template>-->
          <!--<xsl:attribute name="s:posture">UNDEFINED</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>-->
        </xsl:when>
        <xsl:when test="xsl:sort">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:when>
        <xsl:when test="not(@mode='#current') and not((@mode,'#default')[1]=$streaming-modes)">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:when>
        <xsl:otherwise>
          <!--<xsl:variable name="s" as="element()">
            <xsl:apply-templates select="s:sequence-constructor" mode="#current">
              <xsl:with-param name="contextPosture" select="$select.postured/@s:posture"
                tunnel="yes"/>
            </xsl:apply-templates>
          </xsl:variable>
          <xsl:attribute name="s:posture" select="$s/@s:posture"/>
          <xsl:if test="$s/@s:sweep">
            <xsl:attribute name="s:sweep" select="$wider($select.postured/@s:sweep)($s/@s:sweep)"/>
          </xsl:if>-->
          <xsl:call-template name="general-streamability.operands">
            <xsl:with-param name="children" as="item()*">
              <xsl:copy-of select="$select.postured,xsl:sort|xsl:param"/>
            </xsl:with-param>
          </xsl:call-template>
          <xsl:copy-of select="xsl:sort|xsl:param"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xsl:iterate" mode="GS-find-operands">
    <xsl:sequence
      select="*[xp:role.select(.)]| xp:active(xsl:param)| s:sequence-constructor| xp:active(xsl:on-completion)"
    />
  </xsl:template>
  <xsl:template match="xsl:break" mode="GS-find-operands">
    <xsl:sequence select="xp:active(.)"/>
  </xsl:template>
  <xsl:template match="xsl:next-iteration" mode="GS-find-operands">
    <xsl:sequence select="xp:active(xsl:with-param)"/>
  </xsl:template>

  <xsl:template match="xsl:iterate" mode="addPosture">
    <xsl:param name="debug" as="xs:string?" tunnel="yes"/>
    <xsl:copy>
      <xsl:call-template name="init.xsl"/>
      <xsl:variable name="select" select="*[@s:role='select']"/>
      <xsl:variable name="select.postured" as="element()">
        <xsl:apply-templates select="$select" mode="#current"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$select.postured/@s:posture='grounded'">
          <xsl:call-template name="general-streamability.operands.ids">
            <xsl:with-param name="context-postures" as="map(xs:string,item()*)"
              select="map:new((map:entry(s:sequence-constructor/@id,$select.postured/@s:posture),
              xp:active(xsl:on-completion)/@id ! map:entry(.,'roaming')))"
            />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="s" as="element()">
            <xsl:apply-templates select="s:sequence-constructor" mode="#current">
              <xsl:with-param name="contextPosture" select="$select.postured/@s:posture"
                tunnel="yes"/>
            </xsl:apply-templates>
          </xsl:variable>
          <xsl:attribute name="s:posture" select="$s/@s:posture"/>
          <xsl:if test="$select.postured/@s:sweep and $s/@s:sweep">
            <xsl:try>
              <xsl:attribute name="s:sweep" select="$wider($select.postured/@s:sweep)($s/@s:sweep)"/>
              <xsl:catch>
                <xsl:message
                  select="$debug,'Error in iterate sweep widening -','select='||$select.postured/@s:sweep,
                  'seq='||$s/@s:sweep"
                />
              </xsl:catch>
            </xsl:try>
          </xsl:if>
          <xsl:copy-of select="$select.postured,xsl:param,$s,xsl:on-completion"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>



  <xsl:template match="xsl:break|
    xsl:next-iteration|xsl:apply-imports|xsl:next-match"
    mode="addPosture">
    <xsl:copy>
      <xsl:call-template name="init.xsl"/>
      <xsl:call-template name="general-streamability.operands.ids"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
