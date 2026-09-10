<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

   <xsl:use-package name="http://example.com/myPackage" package-version="1.1-alpha" />

   <xsl:template name="xsl:initial-template">
      <xsl:call-template name="myTemplate" />
   </xsl:template>
</xsl:stylesheet>