<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace" exclude-result-prefixes="xs math"
  version="3.0">
  <xsl:variable name="functions" as="map(xs:string,item()*)" use-when="true()">
    <xsl:map><xsl:sequence>
      <xsl:for-each select="doc('function-catalog.xml')//fos:function[not(@prefix='op')],
        doc('stream-function-catalog.xml')//fos:function[not(@prefix='op')]">
        <xsl:map-entry key="(if(@prefix = ('math')) then @prefix||':' else '') || @name">
          <xsl:variable name="proto" select="fos:signatures/fos:proto"/>
          <xsl:map>
            <xsl:sequence>
            <!--                               <xsl:map-entry key="'returnType'"
                                    select="let $t := distinct-values($proto/@return-type) return
                                    if(count($t gt 1)) then error(QName('','mutipleReturnTypes'), 'Multiple return types for '||@name) else $t"
                                />-->
            <xsl:map-entry key="'returnType'" select="distinct-values($proto/@return-type)[last()]"/>
            <xsl:map-entry key="'args'">
              <xsl:map><xsl:sequence>
                <xsl:for-each select="$proto[last()]/fos:arg">
                  <xsl:map-entry key="position()">
                    <xsl:copy-of select="."/>
                  </xsl:map-entry>
                </xsl:for-each></xsl:sequence>
              </xsl:map>
            </xsl:map-entry></xsl:sequence>
          </xsl:map>
        </xsl:map-entry>
      </xsl:for-each></xsl:sequence>
    </xsl:map>
  </xsl:variable>
  
  <xsl:variable name="functions" as="map(xs:string,item()*)" use-when="false()"
    select="map:new(
    for $f in (doc('function-catalog.xml')//fos:function[not(@prefix='op')],
               doc('stream-function-catalog.xml')//fos:function[not(@prefix='op')]) 
    return map:entry($f/(if(@prefix = ('math')) then @prefix||':' else '') || @name,
                     let $proto := $f/fos:signatures/fos:proto
                     return map:new((
                                     map:entry('returnType',distinct-values($proto/@return-type)[last()]),
                                     map:entry('args',$proto[last()]/fos:arg/map:entry(position(),.))
                                   ))
                     )
                    )"/>
   
  

  <xsl:variable name="default-dot-functions" as="xs:string*"
    select="map:keys($functions)! .[$functions(.)('args')(1)/@default='.']"/>

  <xsl:variable name="instructions" as="map(xs:string,item()*)" use-when="true()">
    <xsl:map>
      <xsl:for-each
        select="doc('element-catalog.xml')//e:element-syntax[(e:in-category/@name='instruction') or @name=('when','otherwise','with-param','sort')]">
        <xsl:map-entry key="concat('xsl:',@name)">
          <xsl:copy-of select="* except e:in-category"/>
        </xsl:map-entry>
      </xsl:for-each>
    </xsl:map>
  </xsl:variable>
  <xsl:variable name="instructions" as="map(xs:string,item()*)" use-when="false()" select="
    map:new(
    for $i in doc('element-catalog.xml')//e:element-syntax[(e:in-category/@name='instruction') 
                                                            or @name=('when','otherwise','with-param','sort')]
    return map:entry(concat('xsl:',$i/@name),* except e:in-category)
           )
    "/>
  <xsl:variable name="declarations" as="map(xs:string,item()*)" use-when="false()" select="
    map:new(
    for $i in doc('element-catalog.xml')//e:element-syntax[(e:in-category/@name='declaration')]
    return map:entry(concat('xsl:',$i/@name),* except e:in-category)
    )
    "/>  
    
  <xsl:variable name="declarations" as="map(xs:string,item()*)" use-when="true()">
    <xsl:map>
      <xsl:for-each
        select="doc('element-catalog.xml')//e:element-syntax[e:in-category/@name='declaration']">
        <xsl:map-entry key="concat('xsl:',@name)">
          <xsl:copy-of select="* except e:in-category"/>
        </xsl:map-entry>
      </xsl:for-each>
    </xsl:map>
  </xsl:variable>
</xsl:stylesheet>
