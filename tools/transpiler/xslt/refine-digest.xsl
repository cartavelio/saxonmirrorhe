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
      
      @virtual="1" if this is a virtual method: that is, if it is a concrete method
      that has one or more overriding methods in other classes.
      
      @hidden="1" if this is a method on an interface with the same signature as
      a method on a super-interface. Such methods don't appear in the generated
      C# code, as they result in warnings.
      
      @new="1" if this is a new method: specifically, a static method whose signature
      is the same as a static method in a superclass.
      
      @csReturns="type" if this is amethod on a class that implements a method defined 
      in an interface with a different declared return type (a covariant return type). 
      The attribute gives the return type that must be used in the C# method declaration.
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
      <xsl:for-each select="$root//method">
         <xsl:variable name="this-method-name" select="f:full-method-name(.)"/>
         <xsl:variable name="overridden-method" select="f:overridden-method(., ..)"/>
         <xsl:choose>
            <xsl:when test="exists($overridden-method)">
               <xsl:choose>
                  <xsl:when test="$overridden-method!@static='1'">
                     <xsl:sequence select="map{'method':$this-method-name, 'new':true()}"/>
                  </xsl:when>
                  <xsl:when test="$overridden-method!parent::class">
                     <xsl:if test="not($overridden-method!@abstract='1')">
                        <xsl:sequence select="map{'method':f:full-method-name($overridden-method), 'virtual':true()}"/>
                     </xsl:if>
                     <xsl:sequence select="map{'method':$this-method-name, 'override':true()}"/>
                  </xsl:when>
                  <xsl:when test="$overridden-method[@injected]">
                     <xsl:sequence select="map{'method':$this-method-name, 'override':true()}"/>
                  </xsl:when>
               </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
               <xsl:variable name="implemented-methods" select="f:implemented-methods(., ..)"/>
               <xsl:if test="exists($implemented-methods) and parent::interface">
                  <xsl:sequence select="map{'method': $this-method-name, 'hidden':true()}"/>
               </xsl:if>
               <xsl:if test="exists($implemented-methods[@returns != current()/@returns and not(@returns = ../typeParameter/@name)])">
                  <xsl:choose>
                     <xsl:when test="count(distinct-values($implemented-methods!@returns)) eq 1">
                        <xsl:sequence select="map{'method': $this-method-name, 
                                                  'csReturns': $implemented-methods[1]!@returns}"/>
                        
                     </xsl:when>
                  </xsl:choose>
                  <xsl:sequence select="map{'method': $this-method-name, 
                                            'csReturns': $implemented-methods[@returns != current()/@returns][last()]/@returns}"/>
               </xsl:if>
            </xsl:otherwise>
         </xsl:choose>

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
         <xsl:with-param name="new-methods"
            select="map:merge(($method-details[?new]?method)!map{.:true()}, map{'duplicates':'use-last'})"
            tunnel="yes"/>
         <xsl:with-param name="hidden-methods"
            select="map:merge(($method-details[?hidden]?method)!map{.:true()}, map{'duplicates':'use-last'})"
            tunnel="yes"/>
         <xsl:with-param name="covariant-methods"
            select="map:merge(($method-details[?csReturns])!map{?method:?csReturns}, map{'duplicates':'use-last'})"
            tunnel="yes"/>
      </xsl:apply-templates>      
   </xsl:template>
   
   <xsl:template match="method">
      <xsl:param name="virtual-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:param name="override-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:param name="hidden-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:param name="new-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:param name="covariant-methods" tunnel="yes" as="map(*)" select="map{}"/>
      <xsl:variable name="full-name" select="f:full-method-name(.)"/>
      <xsl:copy>
         <xsl:copy-of select="@*"/>
         <xsl:if test="$virtual-methods($full-name)">
            <xsl:attribute name="virtual" select="1"/>
         </xsl:if>
         <xsl:if test="$override-methods($full-name)">
            <xsl:attribute name="override" select="1"/>
         </xsl:if>
         <xsl:if test="$hidden-methods($full-name)">
            <xsl:attribute name="hidden" select="1"/>
         </xsl:if>
         <xsl:if test="$new-methods($full-name)">
            <xsl:attribute name="new" select="1"/>
         </xsl:if>
         <xsl:if test="$covariant-methods($full-name)">
            <xsl:attribute name="csReturns" select="$covariant-methods($full-name)"/>
         </xsl:if> 
         <!--<xsl:variable name="supermethods" select="f:all-supermethods(.)"/>
         <xsl:variable name="covariant-supermethods" select="$supermethods[@returns != current()/@returns]"/>
         <xsl:if test="exists($covariant-supermethods)">
            <xsl:choose>
               <xsl:when test="count(distinct-values($covariant-supermethods/@returns)) = 1">
                  <xsl:attribute name="csReturns" select="$covariant-supermethods[1]/@returns"/>
                  <xsl:attribute name="unique" select="1"/>
               </xsl:when>
               <xsl:otherwise>
                  <!-\- find one that has no supermethods -\->
                  <!-\-<xsl:for-each select="$covariant-supermethods">
                     <xsl:message>Candidate {../@name} returns {@returns} supermethods {f:all-supermethods(.)/@name}</xsl:message>
                  </xsl:for-each>-\->
                  <xsl:variable name="retain" select="$covariant-supermethods[empty(f:all-supermethods(.))]"/>
                  <!-\-<xsl:for-each select="$retain">
                     <xsl:message>Retained Candidate {../@name} returns {@returns} supermethods {f:all-supermethods(.)/@name}</xsl:message>
                  </xsl:for-each>-\->
                  <xsl:attribute name="csReturns" select="$retain[1]/@returns"/>
                  <xsl:attribute name="candidates" select="count($retain)"/>
               </xsl:otherwise>
            </xsl:choose>
         </xsl:if>-->
         <xsl:apply-templates/>
      </xsl:copy>
   </xsl:template>
   
   <!-- For a given method, find any method that it overrides. This may be in an immediate
      superclass, or it may be higher up the hierarchy. Only superclasses are considered, 
      not interfaces. This will therefore return at most one method. -->
   
   <xsl:function name="f:overridden-method" as="element(method)?">
      <xsl:param name="method" as="element(method)"/>
      <xsl:param name="class" as="element(*)"/>
      <xsl:for-each select="$class/extends[string(@name)][1]">
         <!-- loops zero or one times: there is at most one superclass -->
         <xsl:variable name="superclass" select="key('classKey', @name)[self::class]" as="element(*)?"/>
         <xsl:if test="exists($superclass)">
            <xsl:variable name="supermethod"
               select="$superclass/method[@name = $method/@name and string(@sig) = string($method/@sig)]"
               as="element(method)?"/>
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
                  <xsl:variable name="remoteDeclaration" select="f:overridden-method($method, $superclass)" 
                                 as="element(method)?"/>
                  <xsl:choose>
                     <xsl:when test="$remoteDeclaration/parent::class">
                        <xsl:sequence select="$remoteDeclaration"/>
                     </xsl:when>
                     <xsl:when test="exists(f:implemented-methods($method, $superclass))">
                        <method name="{@name}" injected="1" returns="{f:implemented-methods($method, $superclass)[1]/@returns}"/>
                     </xsl:when>
                     <xsl:otherwise>
                        <xsl:sequence select="$remoteDeclaration"/>
                     </xsl:otherwise>
                  </xsl:choose>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:sequence select="f:overridden-method($method, $superclass)"/>
               </xsl:otherwise>
            </xsl:choose>
         </xsl:if>
      </xsl:for-each>
   </xsl:function>
   
   <!-- For a given method, find any methods defined in interfaces that it implements.
        This may return more than one method in the case of multiple inheritance.
        It will only return methods defined in interfaces, not classes -->
   
   <xsl:function name="f:implemented-methods" as="element(method)*">
      <xsl:param name="method" as="element(method)"/>
      <xsl:param name="class" as="element(*)"/>
      <xsl:for-each select="($class/extends | $class/implements)[string(@name)]">
         <xsl:variable name="superclass" select="key('classKey', @name)" as="element(*)?"/>
         <xsl:if test="exists($superclass)">
            <xsl:variable name="supermethod"
               select="$superclass/method[@name = $method/@name and string(@sig) = string($method/@sig)]"/>
            <xsl:choose>
               <xsl:when test="$superclass[self::interface] and exists($supermethod)">
                  <!-- if the method on the interface is itself inherited, then recurse upwards -->
                  <xsl:variable name="ancestorMethods" select="f:implemented-methods($supermethod, $superclass)"/>                 
                  <xsl:sequence select="if (exists($ancestorMethods)) then $ancestorMethods else $supermethod"/>
               </xsl:when>
               <xsl:when test="exists($supermethod)">
                  <xsl:sequence select="f:implemented-methods($supermethod, $superclass)"/>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:sequence select="f:implemented-methods($method, $superclass)"/>
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
