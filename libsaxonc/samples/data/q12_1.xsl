<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:math="http://www.w3.org/2005/xpath-functions/math"
    exclude-result-prefixes="xs math"
    version="3.0">
    <xsl:template match="/">
    
    <result>
        <!-- Q12.   For each richer-than-average person, list the number of items
      currently on sale whose price does not exceed 0.02% of the
       person's income. -->
        
        
        
        <xsl:for-each select="site/people/person">
            
            <xsl:variable name="p" select="." as="element(person)"/>
            
            <xsl:variable name="l" as="element(initial)*" select="doc('xmark100k.xml')/site/open_auctions/open_auction/initial[$p/profile/@income > (5000 * .)]" />
            
            <xsl:if test="$p/profile/@income > 50000">
                <item person="{name}">
                    <xsl:value-of select="count($l)"/>
                </item>
            </xsl:if>
        </xsl:for-each>
        
    </result>
        
    </xsl:template>
    
</xsl:stylesheet>
