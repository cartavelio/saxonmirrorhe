<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:array="http://www.w3.org/2005/xpath-functions/array"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math" xmlns:saxon="http://saxon.sf.net/"
   exclude-result-prefixes="xs math FN MAP ARRAY" xmlns:FN="http://ns.saxonica.com/xpath-functions"
   xmlns:ARRAY="http://ns.saxonica.com/xpath-functions/array"
   xmlns:MAP="http://ns.saxonica.com/xpath-functions/map" expand-text="true" version="3.0">

   <xsl:function name="FN:json" as="xs:string">
      <xsl:param name="seq" as="item()*"/>
      <xsl:sequence select="xml-to-json(FN:toJson($seq,()),map{'indent': true()})"/>
   </xsl:function>

   <xsl:mode name="toJson" on-no-match="shallow-copy"/>
   <xsl:function name="FN:toJson" as="element()*" xmlns="http://www.w3.org/2005/xpath-functions">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="key" as="attribute()?"/>
      <xsl:choose>
         <xsl:when test="empty($seq)">
            <null>
               <xsl:sequence select="$key"/>
            </null>
         </xsl:when>
         <xsl:when test="count($seq) gt 1">
            <array>
               <xsl:sequence select="$key"/>
               <xsl:sequence select="$seq ! FN:toJson(., ())"/>
            </array>
         </xsl:when>
         <xsl:when test="$seq instance of xs:boolean">
            <boolean>
               <xsl:sequence select="$key, string($seq)"/>
            </boolean>
         </xsl:when>
         <xsl:when
            test="$seq instance of xs:numeric and not(FN:is-NaN($seq)) and ($seq ne number('INF'))">
            <number>
               <xsl:sequence select="$key, string($seq)"/>
            </number>
         </xsl:when>
         <xsl:when test="$seq instance of xs:anyAtomicType">
            <string>
               <xsl:sequence select="$key, string($seq)"/>
            </string>
         </xsl:when>
         <xsl:when test="$seq instance of array(*)">
            <array>
               <xsl:sequence select="$key"/>
               <xsl:sequence select="(1 to array:size($seq)) ! FN:toJson($seq(.), ())"/>
            </array>
         </xsl:when>
         <xsl:when test="$seq instance of map(*)">
            <map>
               <xsl:sequence select="$key"/>
               <xsl:iterate select="map:keys($seq)">
                  <xsl:variable name="key" as="attribute()">
                     <xsl:attribute name="key" select="."/>
                  </xsl:variable>
                  <xsl:sequence select="FN:toJson($seq(.), $key)"/>
               </xsl:iterate>
            </map>
         </xsl:when>
         <xsl:when test="$seq instance of node()">
            <xsl:apply-templates select="$seq" mode="toJson">
               <xsl:with-param name="key" select="$key"/>
            </xsl:apply-templates>
         </xsl:when>
         <!-- We could do apply-templates here, 
            but I think it's neater, more efficient and more coherent to use when cases  -->
         <xsl:otherwise>
            <xsl:apply-templates select="$seq" mode="toJson">
               <xsl:with-param name="key" select="$key"/>
            </xsl:apply-templates>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>

   <xsl:variable name="key.value" as="attribute()">
      <xsl:attribute name="key" select="'#value'"/>
   </xsl:variable>

   <xsl:template match="/" mode="toJson" as="element()"
      xmlns="http://www.w3.org/2005/xpath-functions">
      <xsl:param name="key" as="attribute()?"/>
      <map>
         <xsl:sequence select="$key"/>
         <string key="#name">#document</string>
         <xsl:if test="node()">
            <array key="#content">
               <xsl:apply-templates select="node()" mode="#current"/>
            </array>
         </xsl:if>
      </map>
   </xsl:template>
   <xsl:template match="*" mode="toJson" as="element()"
      xmlns="http://www.w3.org/2005/xpath-functions">
      <xsl:param name="key" as="attribute()?"/>
      <map>
         <xsl:sequence select="$key"/>
         <string key="#name">{local-name(.)}</string>
         <xsl:for-each select="@*">
            <string key="@{name(.)}">{.}</string>
         </xsl:for-each>
         <xsl:choose>
            <xsl:when test="count(node()) eq 1 and exists(text())">
               <xsl:sequence select="FN:toJson(data(text()), $key.value)"/>
            </xsl:when>
            <xsl:when test="node()">
               <array key="#content">
                  <xsl:apply-templates select="node()" mode="#current"/>
               </array>
            </xsl:when>
         </xsl:choose>
      </map>
   </xsl:template>
   <xsl:template match="attribute()" mode="toJson" as="element()"
      xmlns="http://www.w3.org/2005/xpath-functions">
      <xsl:param name="key" as="attribute()?"/>
      <map>
         <xsl:sequence select="$key"/>
         <string key="#attribute">{local-name(.)}</string>
      </map>
   </xsl:template>
   <xsl:template match="text()" mode="toJson" as="element()"
      xmlns="http://www.w3.org/2005/xpath-functions">
      <xsl:param name="key" as="attribute()?"/>
      <string>
         <xsl:sequence select="$key, ."/>
      </string>
   </xsl:template>
   <xsl:template match="comment()" mode="toJson" as="element()"
      xmlns="http://www.w3.org/2005/xpath-functions">
      <xsl:param name="key" as="attribute()?"/>
      <map>
         <xsl:sequence select="$key"/>
         <string key="#comment">{.}</string>
      </map>
   </xsl:template>
   <xsl:template match="processing-instruction()" mode="toJson" as="element()"
      xmlns="http://www.w3.org/2005/xpath-functions">
      <xsl:param name="key" as="attribute()?"/>
      <map>
         <xsl:sequence select="$key"/>
         <string key="#pi">{name(.)}</string>
         <string key="#data">{.}</string>
      </map>
   </xsl:template>

   <xsl:function name="FN:fromJSON" as="item()*">
      <xsl:param name="input" as="xs:string"/>
      <xsl:apply-templates select="json-to-xml($input)" mode="fromJSON"/>
   </xsl:function>

   <xsl:mode name="fromJSON" on-no-match="shallow-copy"/>
   <xsl:template match="map[string[@key eq '#name']]"
      xpath-default-namespace="http://www.w3.org/2005/xpath-functions" mode="fromJSON" as="item()*">
      <xsl:variable name="name" select="string[@key eq '#name']"/>
      <xsl:variable name="content" select="*[@key = ('#content', '#value')]"/>
      <xsl:choose>
         <xsl:when test="$name eq '#document'">
            <xsl:document>
               <xsl:apply-templates select="$content" mode="#current"/>
            </xsl:document>
         </xsl:when>
         <xsl:otherwise>
            <xsl:element name="{$name}">
               <xsl:for-each select="*[starts-with(@key, '@')]">
                  <xsl:attribute name="{substring-after(@key,'@')}">
                     <xsl:apply-templates select="." mode="#current"/>
                  </xsl:attribute>
               </xsl:for-each>
               <xsl:apply-templates select="$content" mode="#current"/>
            </xsl:element>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   <xsl:template match="array[@key eq '#content']"
      xpath-default-namespace="http://www.w3.org/2005/xpath-functions" mode="fromJSON" as="item()*">
      <xsl:apply-templates select="*" mode="#current"/>
   </xsl:template>
   <xsl:template
      match="string[@key eq '#value' or starts-with(@key, '@')] | number[@key eq '#value' or starts-with(@key, '@')]"
      xpath-default-namespace="http://www.w3.org/2005/xpath-functions" mode="fromJSON" as="item()*">
      <xsl:sequence select="data(.)"/>
   </xsl:template>
   <xsl:template match="map[string[@key eq '#comment']]"
      xpath-default-namespace="http://www.w3.org/2005/xpath-functions" mode="fromJSON" as="item()*">
      <xsl:comment select="*"/>
   </xsl:template>
   <xsl:template match="number" xpath-default-namespace="http://www.w3.org/2005/xpath-functions"
      mode="fromJSON" as="item()*">
      <xsl:sequence select="number(.)"/>
   </xsl:template>
   <xsl:template match="string" xpath-default-namespace="http://www.w3.org/2005/xpath-functions"
      mode="fromJSON" as="item()*">
      <xsl:sequence select="string(.)"/>
   </xsl:template>
   <xsl:template match="boolean" xpath-default-namespace="http://www.w3.org/2005/xpath-functions"
      mode="fromJSON" as="item()*">
      <xsl:sequence select=". eq 'true'"/>
   </xsl:template>
   <xsl:template match="Zmap" xpath-default-namespace="http://www.w3.org/2005/xpath-functions"
      mode="fromJSON" as="item()*">
      <xsl:map>
         <xsl:for-each select="*">
            <xsl:map-entry key="@key">
               <xsl:apply-templates select="." mode="#current"/>
            </xsl:map-entry>
         </xsl:for-each>
      </xsl:map>
   </xsl:template>

   <xsl:template match="Zarray" xpath-default-namespace="http://www.w3.org/2005/xpath-functions"
      mode="fromJSON" as="item()*">
      <xsl:iterate select="*">
         <xsl:param name="result" select="array{}"/>
         <xsl:on-completion select="$result"/>
         <xsl:variable name="processed" as="item()*">
            <xsl:apply-templates select="." mode="#current"/>
         </xsl:variable>
         <xsl:next-iteration>
            <xsl:with-param name="result" select="array:append($result, $processed)"/>
         </xsl:next-iteration>
      </xsl:iterate>
   </xsl:template>


</xsl:stylesheet>
