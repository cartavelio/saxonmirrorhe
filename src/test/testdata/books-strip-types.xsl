<xsl:transform
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema"
 input-type-annotations="strip"
 version="3.0"
>

<!-- This style sheet processes the books.xml file with input-type-annotations=strip. 
     The books.xml file should be validated using books11.xsd -->

<xsl:template match="/">
  <out
    tax-expanded="{(//ITEM)[1]/@TAX = 'V'}"
    id-retained="{id('P') is (//CATEGORY)[1]}"
    price-typed="{data((//ITEM)[1]/PRICE) instance of xs:numeric}" 
    >

  </out>
 
</xsl:template>

</xsl:transform>