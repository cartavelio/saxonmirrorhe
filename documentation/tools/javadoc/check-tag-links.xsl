<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="urn:local.function"
   exclude-result-prefixes="xs f"
   version="3.0">
   
   <!-- Check tag links in the Javadoc files in javadoc-xml/packages/
      by comparing against javadoc-xml/javadoc-types.xml 
   
      Currently only check fully qualified links (i.e. containing '.') i.e. non-local
      
   -->
   <xsl:param name="base-dir" select="''" as="xs:string"/>
   
   <!-- Choose whether or not to apply post processing filtering of results,
      e.g. to only show internal Saxon links which are not found -->
   <xsl:param name="filter" select="true()"/>
   
   <xsl:mode name="filter" on-no-match="shallow-copy"/>
   
   <xsl:template match="tag[@found eq 'yes']" mode="filter"/>
   
   <xsl:template match="tag[@found eq 'no'][not(contains(@href, 'saxon'))]" mode="filter"/>
   
   
   <xsl:variable name="javadoc-types" select="doc(concat($base-dir, 'javadoc-types.xml'))"/>
   
   <xsl:template match="/" name="main">
      <xsl:variable name="out1">
         <out>
            <xsl:for-each select="collection(concat($base-dir, 'packages/'))//package">
               <package>
                  <xsl:attribute name="id" select="@id"/>
                  <xsl:apply-templates select="*" mode="tag-only"/>
               </package>
            </xsl:for-each>
         </out>
      </xsl:variable>
      
      <xsl:variable name="out2">
         <xsl:choose>
            <xsl:when test="$filter">
               <xsl:apply-templates select="$out1" mode="filter"/>
            </xsl:when>
            <xsl:otherwise>
               <xsl:sequence select="$out1"/>
            </xsl:otherwise>
         </xsl:choose>
      </xsl:variable>
      
      <xsl:sequence select="$out2"/>
      
      <xsl:for-each select="$out2//tag">
         <xsl:message><xsl:value-of select="@to"/> not found: <xsl:value-of select="@href"/></xsl:message>
      </xsl:for-each>
   </xsl:template>
   
   <xsl:template match="*" mode="tag-only">
      <xsl:apply-templates select="node()" mode="tag-only"/>
   </xsl:template>
   
   <xsl:template match="text()" mode="tag-only">
   </xsl:template>
   
   <xsl:template match="tag[@kind eq 'see']" mode="tag-only">
      
      <xsl:variable name="fromPackage" select="ancestor::package/@id" as="xs:string"/>
      <xsl:variable name="fromClass" select="ancestor::class/@id" as="xs:string*"/>
      
      <xsl:if test="contains(., '.')">
         <!-- Process link reference much like viewer_app.xsl f:anchor-navigation
            - remove substring after (
            - if end in ] then remove substring after [
            *** filter out non-local links again
            - split into package path (e.g. net.sf.saxon.lib) and class name (possible nested class) and member name
            - use this to match in javadoc-types.xml
         -->
         
         <xsl:variable name="data-href" select="string(.)"/>
         <xsl:variable name="ref"
            select="for $a in substring-before($data-href,'(') return if ($a eq '') then $data-href else $a"/>
         <xsl:variable name="pageref"
            select="if (ends-with($ref, ']')) then substring-before($ref, '[') else $ref"/>
         
         <xsl:if test="contains($pageref, '.')">
            <xsl:variable name="tokens" select="tokenize($pageref,'\.')" as="xs:string*"/>
            <xsl:variable name="paths" as="xs:string*"
               select="for $t in 1 to count($tokens),
               $ch in substring($tokens[$t],1,1) return
               if (upper-case($ch) eq $ch) then
               concat('/',$tokens[$t]) else concat('.',$tokens[$t])"/>
            
            <xsl:variable name="path" select="substring(string-join($paths,''),2)"/>
            <xsl:variable name="paths2" select="tokenize($path, '/')"/>
            
            
            <!-- If $paths2[last()] contains '#', then split here for member name.
               But note that must check whether path is for nested class (i.e. $paths2 length 3)
               before splitting members. -->
            
            <tag fromPackage="{$fromPackage}" fromClass="{$fromClass}" href="{$paths2}">
               <xsl:choose>
                  <xsl:when test="contains($paths2[last()], '#')">
                     <!-- member link -->
                     <xsl:variable name="package" select="$javadoc-types//j[@in eq 'p'][@id eq $paths2[1]]"/>
                     <xsl:choose>
                        <xsl:when test="count($paths2) eq 3">
                           <xsl:variable name="class" select="$package/j[@in eq 'c'][@id eq $paths2[2]]"/>
                           <xsl:variable name="nested-class" select="$class/j[@in eq 'c'][@id eq
                              substring-before($paths2[3], '#')]"/>
                           <xsl:variable name="member" select="$nested-class/j[@id eq
                              substring-after($paths2[3], '#')]"/>
                           <xsl:attribute name="to">nested-class-member</xsl:attribute>
                           <xsl:attribute name="found" select="if (exists($member)) then 'yes' else
                              'no'"/>
                        </xsl:when>
                        <xsl:otherwise>
                           <xsl:variable name="class" select="$package/j[@in eq 'c'][@id eq
                              substring-before($paths2[2], '#')]"/>
                           <xsl:variable name="member" select="$class/j[@id eq
                              substring-after($paths2[2], '#')]"/>
                           <xsl:attribute name="to">class-member</xsl:attribute>
                           <xsl:attribute name="found" select="if (exists($member)) then 'yes' else
                              'no'"/>
                        </xsl:otherwise>
                     </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>
                     <!-- class link -->
                     <xsl:variable name="package" select="$javadoc-types//j[@in eq 'p'][@id eq $paths2[1]]"/>
                     <xsl:variable name="class" select="$package/j[@in eq 'c'][@id eq $paths2[2]]"/>
                     <xsl:choose>
                        <xsl:when test="count($paths2) eq 3">
                           <xsl:variable name="nested-class" select="$class/j[@in eq 'c'][@id eq $paths2[3]]"/>
                           <xsl:attribute name="to">nested-class</xsl:attribute>
                           <xsl:attribute name="found" select="if (exists($nested-class)) then 'yes'
                              else 'no'"/>
                        </xsl:when>
                        <xsl:otherwise>
                           <xsl:attribute name="to">class</xsl:attribute>
                           <xsl:attribute name="found" select="if (exists($class)) then 'yes'
                              else 'no'"/>
                        </xsl:otherwise>
                     </xsl:choose>
                  </xsl:otherwise>
               </xsl:choose>
            </tag>
         </xsl:if>
         
      </xsl:if>
      
   </xsl:template>
   
   
</xsl:stylesheet>
