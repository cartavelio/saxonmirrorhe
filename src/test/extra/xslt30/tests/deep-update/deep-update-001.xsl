<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="#all" 
   expand-text="yes"
   version="3.0">

   <xsl:template name="xsl:initial-template">
      <xsl:variable name="glossary" select="json-doc('glossary.json')"/>
      <xsl:variable name="updated-glossary" as="map(*)">
         <saxon:deep-update
            root="$glossary"
            select="?glossary?GlossDiv?GlossList?GlossEntry"
            action="map:put(., 'GlossSee', 'markdown')"/>
      </xsl:variable>
      <xsl:message>{serialize($updated-glossary, map{"method":"json", "indent":true()})}</xsl:message>
      <out>{$updated-glossary?glossary?GlossDiv?GlossList?GlossEntry?GlossSee}</out>
   </xsl:template>

 

</xsl:stylesheet>

<!--  "glossary": {
        "title": "example glossary",
		  "GlossDiv": {
           "title": "S",
			  "GlossList": {
              "GlossEntry": {
                "ID": "SGML",
				    "SortAs": "SGML",
				    "GlossTerm": "Standard Generalized Markup Language",
				    "Acronym": "SGML",
				    "Abbrev": "ISO 8879:1986",
				    "GlossDef": {
                  "para": "A meta-markup language, used to create markup languages such as DocBook.",
					   "GlossSeeAlso": ["GML", "XML"]
                },
				   "GlossSee": "markup"
             }
          }
       }
    } -->
