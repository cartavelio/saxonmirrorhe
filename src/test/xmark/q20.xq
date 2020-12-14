(: -- Q20. Group customers by their
--      income and output the cardinality of each group. :)

<result>
 <preferred>
  {count ((:document("auction.xml"):)/site/people/person/profile[@income >= 100000])}
 </preferred>
 <standard>
  {count ((:document("auction.xml"):)/site/people/person/profile[@income < 100000
                                                        and @income >= 30000])}
 </standard>
 <challenge> 
  {count ((:document("auction.xml"):)/site/people/person/profile[@income < 30000])}
 </challenge>
 <na>
  {count (for    $p in (:document("auction.xml"):)/site/people/person
         where  empty($p/profile/@income)
         return $p)}
 </na>
</result>