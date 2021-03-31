<?xml version="1.0" encoding="utf-8"?> 

<?dependency saxon?>

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:saxon="http://saxon.sf.net/"
   extension-element-prefixes="saxon"
>

  <xsl:template match="/">
    <saxon:doctype xsl:extension-element-prefixes="saxon">
      <dtd:doctype name="booklist"
        xmlns:dtd="http://saxon.sf.net/dtd" xsl:exclude-result-prefixes="dtd">
        <dtd:element name="booklist" content="(book)*"/>
        <dtd:element name="book" content="EMPTY"/>
        <dtd:attlist element="book">
          <dtd:attribute name="isbn" type="ID" value="#REQUIRED"/>
          <dtd:attribute name="title" type="CDATA" value="#IMPLIED"/>
        </dtd:attlist>
        <dtd:entity name="blurb">'A <i>cool</i> book with &gt; 200 pictures!'</dtd:entity>
        <dtd:entity name="cover" system="cover.gif" notation="GIF"/>
        <dtd:notation name="GIF" system="http://gif.org/"/>
        <dtd:entity name="ISOEntities" 
          public="ISO 8879-1986//ENTITIES ISO Character Entities 20030531//EN//XML"
          system="file:///D:/ent/ISOEntities.ent"
          parameter="yes"/>     
        <xsl:text>%ISOEntities;</xsl:text>
      </dtd:doctype>
    </saxon:doctype>
    <xsl:apply-templates/>
  </xsl:template>
  
  <xsl:template match="BOOKLIST">
    <booklist>
      <xsl:apply-templates/>
    </booklist>
  </xsl:template>
  
  <xsl:template match="ITEM">
    <book title="{TITLE}" isbn="ISBN-{ISBN}"/>
  </xsl:template>

</xsl:stylesheet>
