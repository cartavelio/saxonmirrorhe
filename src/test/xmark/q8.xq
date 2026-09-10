(: Q8.  List the names of persons and the number of items they bought.
--     (joins person, closed\_auction)} :)

for $p in (:document("auction.xml"):)/site/people/person
let $a := for $t in (:document("auction.xml"):)/site/closed_auctions/closed_auction
             where $t/buyer/@person = $p/@id
             return $t
return <item person="{$p/name}"> {count ($a)} </item>