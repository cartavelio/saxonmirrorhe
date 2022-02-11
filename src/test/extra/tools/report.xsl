<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:t="http://www.w3.org/XML/2004/xml-schema-test-suite/" xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:saxon="http://saxon.sf.net/"
    exclude-result-prefixes="xs saxon t" version="2.0">
    
    <xsl:param name="rundate" select="'20170607'"/>

    <xsl:variable name="input-docs" as="document-node(element(t:testSuiteResults))*"
        select="collection(concat($rundate,'/results/', '?*.xml'))"/>

    <xsl:output indent="yes"/>

    <xsl:template match="/">
        <xsl:result-document href="report.xml">
            <out>
                <xsl:comment>
                Generated - <xsl:value-of select="current-dateTime()"/> 
            </xsl:comment>
                <xsl:for-each select="$input-docs">
                    <xsl:variable name="test-cases"
                        select="/t:testSuiteResults/t:testResult"/>
                    <xsl:variable name="failures" select="$test-cases[@saxon:outcome='different']"/>
                    <product edition="{//t:testSuiteResults/@processor}"
                        lang="{substring-after(/t:testSuiteResults/@processor,'xsdVersion=')}">
                       
                        <test-run date="{/t:testSuiteResults/@submitDate}"/>
                        <tests successes="{count($test-cases[@saxon:outcome='same'])}"
                            failures="{count($failures)}"/>
                        <xsl:if test="count($failures) gt 30">
                            <failure-distribution>
                                <xsl:for-each-group select="$failures" group-by="../@set">
                                    <test-set-failures name="{current-grouping-key()}"
                                        no="{count(current-group())}"/>
                                </xsl:for-each-group>
                            </failure-distribution>
                        </xsl:if>
                        <xsl:for-each select="$failures">
                            <test-case-fail name="{@test}"/>
                        </xsl:for-each>
                    </product>
                </xsl:for-each>
            </out>
        </xsl:result-document>

    </xsl:template>


</xsl:stylesheet>
