(: Q19. Give an alphabetically ordered list of all
--      items along with their location. :)

for    $b in (:document("auction.xml"):)/site/regions//item
let    $k := $b/name
order by $k
return <item name="{$k}">{ $b/location } </item>
