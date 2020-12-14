(: Q6. How many items are listed on all continents? :)

for    $b in (:document("auction.xml"):)/site/regions/*
return count ($b//item)

(: QUERY CORRECTED, previously said /site/region - ignore old measurements :)