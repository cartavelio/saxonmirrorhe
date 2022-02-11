<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:ixsl="http://saxonica.com/ns/interactiveXSLT"
  xmlns:prop="http://saxonica.com/ns/html-property"
  xmlns:style="http://saxonica.com/ns/html-style-property" xmlns:H="http://www.w3.org/1999/xhtml"
  xpath-default-namespace="http://www.w3.org/1999/xhtml" extension-element-prefixes="ixsl"
  exclude-result-prefixes="xs" version="2.0">


  <xsl:template name="go">
    <xsl:result-document href="#loading" method="ixsl:replace-content"/>
    <xsl:call-template name="showCase">
      <xsl:with-param name="case" select="1"/>
    </xsl:call-template>
    <xsl:apply-templates select="ixsl:page()//div[@class='tabset']" mode="init"/>
  </xsl:template>

  <xsl:template match="span[@class='XPExpr']" mode="ixsl:onclick">
    <xsl:variable name="type" select="@type"/>
    <xsl:variable name="body" select="following-sibling::span[@class=('XMLbody','XSLbody')]"/>
    <xsl:variable name="expr"
      select="if($type) then $body/span[@class='XPtop'][@role=$type]
            else $body/span[@class='XPtop']"/>
    <xsl:variable name="raw" select="(following::span[@class='XPathRAW'])[1]"/>
    <xsl:variable name="show.raw" select="id('show.raw',ixsl:page())/@prop:checked='true'"/>
    <xsl:variable name="showing" select="contains($expr/@style,'inline')"/>
    <!--        <xsl:result-document href="#loading" method="ixsl:replace-content"> Display:<xsl:value-of
                select="$expr/@style"/>
        </xsl:result-document>-->
    <xsl:result-document href="#raw" method="ixsl:replace-content">
      <xsl:sequence select="$raw/(*|text())[not($showing)][$show.raw]"/>
    </xsl:result-document>
    <xsl:for-each select="$expr">
      <ixsl:set-attribute name="style"
        select="if($showing) then 'display:none' else 'display:inline'"/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="showCase">
    <xsl:param name="case" as="xs:double"/>
    <xsl:for-each select="ixsl:page()//div[@class='Case']">
      <ixsl:set-attribute name="style"
        select="if(position() eq $case) then 'display:inline' else 'display:none'"/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="select[@class='genSelector']" mode="ixsl:onchange">
    <xsl:variable name="chosen" select="@prop:value"/>
    <xsl:result-document href="#contain" method="ixsl:replace-content" xpath-default-namespace="">
      <!--<xsl:value-of select="resolve-uri($chosen)"/>-->
      <xsl:copy-of select="doc($chosen)//pre"/>
    </xsl:result-document>
    <xsl:for-each select="ixsl:page()//input[@class='showType']">
      <xsl:if test="@prop:checked='true'">
        <ixsl:set-attribute name="prop:checked" select="false()"/>
      </xsl:if>

    </xsl:for-each>
  </xsl:template>

  <!--<xsl:template match="select[@class='genSelector']" mode="ixsl:onchange" use-when="false()">
    <xsl:variable name="chosen" select="number(@prop:value)"/>
    <xsl:for-each select="following-sibling::div">
      <ixsl:set-attribute name="style"
        select="if(position() eq $chosen) then 'display:inline' else 'display:none'"/>
    </xsl:for-each>
  </xsl:template>-->

  <xsl:template match="input[@id='show.raw']" mode="ixsl:onclick">
    <xsl:variable name="checked" select="@prop:checked='true'"/>
    <xsl:if test="not(@prop:checked='true')">
      <xsl:result-document href="#raw" method="ixsl:replace-content"/>
    </xsl:if>
  </xsl:template>


  <xsl:template match="input[@class='showType']" mode="ixsl:onclick">
    <xsl:variable name="checked" select="@prop:checked='true'"/>
    <xsl:variable name="type" select="@value"/>
    <xsl:for-each select="ixsl:page()//pre//span[@type=$type]">
      <ixsl:set-attribute name="style"
        select="if($checked) then 'display:inline' else 'display:none'"/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="div[@class='tabset']/input" mode="ixsl:onclick">
    <xsl:variable name="name" select="@prop:value"/>
    <xsl:for-each select="../following-sibling::table/tbody/tr/td">
      <ixsl:set-attribute name="style"
        select="if(@name = $name) then 'display:inline' else 'display:none'"/>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="div[@class='tabset']" mode="init">
    <xsl:for-each select="following-sibling::table/tbody/tr/td">
      <ixsl:set-attribute name="style"
        select="if(position() eq 1) then 'display:inline' else 'display:none'"/>
    </xsl:for-each>
  </xsl:template>


  <xsl:template match="span[@class='folder']/span[@class=('collapse','expand')]" mode="ixsl:onclick">
    <xsl:variable name="action" select="@class"/>
    <xsl:for-each select="../span">
      <ixsl:set-attribute name="style"
        select="if($action = @class) then 'display:none' else 'display:inline'"/>
    </xsl:for-each>
    <xsl:for-each select="(following::span[@class=('XSLbody','XMLbody')])[1]">
      <ixsl:set-attribute name="style"
        select="if($action ='collapse') then 'display:none' else 'display:inline'"/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="span[@type=('general','generalOperand')]" mode="ixsl:onclick">
    <xsl:variable name="index" select="tokenize(text()[matches(.,'\S+')],',')"/>
    <xsl:for-each select="id('gsr',ixsl:page())//(li|td)">
      <ixsl:set-attribute name="style"
        select="if(@index = $index) then 'background-color:tomato' else 'background-color:transparent'"
      />
    </xsl:for-each>
    <xsl:result-document href="#loading" method="ixsl:replace-content">
      <xsl:value-of select="$index"/>
    </xsl:result-document>
  </xsl:template>

</xsl:stylesheet>
