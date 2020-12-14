(: Q14. Return the names of all items whose description contains the 
--      word `gold'. :)

for $i in (:document("auction.xml"):)/site//item
where contains ($i/description,"gold")
return ($i/name, $i/description)
