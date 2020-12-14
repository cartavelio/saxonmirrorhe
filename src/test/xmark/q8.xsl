<result xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xsl:version="2.0">
<!-- Q8.  List the names of persons and the number of items they bought.
          (joins person, closed_auction) -->

  <xsl:for-each select="/site/people/person">
    <xsl:variable name="a" 
       select="/site/closed_auctions/closed_auction[buyer/@person = current()/@id]"/>
    <item person="{name}"><xsl:value-of select="count($a)"/></item>  
  </xsl:for-each>
  
</result>  