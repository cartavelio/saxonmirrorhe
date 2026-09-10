<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon xs">

<!-- PURPOSE:  Test saxon:hexBinary output method -->
<?dependency saxon?>

<xsl:output method="xml" encoding="iso-8859-1"/>

<xsl:template name="main">
  <html>
    <head>
      <title>An image</title>
    </head>
    <body>
      <h1>An image</h1>
      <p><img src="image004.gif"/></p>
    </body>
  </html>
  <xsl:result-document method="saxon:hexBinary" href="image004.gif" media-type="image/gif">
    <xsl:text>89504E470D0A1A0A0000000D49484452</xsl:text>
    <xsl:text>0000000A0000000A08060000008D32CF</xsl:text>
    <xsl:text>BD0000000467414D410000B18F0BFC61</xsl:text>
    <xsl:text>05000000097048597300000B1300000B</xsl:text>
    <xsl:text>1301009A9C180000000774494D4507D6</xsl:text>
    <xsl:text>06011739281D1757E20000001D744558</xsl:text>
    <xsl:text>74436F6D6D656E740043726561746564</xsl:text>
    <xsl:text>2077697468205468652047494D50EF64</xsl:text>
    <xsl:text>256E0000005D4944415418D3BDCCBD0D</xsl:text>
    <xsl:text>825000C4F11F74B4ECE0164CE0320EE0</xsl:text>
    <xsl:text>2ECEC1083626843D2C4D5EC5D958BC10</xsl:text>
    <xsl:text>5E67B8EA721F7FFEAD6E1F84CBCF0E1D</xsl:text>
    <xsl:text>EBE12B5CC31C5EE1D6C4877728E1139E</xsl:text>
    <xsl:text>75D757A309A5CAB630B688F7B0844773</xsl:text>
    <xsl:text>74BEBE0C921A177B4AD98A0000000049</xsl:text>
    <xsl:text>454E44AE426082</xsl:text>
  </xsl:result-document>
</xsl:template>
</xsl:stylesheet>