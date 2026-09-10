(: Q2. Return the initial increases of all open auctions. :)

for $b in (:document("auction.xml"):)/site/open_auctions/open_auction
return <increase> {$b/bidder[1]/increase } </increase>