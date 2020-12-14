(:-- Q18.Convert the currency of the reserve of all open auctions to 
--     another currency. :)

declare namespace f="http://f/";
declare function f:convert ($v)
{
  2.20371 * $v (: convert Dfl to Euro :)
};

for    $i in (:document("auction.xml"):)/site/open_auctions/open_auction
return f:convert($i/reserve)