<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   version="3.0"
   expand-text="yes"
   default-mode="xml-to-cs">
   
   <!-- 
      This stylesheet takes the collection of XML files produced as the output of the javaParser
      (one XML file for each Java source file in the product), and generates a report,
      containing a list of all the imported packages other than those within Saxon itself.
   -->
   
   <xsl:output method="xml" indent="yes"/>
   <xsl:strip-space elements="*"/>
   <xsl:variable name="NL" select="'&#xa;'" as="xs:string"/>
   
 
   
   <xsl:mode on-no-match="shallow-skip"/>

   
   <xsl:function name="f:qualifiedName" as="xs:string">
      <xsl:param name="in" as="element(*)"/>
      <xsl:variable name="term" as="xs:string" select="$in/@identifier"/>
      <xsl:sequence select="(if ($in/qualifier) then f:qualifiedName($in/qualifier) || '.' else '') || $term"/>
   </xsl:function>
   
   <xsl:function name="f:degenerify" as="xs:string?">
      <xsl:param name="typeName" as="xs:string?"/>
      <xsl:sequence select="replace($typeName, '&lt;.*$', '')"/>
   </xsl:function>

   
   <xsl:param name="xmlDir" as="xs:string" required="yes"/>
   
   <xsl:template name="xsl:initial-template">
      <report>
         <xsl:variable name="types" select="collection($xmlDir || '?recurse=yes;select=*.xml')//@RESOLVED_TYPE!f:degenerify(.)[not(starts-with(., 'net.sf.saxon'))]"/>
         <xsl:for-each-group select="$types" group-by=".">
            <xsl:sort select="current-grouping-key()"/>
            <uses>{current-grouping-key()}</uses>
         </xsl:for-each-group>
      </report>     
   </xsl:template>
   
 
 
</xsl:stylesheet>