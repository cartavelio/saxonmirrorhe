<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="http://ns.saxonica.com/local-functions"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all"
   version="3.0" expand-text="yes">
   
   <!-- Old version used prior to 2021-06-23, when we introduced support for C# v9
      covariant return types -->
   
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
   
   <xsl:key name="classKey" match="class | interface" use="ancestor::module/@package || '.' || string-join(ancestor-or-self::*[self::class|self::interface]/@name, '.')"/>
   
   <xsl:function name="f:full-method-name" as="xs:string">
      <xsl:param name="method" as="element(method)"/>
      <xsl:text>{$method/ancestor::module[1]/@package}.{string-join($method/ancestor::*[self::class|self::interface]/@name, '.')}#{$method/@name}!{$method/@sig}</xsl:text>
   </xsl:function>
   
   <xsl:function name="f:process-methods" as="map(*)*">
      <xsl:param name="root" as="document-node()"/>
      <xsl:for-each select="$root//method[not(@static='1')]">
         <xsl:variable name="this-method-name" select="f:full-method-name(.)"/>
         <xsl:variable name="overridden-methods" select="f:overridden-methods(., ..)"/>
         <xsl:for-each select="$overridden-methods">
            <xsl:if test="not(parent::interface)">
               <xsl:if test="not(@abstract='1')">
                  <xsl:sequence select="map{'method':f:full-method-name(.), 'virtual':true()}"/>
               </xsl:if>
               <xsl:sequence select="map{'method':$this-method-name, 'override':true()}"/>
            </xsl:if>
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
      </xsl:apply-templates>      
   </xsl:template>
   
   <xsl:template match="method[not(@static='1')]">
      <xsl:param name="virtual-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:param name="override-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:variable name="full-name" select="f:full-method-name(.)"/>
      <xsl:copy>
         <xsl:copy-of select="@*"/>
         <xsl:if test="$virtual-methods($full-name)">
            <xsl:attribute name="virtual" select="1"/>
         </xsl:if>
         <xsl:if test="$override-methods($full-name)">
            <xsl:attribute name="override" select="1"/>
         </xsl:if>
         <xsl:variable name="supermethods" select="f:all-supermethods(.)"/>
         <xsl:variable name="covariant-supermethods" select="$supermethods[@returns != current()/@returns]"/>
         <xsl:if test="exists($covariant-supermethods)">
            <xsl:choose>
               <xsl:when test="count(distinct-values($covariant-supermethods/@returns)) = 1">
                  <xsl:attribute name="csReturns" select="$covariant-supermethods[1]/@returns"/>
                  <xsl:attribute name="unique" select="1"/>
               </xsl:when>
               <xsl:otherwise>
                  <!-- find one that has no supermethods -->
                  <!--<xsl:for-each select="$covariant-supermethods">
                     <xsl:message>Candidate {../@name} returns {@returns} supermethods {f:all-supermethods(.)/@name}</xsl:message>
                  </xsl:for-each>-->
                  <xsl:variable name="retain" select="$covariant-supermethods[empty(f:all-supermethods(.))]"/>
                  <!--<xsl:for-each select="$retain">
                     <xsl:message>Retained Candidate {../@name} returns {@returns} supermethods {f:all-supermethods(.)/@name}</xsl:message>
                  </xsl:for-each>-->
                  <xsl:attribute name="csReturns" select="$retain[1]/@returns"/>
                  <xsl:attribute name="candidates" select="count($retain)"/>
               </xsl:otherwise>
            </xsl:choose>
         </xsl:if>
         <xsl:apply-templates/>
      </xsl:copy>
   </xsl:template>
   
   <!-- For a given method, find any method that it directly overrides or implements.
        This may return more than one method in the case of multiple inheritance. -->
   
   <xsl:function name="f:overridden-methods" as="element(method)*">
      <xsl:param name="method" as="element(method)"/>
      <xsl:param name="class" as="element(*)"/>
      <xsl:for-each select="($class/extends | $class/implements)[string(@name)]">
         <xsl:variable name="superclass" select="key('classKey', @name)" as="element(*)?"/>
         <xsl:if test="exists($superclass)">
            <xsl:variable name="supermethod"
               select="$superclass/method[@name = $method/@name and string(@sig) = string($method/@sig)]"/>
            <xsl:choose>
               <xsl:when test="exists($supermethod)">
                  <xsl:sequence select="$supermethod"/>
               </xsl:when>
               <xsl:when test="$superclass/@abstract='1'">
                  <!-- If the class inherits from an abstract superclass that does not define the method,
                     but inherits it from an interface, then the abstract class in C# will acquire a 
                     "stub" implementation of the method (that throws NotImplementedException()), and 
                     the method in the subclass will override this stub). To handle this we
                     return a constructed method with @injected='1'-->
                  <xsl:variable name="remoteDeclarations" select="f:overridden-methods($method, $superclass)"/>
                  <xsl:choose>
                     <xsl:when test="exists($remoteDeclarations[parent::class])">
                        <xsl:sequence select="$remoteDeclarations"/>
                     </xsl:when>
                     <xsl:when test="exists($remoteDeclarations[parent::interface])">
                        <method name="{@name}" injected="1" returns="{$remoteDeclarations[1]/@returns}"/>
                     </xsl:when>
                     <xsl:otherwise>
                        <xsl:sequence select="f:overridden-methods($method, $superclass)"/>
                     </xsl:otherwise>
                  </xsl:choose>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:sequence select="f:overridden-methods($method, $superclass)"/>
               </xsl:otherwise>
            </xsl:choose>
         </xsl:if>
      </xsl:for-each>
   </xsl:function>
   
   <!-- For a given method, get the return type of the method from which it ultimately
      derives. We return potentially multiple methods, to handle multiple inheritance,
      and hope for the best if these are different.
   --> 
   
   <xsl:function name="f:all-superclasses" as="element(*(:class|interface:))*">
      <xsl:param name="class"  as="element(*(:class|interface:))"/>
      <xsl:for-each select="($class/extends | $class/implements)[string(@name)]">
         <xsl:variable name="superclass" select="key('classKey', @name)" as="element(*)?"/>   
         <xsl:sequence select="$superclass ! (., f:all-superclasses(.))"/>
      </xsl:for-each>
   </xsl:function>
   
   <xsl:function name="f:all-supermethods" as="element(method)*">
      <xsl:param name="method" as="element(method)"/>
      <xsl:sequence select="f:all-superclasses($method/..)/method[@name = $method/@name and string(@sig) = string($method/@sig)]"/>
   </xsl:function>
   
   
   
</xsl:stylesheet>
