<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
        xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
        xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        xmlns:saxon="http://saxon.sf.net/" extension-element-prefixes="saxon" 
        xmlns:hyper="http://www.hyperbase.com/cosmos/hyper"
        exclude-result-prefixes="hyper xsl xs saxon"
        >
        <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
        
        <!-- A more complex test of saxon:expression() and saxon:eval() with parameters. Was saxon075 -->
        
        <xsl:variable name="res" select="document('evaluate-024b.xml')/*"/>
        
        <xsl:function name="hyper:getres">
                <xsl:param name="xpathstr"/>
                <xsl:param name="params"/>
                <xsl:variable name="para" select="tokenize($params, ' ')"/>
                <xsl:variable name="ttt" select="saxon:expression('($p1,$p2)')"/>
                <xsl:for-each select="$res">
                   <ttt><xsl:copy-of select="saxon:eval($ttt, $para[1], $para[2], $para[3], $para[4], $para[5], $para[6], $para[7], $para[8], $para[9])"/></ttt>
                   <res><xsl:copy-of select="saxon:eval(saxon:expression($xpathstr), $para[1], $para[2], $para[3], $para[4], $para[5], $para[6], $para[7], $para[8], $para[9])"/></res>
                </xsl:for-each>
        </xsl:function>

        <xsl:template name="main">
           <xsl:variable name="name" select="'a5'"/>     
           <out>
                <message>only p1 : <xsl:copy-of select="hyper:getres('.//entity[@id = $p1]', concat($name, ' ', 'company'))"/></message>
                   <message>only p2 : <xsl:copy-of select="hyper:getres('.//entity[@class = $p2]', concat($name, ' ', 'company'))"/></message>
                   <message>p1 + p2 : <xsl:copy-of select="hyper:getres('.//entity[@id = $p1 and @class = $p2]', concat($name, ' ', 'company'))"/></message>
                <message>static : <xsl:copy-of select="$res//entity[@id = $name and @class = 'company']"/></message>
           </out>
        </xsl:template>

</xsl:stylesheet>
