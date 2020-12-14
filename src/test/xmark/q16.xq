(: -- Q16. Return the IDs of those auctions
--      that have one or more keywords in emphasis. (cf. Q15) :)

for $a in (:document("auction.xml"):)/site/closed_auctions/closed_auction
where exists ($a/annotation/description/parlist/listitem/parlist/
                     listitem/text/emph/keyword/text())
return <person id="{$a/seller/@person}" />
