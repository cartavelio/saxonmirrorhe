(: Q15. Print the keywords in emphasis in annotations of closed auctions. :)

for $a in (:document("auction.xml"):)/site/closed_auctions/closed_auction/annotation/
            description/parlist/listitem/parlist/listitem/text/emph/keyword
return <text>{ $a }</text>

