<?xml version="1.0" encoding="UTF-8"?>
<xsl:package xmlns:xsl="http://www.w3.org/1999/XSL/Transform" name="function-library"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:array="http://www.w3.org/2005/xpath-functions/array"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math" xmlns:saxon="http://saxon.sf.net/"
   exclude-result-prefixes="xs math FN MAP ARRAY" xmlns:FN="http://ns.saxonica.com/xpath-functions"
   xmlns:ARRAY="http://ns.saxonica.com/xpath-functions/array"
   xmlns:MAP="http://ns.saxonica.com/xpath-functions/map" expand-text="true" version="4.0">

   <xsl:variable name="add-mhk" static="true" select="false()"/>

   <xsl:include href="function-lib-JSON.xsl"/>

   <xsl:expose component="function" names="*" visibility="public"/>

   <xsl:function name="FN:characters" as="xs:string*">
      <xsl:param name="input" as="xs:string?"/>
      <xsl:sequence select="string-to-codepoints($input) ! codepoints-to-string(.)"/>
   </xsl:function>
 
   <xsl:function name="FN:item-at" as="item()?">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="index" as="xs:integer"/>
      <xsl:sequence select="$seq[$index]"/>
   </xsl:function>

   <xsl:function name="FN:foot" as="item()?">
      <xsl:param name="seq" as="item()*"/>
      <xsl:sequence select="$seq[last()]"/>
   </xsl:function>
   
   <xsl:function name="FN:identity" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:sequence select="$seq"/>
   </xsl:function>

   <xsl:function name="FN:replicate" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="count" as="xs:nonNegativeInteger"/>
      <xsl:sequence select="(1 to $count) ! $seq"/>
   </xsl:function>
   
   <xsl:function name="FN:truncate" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:sequence select="$seq[position() ne last()]"/>
   </xsl:function>

   <xsl:function name="FN:uniform" as="xs:boolean">
      <xsl:param name="input" as="item()*"/>
      <xsl:sequence select="FN:uniform($input,default-collation())"/>
   </xsl:function>
   <xsl:function name="FN:uniform" as="xs:boolean">
      <xsl:param name="input" as="item()*"/>
      <xsl:param name="collation" as="xs:string"/>
      <xsl:sequence select="count(distinct-values($input,$collation)) le 1"/>
   </xsl:function>

   <xsl:function name="FN:unique" as="xs:boolean">
      <xsl:param name="input" as="item()*"/>
      <xsl:sequence select="FN:unique($input,default-collation())"/>
   </xsl:function>
   <xsl:function name="FN:unique" as="xs:boolean">
      <xsl:param name="input" as="item()*"/>
      <xsl:param name="collation" as="xs:string"/>
      <xsl:sequence select="count(distinct-values($input,$collation)) eq count($input)"/>
   </xsl:function>

   <xsl:function name="FN:all" as="xs:boolean">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="predicate" as="function(item()) as xs:boolean"/>
      <xsl:sequence select=" every $i in $seq satisfies $predicate($i)"/>
   </xsl:function>

   <xsl:function name="FN:some" as="xs:boolean">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="predicate" as="function(item()) as xs:boolean"/>
      <xsl:sequence select=" some $i in $seq satisfies $predicate($i)"/>
   </xsl:function>

   <xsl:function name="FN:index-where" as="xs:integer*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="predicate" as="function(item()) as xs:boolean"/>
      <xsl:sequence select="for $i in (1 to count($seq)) return $i[$predicate($seq[$i])]"/>
   </xsl:function>

   <xsl:function name="FN:is-NaN" as="xs:boolean">
      <xsl:param name="item" as="xs:anyAtomicType"/>
      <xsl:sequence select="$item ne $item"/>
   </xsl:function>

   <xsl:function name="FN:in-scope-namespaces" as="map(*)">
      <xsl:param name="element" as="element(*)"/>
      <xsl:map>
         <xsl:for-each select="in-scope-prefixes($element)">
            <xsl:map-entry key="." select="namespace-uri-for-prefix(., $element)"/>
         </xsl:for-each>
      </xsl:map>
   </xsl:function>
   
   <xsl:function name="FN:range" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="start-where" as="(function(item()) as xs:boolean)?"/>
      <xsl:sequence select="FN:range($seq, $start-where, (), map{})"/>
   </xsl:function>
   
   <xsl:function name="FN:range" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="start-where" as="(function(item()) as xs:boolean)?"/>
      <xsl:param name="end-where" as="(function(item()) as xs:boolean)?"/>
      <xsl:sequence select="FN:range($seq, $start-where, $end-where, map{})"/>
   </xsl:function>
   
   <xsl:function name="FN:range" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="start-where" as="(function(item()) as xs:boolean)?"/>
      <xsl:param name="end-where" as="(function(item()) as xs:boolean)?"/>
      <xsl:param name="options" as="map(*)"/>
      <xsl:iterate select="$seq">
         <xsl:param name="started" as="xs:boolean" select="empty($start-where)"/>
         <xsl:choose>
            <xsl:when test="$started">
               <xsl:choose>
                  <xsl:when test="empty($end-where)" select="."/>
                  <xsl:when test="$end-where(.)">
                     <xsl:if test="not($options?exclude-end)" then="."/>
                     <xsl:break/>
                  </xsl:when>
                  <xsl:otherwise select="."/>
               </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
               <xsl:choose>
                  <xsl:when test="$start-where(.)">
                     <xsl:choose>
                        <xsl:when test="$options?exclude-start">
                           <xsl:next-iteration>
                              <xsl:with-param name="started" select="true()"/>
                           </xsl:next-iteration>
                        </xsl:when>
                        <xsl:when test="exists($end-where) and $end-where(.)">
                           <xsl:if test="not($options?exclude-end)" then="."/>
                           <xsl:break/>
                        </xsl:when>
                        <xsl:otherwise>
                           <xsl:sequence select="."/>
                           <xsl:next-iteration>
                              <xsl:with-param name="started" select="true()"/>
                           </xsl:next-iteration>
                        </xsl:otherwise>
                     </xsl:choose>                    
                  </xsl:when>
                  <xsl:otherwise/>
               </xsl:choose>
            </xsl:otherwise>
         </xsl:choose>
      </xsl:iterate>
   </xsl:function>

   <xsl:function name="FN:items-after" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="predicate" as="function(item()) as xs:boolean"/>
      <xsl:iterate select="$seq">
         <xsl:param name="include" as="xs:boolean" select="false()"/>
         <xsl:if test="$include">
            <xsl:sequence select="."/>
         </xsl:if>
         <xsl:if test="$include or $predicate(.)">
            <xsl:next-iteration>
               <xsl:with-param name="include" select="true()"/>
            </xsl:next-iteration>
         </xsl:if>
      </xsl:iterate>
   </xsl:function>

   <xsl:function name="FN:items-before" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="predicate" as="function(item()) as xs:boolean"/>
      <xsl:iterate select="$seq">
         <xsl:choose>
            <xsl:when test="$predicate(.)">
               <xsl:break/>
            </xsl:when>
            <xsl:otherwise>
               <xsl:sequence select="."/>
            </xsl:otherwise>
         </xsl:choose>
      </xsl:iterate>
   </xsl:function>

   <xsl:function name="FN:items-from" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="predicate" as="function(item()) as xs:boolean"/>
      <xsl:iterate select="$seq">
         <xsl:param name="include" as="xs:boolean" select="false()"/>
         <xsl:if test="$include or $predicate(.)">
            <xsl:sequence select="."/>
            <xsl:next-iteration>
               <xsl:with-param name="include" select="true()"/>
            </xsl:next-iteration>
         </xsl:if>
      </xsl:iterate>
   </xsl:function>

   <xsl:function name="FN:items-until" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="predicate" as="function(item()) as xs:boolean"/>
      <xsl:iterate select="$seq">
         <xsl:sequence select="."/>
         <xsl:if test="$predicate(.)">
            <xsl:break/>
         </xsl:if>
      </xsl:iterate>
   </xsl:function>
   
   <xsl:function name="FN:slice" as="item()*">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="conditions" as="map(*)"/>
      <xsl:variable name="absp" select="function($x as xs:integer) as xs:integer{if ($x gt 0) then $x else count($seq) + 1 + $x}"/>
      <xsl:variable name="start-positions" as="xs:integer*">
         <xsl:sequence select="1"/>
         <xsl:if test="exists($conditions?start)" then="$absp($conditions?start)"/>
         <xsl:sequence select="min($conditions?at ! $absp(.))"/>
         <xsl:if test="exists($conditions?from)" then="FN:index-where($seq, $conditions?from)[1]"/>
         <xsl:if test="exists($conditions?after)" then="FN:index-where($seq, $conditions?after)[1] + 1"/>
      </xsl:variable>
      <xsl:variable name="start-position" select="max($start-positions)"/>
      <xsl:variable name="end-positions" as="xs:integer*">
         <xsl:if test="exists($conditions?end)" then="$absp($conditions?end)"/>
         <xsl:if test="exists($conditions?length)" then="$start-position + $conditions?length - 1"/>
         <xsl:sequence select="max($conditions?at ! $absp(.))"/>
         <xsl:if test="exists($conditions?until)" then="FN:index-where($seq, $conditions?until)[1]"/>
         <xsl:if test="exists($conditions?before)" then="FN:index-where($seq, $conditions?before)[1] - 1"/>
      </xsl:variable>
      <xsl:variable name="end-position" select="min($end-positions)"/>
      <xsl:message>Range: {$start-position} to {$end-position}</xsl:message>
      <xsl:choose>
         <xsl:when test="exists($conditions?at) and empty($conditions?step)">
            <xsl:variable name="end" select="$end-position otherwise count($seq)"/>
            <xsl:for-each select="($conditions?at ! $absp(.))[. ge $start-position and . le ($end-position otherwise count($seq))]">
               <xsl:sort select="."/>
               <xsl:sequence select="subsequence($seq, ., 1)"/>
            </xsl:for-each>
         </xsl:when>
         <xsl:when test="exists($conditions?step) and empty($conditions?at)"
            select="(0 to (($end-position - $start-position + 1) idiv $conditions?step - 1)) 
            ! (. * $conditions?step) ! subsequence($seq, ., 1)">           
         </xsl:when>
         <xsl:when test="exists($conditions?step) and exists($conditions?at)" select="( (: TODO :) )"/>
         <xsl:when test="exists($end-position)" 
            select="subsequence($seq, $start-position, ($end-position - $start-position + 1))"/>
         <xsl:otherwise select="subsequence($seq, $start-position)"/>
      </xsl:choose>
   </xsl:function>

   <!-- NOTE: we don't have any way of saying that in the absence of $collation, we use the default
      collation from the static context of the caller. Calling default-collation() returns the default
      collation in this module, which isn't the same. -->

   <xsl:function name="FN:highest" as="item()*">
      <xsl:param name="input" as="item()*"/>
      <xsl:sequence select="FN:highest($input,default-collation())"/>
   </xsl:function>
   <xsl:function name="FN:highest" as="item()*">
      <xsl:param name="input" as="item()*"/>
      <xsl:param name="collation" as="xs:string?"/>
      <xsl:sequence select="FN:highest($input,$collation,data#1)"/>
   </xsl:function>
   <xsl:function name="FN:highest" as="item()*">
      <xsl:param name="input" as="item()*"/>
      <xsl:param name="collation" as="xs:string?"/>
      <xsl:param name="key" as="function(item()) as xs:anyAtomicType*"/>
      <xsl:variable name="C" select="($collation,default-collation())[1]"/>
      <xsl:if test="exists($input)">
         <xsl:variable name="keyB" as="item()" select="sort($input,$C,$key)[last()] => $key()"/>
         <xsl:sequence select="$input[deep-equal($key(.), $keyB, $C)]"/>
      </xsl:if>
   </xsl:function>

   <xsl:function name="FN:__simple-compare" as="xs:integer" visibility="private">
      <xsl:param name="k1" as="xs:anyAtomicType"/>
      <xsl:param name="k2" as="xs:anyAtomicType"/>
      <xsl:param name="collation" as="xs:string"/>
      <xsl:choose>
         <xsl:when
            test="$k1 instance of union(xs:string, xs:anyURI, xs:untypedAtomic)
            and $k2 instance of union(xs:string, xs:anyURI, xs:untypedAtomic)"
            select="compare($k1, $k2, $collation)"/>
         <xsl:when test="$k1 eq $k2 or (FN:is-NaN($k1) and FN:is-NaN($k2))" select="0"/>
         <xsl:when test="FN:is-NaN($k1) or $k1 lt $k2" select="-1"/>
         <xsl:otherwise select="+1"/>
      </xsl:choose>
   </xsl:function>

   <xsl:function name="FN:__lexicographic-compare" as="xs:integer" visibility="private">
      <xsl:param name="a" as="xs:anyAtomicType*"/>
      <xsl:param name="b" as="xs:anyAtomicType*"/>
      <xsl:param name="collation" as="xs:string"/>
      <xsl:choose>
         <xsl:when test="empty($a) and empty($b)" select="0"/>
         <xsl:when test="empty($a)" select="-1"/>
         <xsl:when test="empty($b)" select="+1"/>
         <xsl:otherwise>
            <xsl:variable name="rel" select="FN:__simple-compare(head($a), head($b), $collation)"/>
            <xsl:if test="$rel eq 0"
               then="FN:__lexicographic-compare(tail($a), tail($b), $collation)" else="$rel"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>

   <!-- Implementation of fn:highest and fn:lowest assuming all three parameters are explicitly supplied,
        with an extra parameter to distinguish whether we want the highest or lowest -->
   <xsl:function name="FN:mhk_highest" as="item()*" use-when="$add-mhk">
      <xsl:param name="input" as="item()*"/>
      <xsl:param name="collation" as="xs:string?"/>
      <xsl:param name="key" as="function(item()) as xs:anyAtomicType*"/>
      <xsl:param name="is-highest" as="xs:boolean"/>
      <xsl:iterate select="$input">
         <xsl:param name="max" as="xs:anyAtomicType*" select="()"/>
         <xsl:param name="highest-so-far" as="item()*" select="()"/>
         <xsl:on-completion select="$highest-so-far"/>
         <xsl:variable name="key-value"
            select="$key(.) ! (if (. instance of xs:untypedAtomic) then xs:double(.) else .)"/>
         <xsl:choose>
            <xsl:when test="empty($highest-so-far)">
               <xsl:next-iteration>
                  <xsl:with-param name="max" select="$key-value"/>
                  <xsl:with-param name="highest-so-far" select="."/>
               </xsl:next-iteration>
            </xsl:when>
            <xsl:otherwise>
               <xsl:variable name="rel"
                  select="FN:__lexicographic-compare($key-value, $max, $collation)"/>
               <xsl:choose>
                  <xsl:when test="$rel eq 0">
                     <xsl:next-iteration>
                        <xsl:with-param name="max" select="$max"/>
                        <xsl:with-param name="highest-so-far" select="$highest-so-far, ."/>
                     </xsl:next-iteration>
                  </xsl:when>
                  <xsl:when test="($rel gt 0) eq $is-highest">
                     <xsl:next-iteration>
                        <xsl:with-param name="max" select="$key-value"/>
                        <xsl:with-param name="highest-so-far" select="."/>
                     </xsl:next-iteration>
                  </xsl:when>
                  <xsl:otherwise>
                     <xsl:next-iteration/>
                  </xsl:otherwise>
               </xsl:choose>
            </xsl:otherwise>
         </xsl:choose>
      </xsl:iterate>
   </xsl:function>


   <xsl:function name="FN:lowest" as="item()*">
      <xsl:param name="input" as="item()*"/>
      <xsl:sequence select="FN:lowest($input,default-collation())"/>
   </xsl:function>
   <xsl:function name="FN:lowest" as="item()*">
      <xsl:param name="input" as="item()*"/>
      <xsl:param name="collation" as="xs:string?"/>
      <xsl:sequence select="FN:lowest($input,$collation,data#1)"/>
   </xsl:function>
   <xsl:function name="FN:lowest" as="item()*">
      <xsl:param name="input" as="item()*"/>
      <xsl:param name="collation" as="xs:string?"/>
      <xsl:param name="key" as="function(item()) as xs:anyAtomicType*"/>
      <xsl:variable name="C" select="($collation,default-collation())[1]"/>
      <xsl:if test="exists($input)">
         <xsl:variable name="keyB" as="item()" select="sort($input,$C,$key)[1] => $key()"/>
         <xsl:sequence select="$input[deep-equal($key(.), $keyB, $C)]"/>
      </xsl:if>
   </xsl:function>


   <xsl:function name="MAP:filter" as="map(*)">
      <xsl:param name="map" as="map(*)"/>
      <xsl:param name="predicate" as="function(xs:anyAtomicType, item()*) as xs:boolean"/>
      <xsl:map>
         <xsl:for-each select="map:keys($map)">
            <xsl:if test="$predicate(.,$map(.))">
               <xsl:map-entry key="." select="$map(.)"/>
            </xsl:if>
         </xsl:for-each>
      </xsl:map>
   </xsl:function>

   <xsl:function name="MAP:substitute" as="map(*)">
      <xsl:param name="map" as="map(*)"/>
      <xsl:param name="predicate" as="function(xs:anyAtomicType, item()*) as item()*"/>
      <xsl:map>
         <xsl:for-each select="map:keys($map)">
            <xsl:map-entry key="." select="$predicate(.,$map(.))"/>
         </xsl:for-each>
      </xsl:map>
   </xsl:function>

   <xsl:function name="MAP:index" as="map(*)">
      <xsl:param name="seq" as="item()*"/>
      <xsl:param name="key" as="function(item()) as xs:anyAtomicType?"/>
      <xsl:sequence
         select="map:merge(
         for $item in $seq,
         $k in $key($item)
         return map:entry($k, $item),
         map{'duplicates':'combine'})"
      />
   </xsl:function>

   <xsl:function name="ARRAY:partition" as="array(*)*">
      <xsl:param name="input" as="array(*)"/>
      <xsl:param name="size" as="xs:integer"/>
      <xsl:sequence
         select="let $s := array:size($input), $rem := $s mod $size 
         return (for $i in 0 to ($s idiv $size -1)  return array:subarray($input,$i * $size + 1,$size),
         if($rem gt 0) then array:subarray($input,$s - $rem + 1,$rem) else ())"
      />
   </xsl:function>




</xsl:package>
