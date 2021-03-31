<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:saxon="http://saxon.sf.net/" xmlns:java="http://java.com/namespace"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="saxon">

  <!-- PURPOSE:  Test saxon:single-quotes with HTML output -->
  <?dependency saxon?>

  <xsl:template name="main">
    <xsl:result-document method="html" saxon:single-quotes="true">
      <html>
        <body>
          <p class="body" id='top' title="O'Connor">Content</p>
        </body>
      </html>
    </xsl:result-document>
  </xsl:template>
</xsl:stylesheet>
