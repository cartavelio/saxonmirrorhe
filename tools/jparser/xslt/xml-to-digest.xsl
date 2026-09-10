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
      (one XML file for each Java source file in the product), and generates a digest file,
      which contains information needed when converting each individual Java file to C#. The
      digest file includes information about classes and interfaces and their relationships.
   -->
   
   <xsl:output method="xml"/>
   <xsl:strip-space elements="*"/>
   <xsl:variable name="NL" select="'&#xa;'" as="xs:string"/>
   
 
   
   <xsl:mode on-no-match="shallow-skip"/>

   
   <xsl:function name="f:qualified-name" as="xs:string">
      <xsl:param name="in" as="element(*)"/>
      <xsl:variable name="term" as="xs:string" select="$in/@identifier"/>
      <xsl:sequence select="(if ($in/qualifier) then f:qualified-name($in/qualifier) || '.' else '') || $term"/>
   </xsl:function>
   
   <xsl:function name="f:degenerify" as="xs:string?">
      <xsl:param name="typeName" as="xs:string?"/>
      <xsl:sequence select="replace($typeName, '&lt;.*$', '')"/>
   </xsl:function>

   
   <xsl:param name="xmlDir" as="xs:string" required="yes"/>
   
   <xsl:template name="xsl:initial-template">
      <digest>
         <xsl:apply-templates select="collection($xmlDir || '?recurse=yes;select=*.xml')"/>
      </digest>     
   </xsl:template>
   
   <xsl:template match="root">
      <module package="{f:qualified-name(packageDeclaration/name)}">
         <xsl:apply-templates select="types/type"/>
      </module>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='true']">
      <interface name="{f:degenerify(name/@identifier)}">
         <xsl:apply-templates select="extendedTypes/ExtendedType"/>
         <xsl:apply-templates select="members/member">
            <xsl:with-param name="isInterface" select="true()" tunnel="yes"/>
         </xsl:apply-templates>
      </interface>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='false']">
      <class name="{f:degenerify(name/@identifier)}">
         <xsl:apply-templates select="extendedTypes/extendedType"/>
         <xsl:apply-templates select="implementedTypes/implementedType"/>
         <xsl:apply-templates select="members/member">
            <xsl:with-param name="isInterface" select="false()" tunnel="yes"/>
         </xsl:apply-templates>
      </class>
   </xsl:template>
   
   <xsl:template match="extendedType">
      <extends name="{f:degenerify(@RESOLVED_TYPE)}"/>
   </xsl:template>
   
   <xsl:template match="implementedType">
      <implements name="{f:degenerify(@RESOLVED_TYPE)}"/>
   </xsl:template>
   
   <xsl:template match="member[@nodeType='MethodDeclaration']">
      <xsl:param name="isInterface" as="xs:boolean" tunnel="yes" select="false()" required="no"/>
      <method name="{name/@identifier}" 
              static="{count(modifiers/modifier[@keyword='STATIC'])}" 
              arity="{count(parameters/parameter)}" 
              returns="{type/@RESOLVED_TYPE}">
          <xsl:if test="$isInterface and modifiers/modifier/@keyword='DEFAULT'">
             <default>
                <xsl:copy-of select="."/>
             </default>
          </xsl:if>  
      </method>
   </xsl:template>
 
</xsl:stylesheet>