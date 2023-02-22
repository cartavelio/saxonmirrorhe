<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="http://ns.saxonica.com/local-functions"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all"
   version="3.0" expand-text="yes">

   <!-- 
      This stylesheet takes as input the digest file produced by the xml-to-digest.xsl
      stylesheet, and produces an augmented digest file that expands the information
      recorded about each method. Specifically, it adds the following attributes:
      
      @override="1" if this is an overriding method in the sense of the C# specification
      (that is, if it overrides a concrete method in a superclass, as distinct from
      implementing a method defined in an interface or abstract class).
      
      @virtual="1" if the is a virtual method: that is, if it is a concrete method
      that has one or more overriding methods in other classes
      
      @csReturns="type" if the return type of the method from which this method is
      derived is different from the declared return type of the overriding Java method
      (a covariant return type). The attribute gives the return type that must be
      used in the C# method declaration.
   -->


   <xsl:output method="xml"/>
   <xsl:strip-space elements="*"/>
   <xsl:mode on-no-match="shallow-copy"/>

   <xsl:key name="classKey" match="class | interface" use="parent::module/@package || '.' || @name"/>

   <xsl:function name="f:full-method-name" as="xs:string">
      <xsl:param name="method" as="element(method)"/>
      <xsl:text>{$method/../../@package}.{$method/../@name}#{$method/@name}!{$method/@sig}</xsl:text>
   </xsl:function>

   <xsl:function name="f:process-methods" as="map(*)*">
      <xsl:param name="root" as="document-node()"/>
      <xsl:for-each select="$root//method[not(@static='1')]">
         <xsl:variable name="this-method-name" select="f:full-method-name(.)"/>
         <xsl:variable name="overridden-methods" select="f:overridden-methods(., .., false())"/>
         <xsl:for-each select="$overridden-methods">
            <xsl:sequence select="map{'method':f:full-method-name(.), 'virtual':true()}"/>
            <xsl:sequence select="map{'method':$this-method-name, 'override':true()}"/>
         </xsl:for-each>
         <xsl:variable name="implemented-methods" select="f:overridden-methods(., .., true())"/>
         <xsl:for-each select="$implemented-methods[not(@returns = current()/@returns)]">
            <xsl:sequence select="map{'method':$this-method-name, 'csReturns':string(@returns)}"/>
         </xsl:for-each>
      </xsl:for-each>
   </xsl:function>

   <xsl:template match="/">
         <xsl:variable name="method-details" select="f:process-methods(.)"/>
         <!--<xsl:message>csReturns: {serialize($method-details[?csReturns], map{'method':'adaptive'})}</xsl:message>
         -->
         <xsl:apply-templates>
            <xsl:with-param name="virtual-methods"
               select="map:merge(($method-details[?virtual]?method)!map{.:true()}, map{'duplicates':'use-last'})"
               tunnel="yes"/>
            <xsl:with-param name="override-methods"
               select="map:merge(($method-details[?override]?method)!map{.:true()}, map{'duplicates':'use-last'})"
               tunnel="yes"/>
            <xsl:with-param name="csReturns" as="map(*)" tunnel="yes">
               <xsl:map>
                  <xsl:for-each-group select="$method-details[?csReturns]" group-by="?method">
                     <xsl:map-entry key="?method" select="current-group()[last()]?csReturns"/>
                  </xsl:for-each-group>
               </xsl:map>
            </xsl:with-param>
         </xsl:apply-templates>      
   </xsl:template>

   <xsl:template match="method[not(@static='1')]">
      <xsl:param name="virtual-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:param name="override-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:param name="csReturns" tunnel="yes" as="map(*)" select="map{'dummy':()}"/>
      <xsl:variable name="full-name" select="f:full-method-name(.)"/>
      <xsl:copy>
         <xsl:copy-of select="@*"/>
         <xsl:if test="$virtual-methods($full-name)">
            <xsl:attribute name="virtual" select="1"/>
         </xsl:if>
         <xsl:if test="$override-methods($full-name)">
            <xsl:attribute name="override" select="1"/>
         </xsl:if>
         <xsl:if test="map:contains($csReturns, $full-name)">
            <xsl:attribute name="csReturns" select="$csReturns($full-name)"/>
         </xsl:if>
         <xsl:apply-templates/>
      </xsl:copy>
   </xsl:template>

   <xsl:function name="f:overridden-methods" as="element(method)*">
      <xsl:param name="method" as="element(method)"/>
      <xsl:param name="class" as="element(*)"/>
      <xsl:param name="include-abstract" as="xs:boolean"/>
      <xsl:for-each select="($class/extends | $class/implements[$include-abstract])[string(@name)]">
         <xsl:variable name="superclass" select="key('classKey', @name)" as="element(*)?"/>
         <xsl:choose>
            <xsl:when test="empty($superclass)">
               <!--<xsl:if test="matches(@name, '^(com\.saxonica|net\.sf\.saxon)')">
                  <xsl:message>*** Class {@name} not found ***</xsl:message>
               </xsl:if>-->
            </xsl:when>
            <xsl:otherwise>
               <xsl:variable name="supermethod"
                  select="$superclass/method[@name = $method/@name and string(@sig) = string($method/@sig)]"/>
               <xsl:sequence select="$supermethod[$include-abstract or parent::class]"/>
               <!-- TODO: digest needs to record which methods are abstract... -->
               <xsl:sequence select="f:overridden-methods($method, $superclass, $include-abstract)"
               />
            </xsl:otherwise>
         </xsl:choose>
      </xsl:for-each>
   </xsl:function>


</xsl:stylesheet>
