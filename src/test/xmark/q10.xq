(: Q10. List all persons according to their interest;
--      use French markup in the result.  :)
declare boundary-space strip;
for $i in distinct-values(
          /site/people/person/profile/interest/@category)
let $p := for    $t in /site/people/person
          where  $t/profile/interest/@category = $i
            return <personne>
                <statistiques>
                        <sexe>{ $t/profile/gender }</sexe>
                        <age>{ $t/profile/age }</age>
                        <education>{ $t/profile/education}</education>
                        <revenu>{ $t/profile/@income } </revenu>
                </statistiques>
                <coordonnees>
                        <nom>{ $t/name }</nom>,
                        <rue>{ $t/address/street }</rue>
                        <ville>{ $t/address/city }</ville>
                        <pays>{ $t/address/country }</pays>
                        <reseau>
                                <courrier>{ $t/emailaddress }</courrier>
                                <pagePerso>{ $t/homepage }</pagePerso>
                        </reseau>
                </coordonnees>
                <cartePaiement>{ $t/creditcard }</cartePaiement>    
              </personne>
return <categorie>
        <id>{ $i }</id>
        { $p }
      </categorie>