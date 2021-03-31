<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- Test of saxon:threads on xsl:result-document with a document
     the test is based on xsl:For-each-group with attribute group-ending -->

  <xsl:output indent="yes"/>
  <xsl:strip-space elements="*"/>


  <xsl:template match="cities">
    <!-- simple test of value-based grouping -->

    <xsl:for-each-group select="city" group-ending-with="*[ends-with(@country,'ly')]">
      <xsl:result-document href="threads017a.out" saxon:threads="2"
        xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
        <table>
          <tr>
            <th>Position</th>
            <th>Country</th>
            <th>City List</th>
            <th>Population</th>
          </tr>


          <xsl:for-each select="current-group()">
            <tr>
              <td>
                <xsl:value-of select="position()"/>
              </td>
              <td>
                <xsl:value-of select="@country"/>
              </td>
              <td>
                <xsl:value-of select="@name"/>
              </td>
              <td>
                <xsl:value-of select="sum(current-group()/@pop)"/>
              </td>
            </tr>
          </xsl:for-each>


        </table>
      </xsl:result-document>
    </xsl:for-each-group>


  </xsl:template>
</xsl:stylesheet>
