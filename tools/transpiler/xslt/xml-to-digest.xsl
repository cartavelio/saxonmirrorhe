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
   
 
   
   <xsl:mode on-no-match="fail"/>

   
   <xsl:function name="f:qualifiedName" as="xs:string">
      <xsl:param name="in" as="element(*)"/>
      <xsl:variable name="term" as="xs:string" select="$in/@identifier"/>
      <xsl:sequence select="(if ($in/qualifier) then f:qualifiedName($in/qualifier) || '.' else '') || $term"/>
   </xsl:function>
   
   <xsl:function name="f:degenerify" as="xs:string?">
      <xsl:param name="typeName" as="xs:string?"/>
      <xsl:sequence select="replace($typeName, '&lt;.*$', '')"/>
   </xsl:function>
   
   <xsl:function name="f:typeParams" as="xs:string?">
      <xsl:param name="typeName" as="xs:string?"/>
      <xsl:sequence select="replace($typeName, '^[^&lt;]+', '')"/>
   </xsl:function>
  
   <xsl:param name="xmlDir" as="xs:string" required="yes"/>
   
   <xsl:template name="xsl:initial-template">
      <digest>
         <xsl:apply-templates select="collection($xmlDir || '?recurse=yes;select=*.xml')"/>
      </digest>     
   </xsl:template>
   
   <xsl:template match="root">
      <module package="{f:qualifiedName(packageDeclaration/name)}">
         <xsl:apply-templates select="types/type"/>
      </module>
   </xsl:template>
   
   <xsl:function name="f:isDelegate" as="xs:boolean">
      <xsl:param name="interfaceDecl" as="element()"/>
      <xsl:choose>
         <xsl:when test="$interfaceDecl/annotations/annotation/name/@identifier='CSharpDelegate'">
            <xsl:sequence select="$interfaceDecl/annotations/annotation[name/@identifier='CSharpDelegate']/memberValue/@value = 'true'"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="exists($interfaceDecl[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='true']
               [annotations/annotation/name/@identifier='FunctionalInterface'][count(members/member)=1])"/>
         </xsl:otherwise>
      </xsl:choose>      
   </xsl:function>
   
   <xsl:template match="*[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='true']">
      <interface name="{f:degenerify(name/@identifier)}">
         <xsl:if test="f:isDelegate(.)">
            <xsl:attribute name="delegate" select="1"/>
         </xsl:if>
         <xsl:apply-templates select="typeParameters/typeParameter" />
         <xsl:apply-templates select="extendedTypes/extendedType"/>
         <xsl:for-each select="annotations/annotation[name/@identifier='CSharpExtendInterface']/pairs/pair/value">
            <extends name="{@value}"/>
         </xsl:for-each>
         <xsl:apply-templates select="members/member[@nodeType='MethodDeclaration']">
            <xsl:with-param name="isInterface" select="true()" tunnel="yes"/>
         </xsl:apply-templates>
      </interface>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='false']">
      <class name="{f:degenerify(name/@identifier)}">
         <xsl:if test="modifiers/modifier[@keyword='ABSTRACT']">
            <xsl:attribute name="abstract" select="1"/>
         </xsl:if>
         <xsl:apply-templates select="extendedTypes/extendedType"/>
         <xsl:apply-templates select="implementedTypes/implementedType"/>
         <xsl:apply-templates select="typeParameters/typeParameter" />
         <xsl:apply-templates select="members/member[@nodeType='ConstructorDeclaration']"/>
         <xsl:apply-templates select="members/member[@nodeType='FieldDeclaration']"/>
         <xsl:apply-templates select="members/member[@nodeType='MethodDeclaration']">
            <xsl:with-param name="isInterface" select="false()" tunnel="yes"/>
         </xsl:apply-templates>
         <xsl:apply-templates select="members/member[@nodeType='ClassOrInterfaceDeclaration']">
            <xsl:with-param name="isInterface" select="@isInterface='true'" tunnel="yes"/>
         </xsl:apply-templates>
         <xsl:apply-templates select="members/member[@nodeType='EnumDeclaration']">
            <xsl:with-param name="isInterface" select="false()" tunnel="yes"/>
         </xsl:apply-templates>
      </class>
   </xsl:template>
   
   <xsl:template match="typeParameter">
      <xsl:message>found code <xsl:copy-of select="."/></xsl:message>
      <xsl:for-each select=".//name">
         <xsl:message>cp1 found code <typeParameter name="{@identifier}" /></xsl:message>
         <typeParameter name="{@identifier}" />                    
      </xsl:for-each>
   </xsl:template>
   
   <xsl:template match="*[@nodeType='EnumDeclaration']">
      <enum name="{f:degenerify(name/@identifier)}">
         <xsl:if test="annotations/annotation/name/@identifier='CSharpSimpleEnum'">
            <xsl:attribute name="simple" select="1"/>
         </xsl:if>
      </enum>
   </xsl:template>
   
   <xsl:template match="extendedType">
      <extends name="{f:degenerify(@RESOLVED_TYPE)}">
         <xsl:variable name="typeParams" select="f:typeParams(@RESOLVED_TYPE)"/>
         <xsl:if test="$typeParams">
            <xsl:attribute name="params" select="$typeParams"/>
         </xsl:if>
      </extends>
   </xsl:template>
   
   <xsl:template match="implementedType">
      <implements name="{f:degenerify(@RESOLVED_TYPE)}">
         <xsl:variable name="typeParams" select="f:typeParams(@RESOLVED_TYPE)"/>
         <xsl:if test="$typeParams">
            <xsl:attribute name="params" select="$typeParams"/>
         </xsl:if>
      </implements>
   </xsl:template>
   
   <xsl:template match="member[@nodeType='ConstructorDeclaration']">
      <constructor>
         <xsl:attribute name="params" select="string-join(parameters/parameter/type/@RESOLVED_TYPE, '|')"/>
      </constructor>
   </xsl:template>
   
   <xsl:template match="member[@nodeType='MethodDeclaration']">
      <xsl:param name="isInterface" as="xs:boolean" tunnel="yes" select="false()" required="no"/>
      <method name="{name/@identifier}"              
              returns="{replace(type/@RESOLVED_TYPE, '\? extends ', '')}">
         <xsl:if test="parameters/parameter or $isInterface">
            <xsl:attribute name="sig" select="string-join(parameters/parameter/
                            ((type/@RESOLVED_TYPE) || (if (@isVarArgs='true') then '[]' else '')), ',')"/>
            <xsl:attribute name="params" select="string-join(parameters/parameter/type/@RESOLVED_TYPE, '|')"/>
         </xsl:if>
         <xsl:if test="modifiers/modifier[@keyword='STATIC']">
            <xsl:attribute name="static" select="1"/>
         </xsl:if>
         <xsl:if test="modifiers/modifier[@keyword='ABSTRACT']">
            <xsl:attribute name="abstract" select="1"/>
         </xsl:if>
          <xsl:if test="$isInterface and modifiers/modifier/@keyword='DEFAULT'">
             <default>
                <xsl:copy-of select="."/>
             </default>
          </xsl:if>  
      </method>
   </xsl:template>
   
   <xsl:template match="member[@nodeType='FieldDeclaration']">
      <xsl:variable name="staticModifier" select="modifiers/modifier[@keyword='STATIC']"/>
      <xsl:for-each select="variables/variable">
         <field name="{name/@identifier}" type="{type/@RESOLVED_TYPE}">
            <xsl:if test="$staticModifier">
               <xsl:attribute name="static" select="1"/>
            </xsl:if>
         </field>
      </xsl:for-each>
   </xsl:template>
 
</xsl:stylesheet>