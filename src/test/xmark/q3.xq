(: Q3. Return the IDs of all open auctions whose current
     increase is at least twice as high as the initial increase. :)

for    $b in (:document("auction.xml"):)/site/open_auctions/open_auction
where  $b/bidder[1]/increase *2 <= $b/bidder[last()]/increase
return <increase first="{$b/bidder[1]/increase}"
                 last="{$b/bidder[last()]/increase}"/>