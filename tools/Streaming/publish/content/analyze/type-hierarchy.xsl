<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  exclude-result-prefixes="xs math" version="3.0">

  <xsl:variable name="type.hierarchy">
    <item>
      <node>
        <document-node/>
        <element/>
        <attribute childless="true"/>
        <text childless="true"/>
        <comment childless="true"/>
        <processing-instruction childless="true"/>
        <namespace childless="true"/>
      </node>
      <xs:anyAtomicType>
        <xs:boolean/>
        <xs:string/>
        <xs:anyURI/>
        <xs:QName/>
        <xs:hexBinary/>
        <xs:base64Binary/>
        <xs:date/>
        <xs:dateTime/>
        <xs:time/>
        <xs:gYear/>
        <xs:gYearMonth/>
        <xs:gMonth/>
        <xs:gMonthDay/>
        <xs:gDay/>
        <xs:duration>
          <xs:dayTimeDuration/>
          <xs:yearMonthDuration/>
        </xs:duration>
        <xs:double/>
        <xs:float/>
        <xs:decimal>
          <xs:integer/>
        </xs:decimal>
      </xs:anyAtomicType>
      <!-- Function and map types -->
    </item>
  </xsl:variable>

  <xsl:variable name="atomicTypes"
    select="$type.hierarchy//*[ancestor-or-self::xs:anyAtomicType]/name()"/>

  <xsl:variable name="least-common-supertypes" as="map(xs:string,map(*))"
    select="map:new($type.hierarchy//* !
            (let $n := . return map:entry(name($n), map:new(
             $type.hierarchy//* ! (let $m := . return map:entry(name($m),if($m is $n) then name($m) else 
             ($n/ancestor-or-self::* intersect $m/ancestor-or-self::*)[last()]/name()))
                                        )))
                                        )"/>

  <xsl:function name="xp:base-type" as="xs:string">
    <xsl:param name="type" as="xs:string"/>
    <xsl:value-of select="replace($type,'\*$|\?$','') ! replace(.,'\(.*\)$','')"/>
  </xsl:function>
  <xsl:function name="xp:least-common-supertype" as="xs:string">
    <xsl:param name="type1" as="xs:string"/>
    <xsl:param name="type2" as="xs:string"/>
    <xsl:try>
      <xsl:value-of
        select="if($type1 eq $type2) then $type1 else
      if(starts-with($type1,'UNKNOWN')) then $type1 else
      if(starts-with($type2,'UNKNOWN')) then $type2 else
            (let $t1 := xp:base-type($type1), $t2 := xp:base-type($type2) return
            $least-common-supertypes($t1)($t2))"/>
      <!-- Something here to handle type, type*, type? etc.  -->
      <xsl:catch>UNKNOWN error in least-common-supertype</xsl:catch>
    </xsl:try>
  </xsl:function>
  <xsl:function name="xp:composite-type" as="xs:string">
    <xsl:param name="types" as="xs:string*"/>
    <xsl:variable name="temp"
      select="fold-left($types,(),
            function($t0,$t1){if(empty($t0)) then string($t1) else xp:least-common-supertype($t0,$t1)})"/>
    <xsl:value-of
      select="if(starts-with($temp,'xs:') or matches($temp,'\)[*+?]?$')) then $temp else $temp||'()'"
    />
  </xsl:function>
  <xsl:function name="xp:composite-type-multiple" as="xs:string">
    <xsl:param name="types" as="xs:string*"/>
    <xsl:value-of
      select="let $t := xp:composite-type($types) return
            if(count($types) gt 1 and not(matches($t,'\*$|\?$'))) then $t||'*' else $t"
    />
  </xsl:function>
  <xsl:function name="xp:composite-type-cardinal" as="xs:string">
    <xsl:param name="types" as="xs:string*"/>
    <xsl:value-of
      select="let $t := xp:composite-type($types) return
            if(count($types) gt 1 and not(matches($t,'\*$|\?$'))) then $t||'*' else $t"
    />
  </xsl:function>
  <xsl:function name="xp:composite-type-possible" as="xs:string">
    <xsl:param name="types" as="xs:string*"/>
    <xsl:value-of
      select="let $t := xp:composite-type($types) return
            if(not(matches($t,'\*$|\?$'))) then $t||(if(count($types) gt 1) then '*' else '?') else $t"
    />
  </xsl:function>

  <xsl:variable name="childless-node-types"
    select="$type.hierarchy//node/*[@childless]/name(),$type.hierarchy//xs:*/name(.)"/>

  <xsl:function name="xp:is-childless-type" as="xs:boolean">
    <xsl:param name="type" as="xs:string?"/>
    <xsl:try>
      <xsl:sequence select="replace($type,'\(.*\)|\*$|\?$','') = $childless-node-types"/>
      <xsl:catch>
        <xsl:sequence select="false()"/>
      </xsl:catch>
    </xsl:try>
  </xsl:function>
</xsl:stylesheet>
