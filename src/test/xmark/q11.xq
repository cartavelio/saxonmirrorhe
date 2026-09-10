(: Q11. For each person, list the number of items currently on sale whose
      price does not exceed 0.02% of the person's income. :)

for $p in (:document("auction.xml"):)/site/people/person
let $l := for $i in (:document("auction.xml"):)/site/open_auctions/open_auction/initial
          where $p/profile/@income > (5000 * $i)
          return $i
return <items name="{$p/name}">{ count ($l) }</items>