<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns="http://www.w3.org/1999/XSL/Format"
  xmlns:doc="http://www.saxonica.com/ns/documentation" exclude-result-prefixes="xs math"
  version="3.0" expand-text="yes">

  <doc:doc>
    <h1>Generate FO from an HTML body</h1>
    <p>This stylesheet generates an FO version of an HTML document body, using pushmode templates in
      the mode <code>FO</code>.</p>
  </doc:doc>

  <xsl:variable name="config" select="doc('config.xml')/*"/>

  <xsl:variable name="page-sizes" as="element()*">
    <page name="A3" height="420" width="297"/>
    <page name="A4" height="297" width="210"/>
    <page name="A5" height="210" width="149"/>
    <page name="A6" height="149" width="105"/>
  </xsl:variable>

  <xsl:variable name="page" as="element()">
    <page>
      <xsl:sequence
        select="
          ($page-sizes[@name = $config/page/@size],
          $config/page)/@*"/>
    </page>
  </xsl:variable>




  <xsl:mode name="FO" on-no-match="shallow-copy" doc:doc="Convert HTML to FO"/>

  <xsl:variable name="header.size" select="
      ($config/header/@size,
      25)[1]"
    doc:doc="units: mm"/>
  <xsl:variable name="footer.size" select="
      ($config/footer/@size,
      25)[1]"
    doc:doc="units: mm"/>
  <xsl:variable name="page-number.format"
    select="
      ($config/page-number/@format,
      '1')[1]" as="xs:string"
    doc:doc="Typically '1' or 'i'"/>

  <xsl:template match="body" mode="FO">
    <xsl:variable name="header" select="./div[@class = 'header']"/>
    <xsl:variable name="footer" select="./div[@class = 'footer']"/>
    <xsl:variable name="header.fo" as="element()*">
      <xsl:apply-templates select="$header" mode="#current"/>
    </xsl:variable>
    <xsl:variable name="footer.fo" as="element()*">
      <xsl:apply-templates select="$footer" mode="#current"/>
    </xsl:variable>
    <xsl:variable name="header.size"
      select="
        if ($header.fo) then
          $header.size
        else
          0"/>
    <xsl:variable name="footer.size"
      select="
        if ($footer.fo) then
          $footer.size
        else
          0"/>
    <root xmlns="http://www.w3.org/1999/XSL/Format">
      <layout-master-set>
        <simple-page-master master-name="foo" page-height="{$page/@height}mm"
          page-width="{$page/@width}mm"
          margin="{$page/@margin}mm {$page/@margin}mm {$page/@margin}mm {$page/@margin}mm">
          <region-body margin-top="{$header.size}mm" margin-bottom="{$footer.size}mm"/>
          <region-before extent="{$header.size}mm"/>
          <region-after extent="{$footer.size}mm"/>
        </simple-page-master>
      </layout-master-set>
      <page-sequence master-reference="foo" format="{$page-number.format}">
        <xsl:if test="$header.fo">
          <static-content flow-name="xsl-region-before">
            <xsl:sequence select="$header.fo"/>
          </static-content>
        </xsl:if>
        <xsl:if test="$footer.fo">
          <static-content flow-name="xsl-region-after">
            <xsl:sequence select="$footer.fo"/>
          </static-content>
        </xsl:if>
        <flow flow-name="xsl-region-body">
          <xsl:apply-templates select="
              * except ($header,
              $footer)"
            mode="#current"/>
          <block id="LastPage"/>
        </flow>
      </page-sequence>
    </root>
  </xsl:template>

  <xsl:template match="br" mode="FO">
    <block/>
  </xsl:template>
  <xsl:template match="@class | @style | @use-when | @title" mode="FO"/>

  <xsl:template match="div" mode="FO">
    <xsl:apply-templates mode="#current"/>
  </xsl:template>

  <xsl:template match="page-number" mode="FO">
    <page-number/>
  </xsl:template>
  <xsl:template match="total-pages" mode="FO">
    <page-number-citation ref-id="LastPage"/>
  </xsl:template>

  <xsl:template match="span|code" mode="FO">
    <inline>
      <xsl:apply-templates select="@* | node()" mode="#current"/>
    </inline>
  </xsl:template>

  <xsl:template match="title | h1 | h2 | h3 | h4" mode="FO">
    <!-- <block keep-with-next="always">-->
    <block>
      <xsl:apply-templates select="@* | node()" mode="#current"/>
    </block>
  </xsl:template>
  <xsl:template match="p" mode="FO">
    <block>
      <xsl:apply-templates select="@* | node()" mode="#current"/>
    </block>
  </xsl:template>
 
  <xsl:template match="section[@title]" mode="FO">
    <block>{@title}</block>
    <xsl:apply-templates mode="#current"/>
  </xsl:template>

  <xsl:template match="a" mode="FO">
    <basic-link>
      <xsl:apply-templates select="@* | node()" mode="#current"/>
    </basic-link>
  </xsl:template>
  <xsl:template match="a/@href" mode="FO">
    <xsl:attribute name="external-destination" select="."/>
  </xsl:template>

  <xsl:template match="ul | ol" mode="FO">
    <list-block>
      <xsl:apply-templates select="@* | * | text()" mode="#current"/>
    </list-block>
  </xsl:template>
  <xsl:template match="ul/li" mode="FO">
    <list-item>
      <list-item-label start-indent="5mm" end-indent="label-end()">
        <block>&#8226;</block>
      </list-item-label>
      <list-item-body start-indent="body-start()">
        <block>
          <xsl:apply-templates select="@* | node()" mode="#current"/>
        </block>
      </list-item-body>
    </list-item>
  </xsl:template>
  <xsl:template match="ol/li" mode="FO">
    <list-item>
      <list-item-label end-indent="label-end()">
        <block>{position()}.</block>
      </list-item-label>
      <list-item-body start-indent="body-start()">
        <block>
          <xsl:apply-templates select="@* | * | text()" mode="#current"/>
        </block>
      </list-item-body>
    </list-item>
  </xsl:template>

  <xsl:template match="hr" mode="FO">
    <block>
      <leader leader-length="100%" leader-pattern="rule" rule-style="solid" rule-thickness="0.2mm"
        color="black">
        <xsl:apply-templates select="@*" mode="#current"/>
      </leader>
    </block>
  </xsl:template>

  <xsl:template match="i" mode="FO">
    <inline>
      <xsl:apply-templates select="@* | * | text()" mode="#current"/>
    </inline>
  </xsl:template>
  <xsl:template match="img" mode="FO">
    <block>
      <external-graphic>
        <xsl:apply-templates select="@* | * | text()" mode="#current"/>
      </external-graphic>
    </block>
  </xsl:template>

  <xsl:template match="table" mode="FO">
    <table table-layout="fixed">
      <xsl:apply-templates select="@* | * | text()" mode="#current"/>
    </table>
  </xsl:template>
  <xsl:template match="colgroup" mode="FO">
    <xsl:apply-templates select="*" mode="#current"/>
  </xsl:template>
  <xsl:template match="col" mode="FO">
    <xsl:variable name="pos" select="position()"/>
    <xsl:variable name="n.cols" select="count(../col)"/>
    <xsl:variable name="cells" select="../../tbody/tr/td[position() = $pos]"/>
    <xsl:variable name="max.width"
      select="
       max( for $w in $cells/@width
        return
          if (ends-with($w, '%')) then
            number(replace($w, '%', '')) div 100
          else
            number($w))"/>
  <!--  <xsl:if test="$max.width gt 0">
      <xsl:message select="$cells"/>
      <xsl:message>WIDTHS:<xsl:sequence select="$max.width"/></xsl:message>
    </xsl:if>-->
    <table-column column-width="proportional-column-width({if($max.width gt 0) then ($n.cols - 1) * $max.width div (1 - $max.width) else 1})"/>
  </xsl:template>
  <xsl:template match="tbody" mode="FO">
    <table-body>
      <xsl:apply-templates select="@* | node()" mode="#current"/>
    </table-body>
  </xsl:template>
  <xsl:template match="tr" mode="FO">
    <table-row>
      <xsl:apply-templates select="@* | node()" mode="#current"/>
    </table-row>
  </xsl:template>
  <xsl:template match="td" mode="FO">
    <table-cell>
      <xsl:apply-templates select="@* | node()" mode="#current"/>
    </table-cell>
  </xsl:template>
  <xsl:template match="td/@width" mode="FO"></xsl:template>
  <xsl:template match="td/text()" mode="FO">
    <block>
      <xsl:next-match/>
    </block>
  </xsl:template>

</xsl:stylesheet>
