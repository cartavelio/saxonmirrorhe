(: Q5.  How many sold items cost more than 40? :)

count(for $i in (:document("auction.xml"):)/site/closed_auctions/closed_auction
        where  $i/price >= 40 
        return $i/price)