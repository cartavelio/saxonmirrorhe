(:-- Q17. Which persons don't have a homepage? :)

for    $p in (:document("auction.xml"):)/site/people/person
where  empty($p/homepage/text())
return <person>{$p/name}</person>
