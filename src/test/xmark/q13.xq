(: Q13. List the names of items registered in Australia along with 
--      their descriptions. :)

for $i in (:document("auction.xml"):)/site/regions/australia/item
return <item name="{$i/name}">{ $i/description }</item>