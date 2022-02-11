<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fc="http://www.saxonica.com/ns/doc/functions"
    exclude-result-prefixes="xs fc" version="2.0">
    
    <xsl:variable name="implementation-doc" select="doc($implement-path)"
        as="document-node()"/>

    <!-- Produce sorted list of functions. -->
    <xsl:function name="fc:fn-list" as="element()*">
        <xsl:param name="fns" as="element()*"/>
        <xsl:perform-sort select="$fns">
            <xsl:sort select="lower-case(fc:name)" lang="en"/>
        </xsl:perform-sort>
    </xsl:function>

    <!-- Get function namespace 'usual' (conventional) prefix (used for definition page titles and function references). -->
    <xsl:function name="fc:usual-prefix" as="xs:string">
        <xsl:param name="ns" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions'">fn</xsl:when>
            <xsl:when test="$ns = 'http://saxon.sf.net/'">saxon</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/map'">map</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/array'">array</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/math'">math</xsl:when>
            <xsl:when test="$ns = 'http://expath.org/ns/archive'">arch</xsl:when>
            <xsl:when test="$ns = 'http://expath.org/ns/binary'">bin</xsl:when>
            <xsl:when test="$ns = 'http://expath.org/ns/file'">file</xsl:when>
            <xsl:when test="$ns = 'http://expath.org/ns/zip'">zip</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/common'">exsl</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/random'">random</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/math'">exslt-math</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/sets'">set</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/dates-and-times'">date</xsl:when>
            <xsl:when test="$ns = 'http://saxon.sf.net/sql'">sql</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$ns"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <!-- Get function namespace reference (short form name used in URL page names). -->
    <xsl:function name="fc:namespace-ref" as="xs:string">
        <xsl:param name="ns" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions'">fn</xsl:when>
            <xsl:when test="$ns = 'http://saxon.sf.net/'">saxon</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/map'">map</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/array'">array</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/math'">math</xsl:when>
            <xsl:when test="$ns = 'http://expath.org/ns/archive'">expath-archive</xsl:when>
            <xsl:when test="$ns = 'http://expath.org/ns/binary'">expath-binary</xsl:when>
            <xsl:when test="$ns = 'http://expath.org/ns/file'">expath-file</xsl:when>
            <xsl:when test="$ns = 'http://expath.org/ns/zip'">expath-zip</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/common'">exslt-common</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/random'">exslt-random</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/math'">exslt-math</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/sets'">exslt-sets</xsl:when>
            <xsl:when test="$ns = 'http://exslt.org/dates-and-times'"
                >exslt-dates-and-times</xsl:when>
            <xsl:when test="$ns = 'http://saxon.sf.net/sql'">sql</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$ns"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <!-- Determine specifications text within 'Saxon availability' section: 
  Spec versions in which the functions are defined; or for 'expath' functions, the xpath requirement information. -->
    <xsl:function name="fc:specs" as="xs:string*">
        <xsl:param name="in" as="element(fc:in-spec)*"/>
        <xsl:variable name="my-function" select="$in/ancestor::fc:function"/>
        <xsl:variable name="namespace" select="$in/ancestor::section/@id"/>
        <xsl:variable name="default.saxon"
            select="$implementation-doc/implement/default[@id = $namespace]/@saxon"/>
        <xsl:variable name="default.xpath"
            select="$implementation-doc/implement/default[@id = $namespace]/@xpath"/>
        <xsl:choose>
            <!-- Get XPath requirement information for functions in 'expath' namespace. -->
            <xsl:when test="contains($in[1], 'expath')">
                <xsl:value-of
                    select="
                        $implementation-doc/implement/xpath[@version = ($my-function/fc:xpath,
                        $default.xpath)[1]]"
                />
            </xsl:when>
            <!-- No XPath or XSLT requirement information for functions in 'exslt' namespace. -->
            <xsl:when test="contains($in[1], 'exslt')"> </xsl:when>
            <xsl:otherwise>
                <!-- in-spec elements should be in order such that first one $in[1] is earliest spec, to ensure the correct message. -->
                <xsl:value-of select="$implementation-doc/implement/spec[@value = $in[1]]"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <!-- Determine editions & versions text within 'Saxon availability' section. -->
    <xsl:function name="fc:edition" as="xs:string*">
        <xsl:param name="available" as="xs:string"/>
        <xsl:variable name="available"
            select="
                for $t in tokenize($available, '\s+')
                return
                    upper-case($t)"
            as="xs:string*"/>
        <xsl:for-each select="$available">
            <xsl:value-of select="$implementation-doc/implement/saxon[@version = current()]"/>
        </xsl:for-each>
    </xsl:function>
    
    <!-- Determine platform text within 'Saxon availability' section. -->
    <xsl:function name="fc:platform" as="xs:string*">
        <xsl:param name="available" as="xs:string"/>
        <xsl:variable name="availableSeq"
            select="for $t in tokenize($available, '\s+') return upper-case($t)" as="xs:string*"/>
        <xsl:variable name="platforms" as="xs:string">
            <xsl:choose>
                <xsl:when test="$availableSeq = 'J' and $availableSeq = 'C'">
                    <xsl:value-of select="'J_C'"/>
                </xsl:when>
                <xsl:when test="$availableSeq = 'J' and $availableSeq = 'CS'">
                    <xsl:value-of select="'J_CS'"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="$available"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <!--<xsl:message>platforms: <xsl:value-of select="$platforms"/></xsl:message>-->
        
        <xsl:for-each select="$platforms">
            <xsl:value-of select="$implementation-doc/implement/saxon[@platform = current()]"/>
        </xsl:for-each>
    </xsl:function>

    <!-- Determine text for 'Links to specifications' links. -->
    <xsl:function name="fc:spec-link-name" as="xs:string">
        <xsl:param name="p" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$p = 'xpath20'">XPath 2.0 Functions and Operators</xsl:when>
            <xsl:when test="$p = 'xpath30'">XPath 3.0 Functions and Operators</xsl:when>
            <xsl:when test="$p = 'xpath31'">XPath 3.1 Functions and Operators</xsl:when>
            <xsl:when test="$p = 'xslt20'">XSLT 2.0 Specification</xsl:when>
            <xsl:when test="$p = 'xslt30'">XSLT 3.0 Specification</xsl:when>
            <xsl:when test="$p = 'xquery-update10'">XQuery Update 1.0</xsl:when>
            <xsl:when test="$p = 'expath-archive'">EXPath Archive Module</xsl:when>
            <xsl:when test="$p = 'expath-binary'">EXPath Binary Module</xsl:when>
            <xsl:when test="$p = 'expath-file'">EXPath File Module</xsl:when>
            <xsl:when test="$p = 'exslt-common'">EXSLT Common Module</xsl:when>
            <xsl:when test="$p = 'exslt-math'">EXSLT Math Module</xsl:when>
            <xsl:when test="$p = 'exslt-random'">EXSLT Random Module</xsl:when>
            <xsl:when test="$p = 'exslt-sets'">EXSLT Sets Module</xsl:when>
            <xsl:when test="$p = 'exslt-dates-and-times'">EXSLT Dates and Times Module</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$p"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <!-- Determine href for 'Links to specifications' links. -->
    <xsl:function name="fc:spec-href" as="xs:string">
        <xsl:param name="p" as="xs:string"/>
        <xsl:param name="fname" as="element()*"/>
        <xsl:variable name="spec-address">
            <xsl:choose>
                <xsl:when test="$p = 'xpath30'"
                    >http://www.w3.org/TR/xpath-functions-30/#func-</xsl:when>
                <xsl:when test="$p = 'xpath31'"
                    >http://www.w3.org/TR/xpath-functions-31/#func-</xsl:when>
                <xsl:when test="$p = 'xslt20'">http://www.w3.org/TR/xslt20/#function-</xsl:when>
                <xsl:when test="$p = 'xslt30'">http://www.w3.org/TR/xslt-30/#func-</xsl:when>
                <xsl:when test="$p = 'xquery-update10'"
                    >http://www.w3.org/TR/xquery-update-10/#id-func-</xsl:when>
                <xsl:when test="$p = 'expath-archive'"
                    >http://www.expath.org/spec/archive/editor#fn.</xsl:when>
                <xsl:when test="$p = 'expath-binary'">http://www.expath.org/spec/binary#</xsl:when>
                <xsl:when test="$p = 'expath-file'">http://www.expath.org/spec/file#</xsl:when>
                <xsl:when test="$p = 'exslt-common'">http://www.exslt.org/exsl/functions/</xsl:when>
                <xsl:when test="$p = 'exslt-math'">http://www.exslt.org/math/functions/</xsl:when>
                <xsl:when test="$p = 'exslt-random'"
                    >http://www.exslt.org/random/functions/</xsl:when>
                <xsl:when test="$p = 'exslt-sets'">http://www.exslt.org/set/functions/</xsl:when>
                <xsl:when test="$p = 'exslt-dates-and-times'"
                    >http://www.exslt.org/date/functions/</xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="$p"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="namespace-ext">
            <xsl:choose>
                <xsl:when test="$fname/@namespace = 'http://www.w3.org/2005/xpath-functions/math'"
                    >math-</xsl:when>
                <xsl:when test="$fname/@namespace = 'http://www.w3.org/2005/xpath-functions/map'"
                    >map-</xsl:when>
                <xsl:when test="$fname/@namespace = 'http://www.w3.org/2005/xpath-functions/array'"
                    >array-</xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="''"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="function-ref">
            <xsl:choose>
                <xsl:when test="$p = 'expath-archive'">
                    <xsl:choose>
                        <xsl:when test="$fname = 'extract-map'">extractmap</xsl:when>
                        <xsl:when test="$fname = 'entry-names'">entry.names.map</xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="replace($fname, '-', '.')"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:when>
                <xsl:when test="$p = 'expath-file'">
                    <xsl:choose>
                        <xsl:when
                            test="
                                $fname = ('exists',
                                'is-dir',
                                'is-file',
                                'last-modified',
                                'size',
                                'dir-separator',
                                'line-separator',
                                'path-separator',
                                'temp-dir',
                                'base-dir',
                                'current-dir')">
                            <xsl:value-of select="concat('pr.', $fname)"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="concat('fn.', $fname)"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="$fname"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="link-end">
            <xsl:choose>
                <xsl:when test="contains($fname/@namespace, 'exslt.org')">/index.html</xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="''"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="concat($spec-address, $namespace-ext, $function-ref, $link-end)"/>
    </xsl:function>
</xsl:stylesheet>
