<?xml version="1.0" encoding="UTF-8"?>
<scm:schema xmlns:scm="http://ns.saxonica.com/schema-component-model">
   <scm:simpleType id="C0" name="enumRegimePatrimoniale"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C1"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
      <scm:enumeration value="S"/>
      <scm:enumeration value="C"/>
   </scm:simpleType>
   <scm:simpleType id="C2" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2|3|4|5|6|7|8"/>
   </scm:simpleType>
   <scm:complexType id="C4" name="PraticaEmigrazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C8" name="IscrizioneAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C9"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C10"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C11"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C12"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C13"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C14"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C15" name="tipoDatiNascita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C16"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C17"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C18" name="tipoStatoCode"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C19"/>
      <scm:attributeUse required="false" ref="C20" default="0"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C21"/>
   </scm:complexType>
   <scm:complexType id="C22" name="Maternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C23"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C24"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C25"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C26" name="PraticaIscrizioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C27"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C28"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C29"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C30"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C31"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C32"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C33" name="tipoUfficio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C34"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C35"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C36" name="Rettifica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C37"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C38"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C39"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C40" name="tipoUbicazioneConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C41"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C42"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C43"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C44"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C45"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C46"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C47" name="PraticaIscrizioneAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C48" name="alfabetico"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="([a-zA-Z])*"/>
   </scm:simpleType>
   <scm:simpleType id="C49" name="enumGruppoLinguistico"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:complexType id="C50" name="tipoNumeroCivicoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C51"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C52"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C53"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C54"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C55" name="enumMotivoSpedizionePratica"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:complexType id="C56" name="Morte"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C57"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C58"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C59" name="enumTipoPratica"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C1"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
      <scm:enumeration value="D"/>
      <scm:enumeration value="R"/>
      <scm:enumeration value="H"/>
      <scm:enumeration value="C"/>
      <scm:enumeration value="B"/>
      <scm:enumeration value="G"/>
      <scm:enumeration value="I"/>
      <scm:enumeration value="M"/>
      <scm:enumeration value="A"/>
      <scm:enumeration value="F"/>
      <scm:enumeration value="U"/>
      <scm:enumeration value="N"/>
      <scm:enumeration value="E"/>
   </scm:simpleType>
   <scm:complexType id="C60" name="tipoConiuge"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C61"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C62"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C63"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C64" name="tipoEnteConsultabile"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="INA|AE"/>
   </scm:simpleType>
   <scm:complexType id="C65" name="PraticaIscrizioneCancellazioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C66"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C1" name="char1" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
   </scm:simpleType>
   <scm:complexType id="C67" name="Atto"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C68"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C69"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C70"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C71"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C72"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C73"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C74"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C75"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C76"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C77"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C78"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C79" name="CorpoRisposta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C80"/>
   </scm:complexType>
   <scm:simpleType id="C81" name="charMax120"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="120"/>
   </scm:simpleType>
   <scm:simpleType id="C82" name="charMax4"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="4"/>
   </scm:simpleType>
   <scm:complexType id="C83" name="tipoConsultazionePerNomeCognome"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C84"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C85"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C86"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C87"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C88" name="tipoConsultazionePerCodiceFiscale"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C89"/>
   </scm:complexType>
   <scm:simpleType id="C90" name="tipoVersione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C91"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:simpleType id="C92" name="tipoProtocollo"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="20"/>
   </scm:simpleType>
   <scm:simpleType id="C93" name="numerico4"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="4"/>
   </scm:simpleType>
   <scm:complexType id="C95" name="PersonaDatiNascita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C96"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C100"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C101" name="tipoResidenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C102"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C103"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C104"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C105"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C106" name="tipoFamigliaAnagrafica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C107"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C108"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C109"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C110"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C111" name="tipoElencoSinonimiPersoneFisicheConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C112"/>
   </scm:complexType>
   <scm:simpleType id="C3" name="numerico1"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C113" name="charMax16"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="16"/>
   </scm:simpleType>
   <scm:simpleType id="C114" name="enumTipoRelazioneFamiliare"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:complexType id="C115" name="tipoCessazioneAnnullamentoMatrimonio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C116"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C117"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C118" name="numericoMax6"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="6"/>
   </scm:simpleType>
   <scm:simpleType id="C119" name="enumProvvedimentoCancellazione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:complexType id="C120" name="tipoComune"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C121"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C122"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C123"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C124" base="#string" variety="atomic" primitiveType="#string">
      <scm:pattern value="A|C|M|R"/>
   </scm:simpleType>
   <scm:complexType id="C125" name="DatiEmigrazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C126"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C127"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C128"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C129" name="Vedovanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C130"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C131"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C132"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C133"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C134" name="tipoUfficioConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C135"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C136"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C137" name="InterrogazioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C138"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C139"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C140" name="tipoUbicazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C141"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C142"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C143"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C144"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C145"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C146"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C147" name="Colore"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2"/>
   </scm:simpleType>
   <scm:complexType id="C148" name="tipoAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C149"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C150"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C151"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C152"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C153" base="#string" variety="atomic" primitiveType="#string">
      <scm:pattern value="([0-9])*|s.n.c."/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:complexType id="C154" name="tipoToponimoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C155"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C156"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C157" name="charMax10NoMin"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="([0-9])*"/>
      <scm:maxLength value="10"/>
   </scm:simpleType>
   <scm:complexType id="C158" name="PersonaAnagraficaDN"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C96"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C159"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C160" name="enumMotivoRestituzioneSenzaProvvedimento"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C161"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="7"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="6"/>
      <scm:enumeration value="10"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="9"/>
      <scm:enumeration value="4"/>
      <scm:enumeration value="8"/>
   </scm:simpleType>
   <scm:complexType id="C162" name="Luogo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C163"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C164"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C165"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C166" name="tipoStoricoResidenzeConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C167"/>
   </scm:complexType>
   <scm:simpleType id="C168" name="enumTipoCancellazioneAnagrafe"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C1"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
      <scm:enumeration value="H"/>
      <scm:enumeration value="G"/>
      <scm:enumeration value="I"/>
      <scm:enumeration value="M"/>
      <scm:enumeration value="F"/>
      <scm:enumeration value="E"/>
   </scm:simpleType>
   <scm:complexType id="C169" name="tipoMessaggio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C170"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C171"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C172"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C173" name="tipoRettifica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C174"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C175"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C176"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C177" name="Generalita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C178"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C179"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C180"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C181" name="tipoUbicazioneEstera"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C182"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C183"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C184"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C185"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C186" name="tipoStoricoResidenzeEstere"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C187"/>
   </scm:complexType>
   <scm:complexType id="C188" name="tipoAtto"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C189"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C190"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C191"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C192"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C193"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C194"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C195"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C196"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C197"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C198" name="PersonaAnagrafica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C96"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C199"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C200"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C201" name="ResidenzaEstera"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C202"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C203"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C204"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C205"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C206"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C207" name="tipoRispostaDecessoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C208"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C209"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C210" name="tipoSentenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C211"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C212"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C213"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C214" name="char3" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="3"/>
   </scm:simpleType>
   <scm:simpleType id="C215" name="enumComportaConteggio"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C216" name="enumProvvedimentoIscrizione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:complexType id="C217" name="tipoComuneConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C218"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C219"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C220"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C221" name="numerico11"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="11"/>
   </scm:simpleType>
   <scm:complexType id="C222" name="tipoSinonimoPersoneFisiche"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C223"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C224"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C225"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C226"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C227"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C228" name="tipoResidenzaEstera"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C229"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C230"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C231"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C232"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C233" name="Coniuge"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C234"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C235"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C236"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C237" name="Emigrazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C238"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C239"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C240"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C241"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C96" name="Persona"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C242" name="tipoLuogoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C243"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C244"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C245"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C246" name="enumTipoIscrizioneAnagrafe"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C1"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
      <scm:enumeration value="D"/>
      <scm:enumeration value="R"/>
      <scm:enumeration value="C"/>
      <scm:enumeration value="B"/>
      <scm:enumeration value="A"/>
      <scm:enumeration value="U"/>
      <scm:enumeration value="N"/>
   </scm:simpleType>
   <scm:complexType id="C247" name="tipoMorte"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C248"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C249"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C250"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C251" name="Toponimo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C252"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C253"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C254" name="numerico6"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="6"/>
   </scm:simpleType>
   <scm:complexType id="C255" name="tipoResidenzaConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C256"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C257"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C258"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C259"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C260" name="tipoStoricoResidenzeEstereConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C261"/>
   </scm:complexType>
   <scm:complexType id="C5" name="Pratica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C262" name="CartaIdentita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C263"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C264"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C265"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C266"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C267"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C268"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C269"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C270"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C271" name="charMax1024"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="1024"/>
   </scm:simpleType>
   <scm:complexType id="C272" name="CausaleConsultazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C273"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C274"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C275"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C276"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C277"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C278" name="tipoLuogo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C279"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C280"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C281"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C282" name="Paternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C283"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C284"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C285"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C286" name="Divorzio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C287"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C288"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C289"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C290"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C291" name="tipoGeneralita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C292"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C293"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C294"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C295"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C296"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C297"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C298"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C299"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C300"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C301"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C302"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C303" name="CorpoInterrogazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C304"/>
   </scm:complexType>
   <scm:complexType id="C305" name="Comune"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C306"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C307"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C308"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C94" name="numerico"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
   </scm:simpleType>
   <scm:simpleType id="C309" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2"/>
   </scm:simpleType>
   <scm:complexType id="C310" name="tipoPatente"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C311"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C312"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C313"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C314"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C315"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C316"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C317" name="tipoResidenzaEsteraConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C318"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C319"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C320"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C321"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C322" name="tipoNumeroCivico"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C323"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C324"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C325"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C326"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C327" name="charMax10"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="10"/>
   </scm:simpleType>
   <scm:simpleType id="C328" name="tipoProtocolloAnagrafe"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:complexType id="C329" name="tipoLocalita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C330"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C331"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C332"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C333" name="tipoVariazioneSesso"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C334"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C335"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C336"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C337" name="tipoCancellazioneAPR"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C338"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C339"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C340"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C341"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C342"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C343"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C344" name="tipoPresaInCarico"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C345"/>
      <scm:attributeUse required="false" ref="C346" default="0"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C347"/>
   </scm:complexType>
   <scm:complexType id="C348" name="tipoIndirizzo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C349"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C350"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C351"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C352"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C353"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C354"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C355" name="tipoVeicoli"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C356"/>
   </scm:complexType>
   <scm:complexType id="C357" name="tipoCivicoInterno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C358"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C359"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C360"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C361" name="TesseraSanitaria"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C362"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C363"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C364"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C365" name="FamigliaAnagrafica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C366"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C367"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C368"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C369"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C370" name="charMax3"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="3"/>
   </scm:simpleType>
   <scm:simpleType id="C371" name="enumTipoRichiestaCancellazione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:complexType id="C372" name="Cittadinanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C373"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C374"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C375"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C376" name="RispostaAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C377"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C378"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C379"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C380" name="Nascita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C381"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C382"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C383"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C384"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C385" name="tipoComGenerica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C386"/>
      <scm:attributeUse required="true" ref="C387"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C388"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C389"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C390"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C391"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C392"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C393"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C394"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C395"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C396"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C397"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C398"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C399"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C400"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C401"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C402"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C403"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C404"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C405"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C406"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C407"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C408"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C409" name="Pensione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C410"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C411"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C412"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C413" name="Richiesta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C414"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C415"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C416"/>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C417"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C418" name="numerico3"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="3"/>
   </scm:simpleType>
   <scm:complexType id="C419" name="tipoAssegnazioneCodiceFiscaleConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C420"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C421"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C422"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C423" name="tipoCivicoInternoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C424"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C425"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C426"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C427" name="enumTipoConsultazione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="richiesta da comune"/>
      <scm:enumeration value="risposta da comune"/>
   </scm:simpleType>
   <scm:complexType id="C428" name="Comunicazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C429"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C430"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C431"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C432" name="StatoCode"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C433"/>
      <scm:attributeUse required="false" ref="C434" default="0"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C435"/>
   </scm:complexType>
   <scm:simpleType id="C436" base="C161" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
      <scm:pattern value="1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|20|21|22|23"/>
   </scm:simpleType>
   <scm:complexType id="C437" name="tipoConsultazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C438"/>
      <scm:attributeUse required="true" ref="C439"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C440"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C441"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C442" name="Messaggio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C443"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C444"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C445"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C446" name="tipoDatiRettificati"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C447"/>
   </scm:complexType>
   <scm:complexType id="C448" name="tipoLocalitaConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C449"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C450"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C451"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C452" name="tipoCausale"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C453"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C454"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C455"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C456"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C457" name="DatiAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C458"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C459"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C460" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2"/>
   </scm:simpleType>
   <scm:complexType id="C461" name="CancellazioneAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C462"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C463"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C464"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C465" name="charMax80"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="80"/>
   </scm:simpleType>
   <scm:simpleType id="C466" name="charMax8"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="8"/>
   </scm:simpleType>
   <scm:complexType id="C467" name="tipoApplicazioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C468"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C469"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C470"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C471" name="tipoVariazioneCittadinanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C472"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C473"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C474" name="tipoPermessoSoggiorno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C475"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C476"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C477"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C478"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C479"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C480" name="tipoMittente"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C481"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C482"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C483" name="tipoVariazioneNomeCognome"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C484"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C485"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C486"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C487" base="#string" variety="atomic" primitiveType="#string">
      <scm:pattern value="([0-9])*|s.n.c."/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:complexType id="C488" name="tipoConfermaDatiAnagraficiConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C489"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C490"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C491"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C91" name="charMax20"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:simpleType id="C492" name="numerico8"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="8"/>
   </scm:simpleType>
   <scm:simpleType id="C493" name="tipoSN" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C494"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="2"/>
      <scm:pattern value="SI|NO|si|no|Si|No|s|n|S|N"/>
   </scm:simpleType>
   <scm:simpleType id="C495" name="char16" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="16"/>
   </scm:simpleType>
   <scm:simpleType id="C496" base="#string" variety="atomic" primitiveType="#string">
      <scm:maxLength value="15"/>
      <scm:pattern value="variazione|annullamento"/>
   </scm:simpleType>
   <scm:complexType id="C497" name="PermessoSoggiorno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C498"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C499"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C500"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C501"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C502"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C503" name="enumMotivoIscrizioneAIRE"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:complexType id="C504" name="CivicoInterno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C505"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C506"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C507"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C508" name="enumTipoIstanza"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:complexType id="C509" name="StatoCivile"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C510"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C511"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C512" name="PiedeInterrogazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty"/>
   <scm:complexType id="C513" name="PraticaCancellazioneAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C514" base="C161" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
      <scm:pattern value="1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|21"/>
   </scm:simpleType>
   <scm:complexType id="C515" name="Residenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C516"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C517"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C518"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C519"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C520" name="tipoRispostaDecesso"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C521"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C522"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C523" name="tipoVedovanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C524"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C525"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C526"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C527" name="Ubicazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C528"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C529"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C530"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C531"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C532"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C533"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C534" name="StoricoResidenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C535"/>
   </scm:complexType>
   <scm:simpleType id="C536" base="C327" variety="atomic" primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="10"/>
      <scm:pattern value="PERMESSO|CARTA"/>
   </scm:simpleType>
   <scm:simpleType id="C537" name="charMax12"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="12"/>
   </scm:simpleType>
   <scm:complexType id="C538" name="DatiNascita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C539"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C540"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C161" name="numericoMax2"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
   </scm:simpleType>
   <scm:simpleType id="C541" name="tipoCodiceFiscale"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="16"/>
      <scm:pattern value="([A-Z0-9])*"/>
   </scm:simpleType>
   <scm:complexType id="C542" name="LibrettoSanitario"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C543"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C544"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C545"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C546" name="char2" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="2"/>
   </scm:simpleType>
   <scm:complexType id="C547" name="UbicazioneEstera"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C548"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C549"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C550"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C551"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C552" name="tipoStatoCivile"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:pattern value="1|2|3|4|5"/>
   </scm:simpleType>
   <scm:complexType id="C553" name="TestataInterrogazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C554"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C555"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C556"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C557" name="LibrettoDiLavoro"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C558"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C559"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C560"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C561" name="charMax5"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="5"/>
   </scm:simpleType>
   <scm:complexType id="C562" name="tipoRichiestaSAIA"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C563"/>
      <scm:attributeUse required="true" ref="C564"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C565"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C566"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C567"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C568"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C569"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C570" name="enumCodiceParentela"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C161"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
      <scm:enumeration value="28"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="29"/>
      <scm:enumeration value="27"/>
      <scm:enumeration value="17"/>
      <scm:enumeration value="14"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="6"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="24"/>
      <scm:enumeration value="16"/>
      <scm:enumeration value="15"/>
      <scm:enumeration value="11"/>
      <scm:enumeration value="4"/>
      <scm:enumeration value="23"/>
      <scm:enumeration value="12"/>
      <scm:enumeration value="30"/>
      <scm:enumeration value="7"/>
      <scm:enumeration value="18"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="20"/>
      <scm:enumeration value="31"/>
      <scm:enumeration value="25"/>
      <scm:enumeration value="21"/>
      <scm:enumeration value="10"/>
      <scm:enumeration value="13"/>
      <scm:enumeration value="26"/>
      <scm:enumeration value="22"/>
      <scm:enumeration value="9"/>
      <scm:enumeration value="19"/>
      <scm:enumeration value="8"/>
   </scm:simpleType>
   <scm:complexType id="C571" name="tipoElencoCodiciFiscaliConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C572"/>
   </scm:complexType>
   <scm:complexType id="C573" name="tipoStoricoResidenze"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C574"/>
   </scm:complexType>
   <scm:simpleType id="C575" name="enumTipoConfermaCancellazione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:complexType id="C576" name="PraticaCancellazioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C577"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C578"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C579"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C580"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C581"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C582"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C583" name="tipoElencoCodiciFiscali"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C584"/>
   </scm:complexType>
   <scm:simpleType id="C585" name="numerico5"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="5"/>
   </scm:simpleType>
   <scm:complexType id="C586" name="Cittadino"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C587"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C588"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C589"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C590"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C591"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C592"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C593"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C594"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C595"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C596"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C597"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C598"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C599" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2|3|4|5|6|7"/>
   </scm:simpleType>
   <scm:complexType id="C600" name="tipoAssegnazioneCodiceFiscale"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C601"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C602"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C603"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C604" name="charMax30"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="30"/>
   </scm:simpleType>
   <scm:complexType id="C605" name="PiedeRisposta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C606"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C607"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C608"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C609"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C610"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C611" name="charMax100"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="100"/>
   </scm:simpleType>
   <scm:simpleType id="C612" name="enumStatoDocumento"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C494"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="2"/>
      <scm:enumeration value="PE"/>
      <scm:enumeration value="PI"/>
      <scm:enumeration value="N"/>
      <scm:enumeration value="CE"/>
   </scm:simpleType>
   <scm:complexType id="C613" name="NumeroCivico"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C614"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C615"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C616"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C617"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C618" name="numericoMax20"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:complexType id="C619" name="ResponsabileAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C96"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C620"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C621"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C622" name="ConfermaDatiAnagrafici"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C623"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C624"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C625"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C626" name="enumTitoloDiStudio"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="7"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="6"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:simpleType id="C627" name="charMaxNumerico"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="([0-9])*"/>
   </scm:simpleType>
   <scm:complexType id="C628" name="Professione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C629"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C630"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C631"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C632"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C633"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C634"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C635" name="Matrimonio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C636"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C637"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C638"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C639"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C640"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C641"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C642"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C643" name="enumMotivoCancellazioneAIRE"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:complexType id="C644" name="tipoTesseraSanitaria"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C645"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C646"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C647"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C648" name="tipoConfermaDatiAnagrafici"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C649"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C650"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C651"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C652" name="TestataRisposta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C653"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C654"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C655"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C656" base="C327" variety="atomic" primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="10"/>
      <scm:pattern value="PERMESSO|CARTA"/>
   </scm:simpleType>
   <scm:complexType id="C657" name="tipoVariazionePaternitaMaternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C658"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C659"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C660"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C661" name="tipoSinonimoPersoneFisicheConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C662"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C663"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C664"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C665"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C666"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C667" name="tipoTesseraSanitariaConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C668"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C669"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C670"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C263" name="DocumentoIndividuale"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C264"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C265"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C266"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C267"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C671" name="PresaInCarico"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C672"/>
      <scm:attributeUse required="false" ref="C673" default="0"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C674"/>
   </scm:complexType>
   <scm:simpleType id="C675" name="numericoMax12"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="12"/>
   </scm:simpleType>
   <scm:simpleType id="C494" name="charMax2"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="2"/>
   </scm:simpleType>
   <scm:complexType id="C676" name="tipoRichiesta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C677"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C678"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C679"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C680"/>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C681"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C682" name="enumStatoCivile"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:complexType id="C683" name="tipoMaternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C684"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C685"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C686"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C687" name="tipoCartaIdentita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C688"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C689"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C690"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C691"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C692" base="C494" variety="atomic" primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="2"/>
      <scm:pattern value="A|B|C|D|AS|BS|CS|DS|BE|CE|DE"/>
   </scm:simpleType>
   <scm:complexType id="C693" name="tipoIscrizioneAPR"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C694"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C695"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C696"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C697"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C698"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C699"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C700"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C701"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C702" name="tipoPaternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C703"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C704"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C705"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C706" name="DatiMorte"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C707"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C708"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C709" name="tipoElencoSinonimiPersoneFisiche"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C710"/>
   </scm:complexType>
   <scm:complexType id="C711" name="Sentenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C712"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C713"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C714"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C715" name="alfabetico3"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C48"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="([a-zA-Z])*"/>
      <scm:length value="3"/>
   </scm:simpleType>
   <scm:complexType id="C716" name="tipoPatenti"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C717"/>
   </scm:complexType>
   <scm:complexType id="C718" name="Localita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C719"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C720"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C721"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C722" name="tipoCittadinanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C723"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C724"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C725"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C726" name="charMax14"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="14"/>
   </scm:simpleType>
   <scm:simpleType id="C727" name="charMax40"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="40"/>
   </scm:simpleType>
   <scm:simpleType id="C728" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="0|1"/>
   </scm:simpleType>
   <scm:complexType id="C729" name="Ufficio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C730"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C731"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C732" name="tipoToponimo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C733"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C734"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C735" name="char4" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="4"/>
   </scm:simpleType>
   <scm:simpleType id="C736" name="enumMotivoConsultazioneAnagrafica"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:complexType id="C737" name="tipoMatrimonio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C738"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C739"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C740"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C741" name="tipoSesso"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="M|F|m|f"/>
   </scm:simpleType>
   <scm:complexType id="C742" name="tipoDatiNascitaConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C743"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C744"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C745" name="tipoVariazionePermessoSoggiorno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C746"/>
   </scm:complexType>
   <scm:complexType id="C747" name="tipoUbicazioneEsteraConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C748"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C749"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C750"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C751"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C752" name="enumSesso"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C741"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="M|F|m|f"/>
      <scm:enumeration value="M"/>
      <scm:enumeration value="m"/>
      <scm:enumeration value="F"/>
      <scm:enumeration value="f"/>
   </scm:simpleType>
   <scm:complexType id="C753" name="tipoUtente"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C754"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C755"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C756" name="IdentificativoPersona"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C757"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C758"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C759" name="Indirizzo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C760"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C761"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C762"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C763"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C764"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C765"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C766" name="richiestaSAIA"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C562"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C335" name="nuovoNome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C333"
                nillable="false"
                abstract="false"/>
   <scm:element id="C719" name="descrizioneLocalita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C718"
                nillable="false"
                abstract="false"/>
   <scm:element id="C602" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C600"
                nillable="false"
                abstract="false"/>
   <scm:element id="C582" name="provenienza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C767"
                global="false"
                containingComplexType="C576"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C95" name="PersonaDatiNascita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C96"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C100"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C383" name="trascrizioneAttoNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C380"
                nillable="false"
                abstract="false"/>
   <scm:element id="C763" name="toponimo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C251"
                global="false"
                containingComplexType="C759"
                nillable="false"
                abstract="false"/>
   <scm:element id="C713" name="numero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C618"
                global="false"
                containingComplexType="C711"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C228" name="tipoResidenzaEstera"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C229"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C230"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C231"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C232"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C221" name="numerico11"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="11"/>
   </scm:simpleType>
   <scm:element id="C184" name="localita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C329"
                global="false"
                containingComplexType="C181"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C96" name="Persona"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C388" name="causale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C452"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C109" name="dataDecorrenzaAppartenenzaFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C106"
                nillable="false"
                abstract="false"/>
   <scm:element id="C341" name="numeroPraticaCancellazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C337"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C119" name="enumProvvedimentoCancellazione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:element id="C645" name="identificativo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C644"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C471" name="tipoVariazioneCittadinanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C472"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C473"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C623" name="ufficio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C729"
                global="false"
                containingComplexType="C622"
                nillable="false"
                abstract="false"/>
   <scm:element id="C294" name="nome" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C418" name="numerico3"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="3"/>
   </scm:simpleType>
   <scm:complexType id="C237" name="Emigrazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C238"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C239"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C240"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C241"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C707" name="dataMorte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C706"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C488" name="tipoConfermaDatiAnagraficiConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C489"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C490"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C491"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C151" name="numeroFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C726"
                global="false"
                containingComplexType="C148"
                nillable="false"
                abstract="false"/>
   <scm:element id="C368" name="dataDecorrenzaAppartenenzaFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C365"
                nillable="false"
                abstract="false"/>
   <scm:element id="C589" name="matrimonio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C635"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C19" name="dataOraRichiesta" type="#dateTime" global="false"
                  containingComplexType="C18"/>
   <scm:complexType id="C709" name="tipoElencoSinonimiPersoneFisiche"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C710"/>
   </scm:complexType>
   <scm:element id="C203" name="dataInizio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:element id="C37" name="nomeCampo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#string"
                global="false"
                containingComplexType="C36"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C483" name="tipoVariazioneNomeCognome"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C484"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C485"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C486"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C164" name="comune"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C162"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C317" name="tipoResidenzaEsteraConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C318"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C319"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C320"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C321"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C708" name="luogoMorte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C162"
                global="false"
                containingComplexType="C706"
                nillable="false"
                abstract="false"/>
   <scm:element id="C24" name="nomeMadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C22"
                nillable="false"
                abstract="false"/>
   <scm:element id="C74" name="frazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C327"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:element id="C456" name="protocolloComunicazioneAnagrafica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C92"
                global="false"
                containingComplexType="C452"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C627" name="charMaxNumerico"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="([0-9])*"/>
   </scm:simpleType>
   <scm:element id="C16" name="luogoNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C278"
                global="false"
                containingComplexType="C15"
                nillable="false"
                abstract="false"/>
   <scm:element id="C279" name="luogoEccezionale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C81"
                global="false"
                containingComplexType="C278"
                nillable="false"
                abstract="false"/>
   <scm:element id="C405" name="variazioneCittadinanza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C471"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C338" name="motivoCancellazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C2"
                global="false"
                containingComplexType="C337"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C409" name="Pensione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C410"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C411"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C412"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C101" name="tipoResidenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C102"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C103"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C104"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C105"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C353" name="numeroCivico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C322"
                global="false"
                containingComplexType="C348"
                nillable="false"
                abstract="false"/>
   <scm:element id="C369" name="codiceRelazioneParentela"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C570"
                global="false"
                containingComplexType="C365"
                nillable="false"
                abstract="false"/>
   <scm:element id="C700" name="flagResidenteDallaNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C1"
                global="false"
                containingComplexType="C693"
                nillable="false"
                abstract="false"/>
   <scm:element id="C358" name="corte" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C357"
                nillable="false"
                abstract="false"/>
   <scm:element id="C431" name="messaggio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C442"
                global="false"
                containingComplexType="C428"
                nillable="false"
                abstract="false"/>
   <scm:element id="C754" name="login" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C753"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C140" name="tipoUbicazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C141"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C142"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C143"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C144"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C145"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C146"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C11" name="pratica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C47"
                global="false"
                containingComplexType="C8"
                nillable="false"
                abstract="false"/>
   <scm:element id="C321" name="fonteDato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C317"
                nillable="false"
                abstract="false"/>
   <scm:element id="C311" name="numeroPatente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C91"
                global="false"
                containingComplexType="C310"
                nillable="false"
                abstract="false"/>
   <scm:element id="C350" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C348"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C271" name="charMax1024"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="1024"/>
   </scm:simpleType>
   <scm:complexType id="C600" name="tipoAssegnazioneCodiceFiscale"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C601"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C602"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C603"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C291" name="tipoGeneralita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C292"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C293"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C294"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C295"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C296"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C297"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C298"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C299"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C300"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C301"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C302"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C392" name="vedovanza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C523"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C386" name="protocolloAnagrafe" type="C328" global="false"
                  containingComplexType="C385"/>
   <scm:simpleType id="C604" name="charMax30"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="30"/>
   </scm:simpleType>
   <scm:element id="C155" name="specie" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C154"
                nillable="false"
                abstract="false"/>
   <scm:element id="C308" name="siglaProvinciaIstat"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C546"
                global="false"
                containingComplexType="C305"
                nillable="false"
                abstract="false"/>
   <scm:element id="C97" name="cognome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C96"
                nillable="false"
                abstract="false"/>
   <scm:element id="C477" name="dataRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C474"
                nillable="false"
                abstract="false"/>
   <scm:element id="C204" name="dataFine"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:element id="C274" name="protocolloSAIA"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C92"
                global="false"
                containingComplexType="C272"
                nillable="false"
                abstract="false"/>
   <scm:element id="C424" name="corte" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C423"
                nillable="false"
                abstract="false"/>
   <scm:element id="C666" name="residenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C255"
                global="false"
                containingComplexType="C661"
                nillable="false"
                abstract="false"/>
   <scm:element id="C567" name="applicazioneAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C467"
                global="false"
                containingComplexType="C562"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C563" name="versione" type="C90" global="false"
                  containingComplexType="C562"/>
   <scm:element id="C27" name="tipoIscrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C246"
                global="false"
                containingComplexType="C26"
                nillable="false"
                abstract="false"/>
   <scm:element id="C588" name="nascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C380"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:element id="C394" name="iscrizioneAPR"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C693"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C179" name="statoCivile"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C682"
                global="false"
                containingComplexType="C177"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C656" base="C327" variety="atomic" primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="10"/>
      <scm:pattern value="PERMESSO|CARTA"/>
   </scm:simpleType>
   <scm:element id="C276" name="comuneOrigine"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C305"
                global="false"
                containingComplexType="C272"
                nillable="false"
                abstract="false"/>
   <scm:element id="C320" name="dataFine"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C317"
                nillable="false"
                abstract="false"/>
   <scm:element id="C760" name="cap"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C585"
                global="false"
                containingComplexType="C759"
                nillable="false"
                abstract="false"/>
   <scm:element id="C98" name="nome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C96"
                nillable="false"
                abstract="false"/>
   <scm:element id="C624" name="comune"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C622"
                nillable="false"
                abstract="false"/>
   <scm:element id="C146" name="altro" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C140"
                nillable="false"
                abstract="false"/>
   <scm:element id="C206" name="codiceStatoEstero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C214"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C118" name="numericoMax6"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="6"/>
   </scm:simpleType>
   <scm:element id="C149" name="annoIscrizioneAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C93"
                global="false"
                containingComplexType="C148"
                nillable="false"
                abstract="false"/>
   <scm:element id="C764" name="numeroCivico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C613"
                global="false"
                containingComplexType="C759"
                nillable="false"
                abstract="false"/>
   <scm:element id="C554" name="comuneRichiedente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C305"
                global="false"
                containingComplexType="C553"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C47" name="PraticaIscrizioneAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C464" name="motivo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C643"
                global="false"
                containingComplexType="C461"
                nillable="false"
                abstract="false"/>
   <scm:element id="C559" name="dataRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C557"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C115" name="tipoCessazioneAnnullamentoMatrimonio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C116"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C117"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C107" name="codiceFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C221"
                global="false"
                containingComplexType="C106"
                nillable="false"
                abstract="false"/>
   <scm:element id="C535" name="residenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C768"
                global="false"
                containingComplexType="C534"
                nillable="false"
                abstract="false"/>
   <scm:element id="C551" name="altro"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C271"
                global="false"
                containingComplexType="C547"
                nillable="false"
                abstract="false"/>
   <scm:element id="C425" name="scala" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C423"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C576" name="PraticaCancellazioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C577"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C578"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C579"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C580"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C581"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C582"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C468" name="nome" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C467"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C442" name="Messaggio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C443"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C444"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C445"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C579" name="comportaConteggio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C215"
                global="false"
                containingComplexType="C576"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C3" name="numerico1"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
   </scm:simpleType>
   <scm:element id="C235" name="nomeConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C233"
                nillable="false"
                abstract="false"/>
   <scm:element id="C659" name="nuovaMaternita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C683"
                global="false"
                containingComplexType="C657"
                nillable="false"
                abstract="false"/>
   <scm:element id="C630" name="posizioneNellaProfessione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C3"
                global="false"
                containingComplexType="C628"
                nillable="false"
                abstract="false"/>
   <scm:element id="C99" name="sesso"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C752"
                global="false"
                containingComplexType="C96"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C49" name="enumGruppoLinguistico"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:element id="C765" name="dataDecorrenzaIndirizzo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C759"
                nillable="false"
                abstract="false"/>
   <scm:element id="C593" name="professione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C628"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:element id="C126" name="praticaEmigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C4"
                global="false"
                containingComplexType="C125"
                nillable="false"
                abstract="false"/>
   <scm:element id="C249" name="luogoMorte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C278"
                global="false"
                containingComplexType="C247"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C255" name="tipoResidenzaConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C256"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C257"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C258"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C259"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C555" name="comuneDestinatario"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C305"
                global="false"
                containingComplexType="C553"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C553" name="TestataInterrogazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C554"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C555"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C556"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C428" name="Comunicazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C429"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C430"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C431"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C50" name="tipoNumeroCivicoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C51"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C52"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C53"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C54"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C734" name="denominazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C611"
                global="false"
                containingComplexType="C732"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C8" name="IscrizioneAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C9"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C10"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C11"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C12"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C13"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C14"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C450" name="descrizioneStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C448"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C628" name="Professione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C629"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C630"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C631"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C632"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C633"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C634"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C236" name="codiceFiscaleConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C541"
                global="false"
                containingComplexType="C233"
                nillable="false"
                abstract="false"/>
   <scm:element id="C295" name="paternita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C702"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C675" name="numericoMax12"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="12"/>
   </scm:simpleType>
   <scm:element id="C592" name="gruppoLinguistico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C49"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:element id="C267" name="comuneRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C263"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C310" name="tipoPatente"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C311"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C312"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C313"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C314"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C315"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C316"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C532" name="distretto"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C735"
                global="false"
                containingComplexType="C527"
                nillable="false"
                abstract="false"/>
   <scm:element id="C225" name="nome" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C222"
                nillable="false"
                abstract="false"/>
   <scm:element id="C744" name="dataNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C742"
                nillable="false"
                abstract="false"/>
   <scm:element id="C202" name="ubicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C547"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:element id="C264" name="numero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C91"
                global="false"
                containingComplexType="C263"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C692" base="C494" variety="atomic" primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="2"/>
      <scm:pattern value="A|B|C|D|AS|BS|CS|DS|BE|CE|DE"/>
   </scm:simpleType>
   <scm:element id="C397" name="permessoSoggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C474"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C199" name="codiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C541"
                global="false"
                containingComplexType="C198"
                nillable="false"
                abstract="false"/>
   <scm:element id="C241" name="residenzaEstero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C201"
                global="false"
                containingComplexType="C237"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C33" name="tipoUfficio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C34"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C35"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C283" name="cognomePadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C282"
                nillable="false"
                abstract="false"/>
   <scm:element id="C521" name="dataDecesso"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C520"
                nillable="false"
                abstract="false"/>
   <scm:element id="C404" name="variazioneSesso"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C333"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C660" name="dataVariazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C657"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C438" name="tipo" type="C64" global="false" containingComplexType="C437"/>
   <scm:element id="C334" name="nuovoSesso"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C741"
                global="false"
                containingComplexType="C333"
                nillable="false"
                abstract="false"/>
   <scm:element id="C686" name="codiceFiscaleMadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C683"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C210" name="tipoSentenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C211"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C212"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C213"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C373" name="descrizioneStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C372"
                nillable="false"
                abstract="false"/>
   <scm:element id="C712" name="data"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C711"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C357" name="tipoCivicoInterno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C358"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C359"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C360"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C301" name="cittadinanza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C722"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C443" name="protocolloAnagrafe" type="C328" global="false"
                  containingComplexType="C442"/>
   <scm:simpleType id="C161" name="numericoMax2"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
   </scm:simpleType>
   <scm:element id="C17" name="dataNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C15"
                nillable="false"
                abstract="false"/>
   <scm:element id="C273" name="motivoConsultazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C736"
                global="false"
                containingComplexType="C272"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C513" name="PraticaCancellazioneAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C220" name="siglaProvinciaIstat"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C217"
                nillable="false"
                abstract="false"/>
   <scm:element id="C581" name="tipoIstanza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C508"
                global="false"
                containingComplexType="C576"
                nillable="false"
                abstract="false"/>
   <scm:element id="C87" name="datiNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C15"
                global="false"
                containingComplexType="C83"
                nillable="false"
                abstract="false"/>
   <scm:element id="C498" name="numeroSoggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C91"
                global="false"
                containingComplexType="C497"
                nillable="false"
                abstract="false"/>
   <scm:element id="C556" name="responsabileAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C619"
                global="false"
                containingComplexType="C553"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C512" name="PiedeInterrogazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="empty"/>
   <scm:simpleType id="C81" name="charMax120"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="120"/>
   </scm:simpleType>
   <scm:element id="C603" name="dataAssegnazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C600"
                nillable="false"
                abstract="false"/>
   <scm:element id="C566" name="utente" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C753"
                global="false"
                containingComplexType="C562"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C756" name="IdentificativoPersona"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C757"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C758"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C53" name="colore" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C460"
                global="false"
                containingComplexType="C50"
                nillable="false"
                abstract="false"/>
   <scm:element id="C347" name="protocolloComunicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C92"
                global="false"
                containingComplexType="C344"
                nillable="false"
                abstract="false"/>
   <scm:element id="C714" name="tribunale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C711"
                nillable="false"
                abstract="false"/>
   <scm:element id="C128" name="codiceStatoEstero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C214"
                global="false"
                containingComplexType="C125"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C622" name="ConfermaDatiAnagrafici"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C623"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C624"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C625"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C57" name="attoMorte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C56"
                nillable="false"
                abstract="false"/>
   <scm:element id="C638" name="coniuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C96"
                global="false"
                containingComplexType="C635"
                nillable="false"
                abstract="false"/>
   <scm:element id="C14" name="motivo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C503"
                global="false"
                containingComplexType="C8"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C413" name="Richiesta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C414"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C415"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C416"/>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C417"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C132" name="attoMorteConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C129"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C683" name="tipoMaternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C684"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C685"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C686"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C644" name="tipoTesseraSanitaria"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C645"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C646"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C647"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:attribute id="C387" name="dataOraRichiesta" type="#dateTime" global="false"
                  containingComplexType="C385"/>
   <scm:complexType id="C79" name="CorpoRisposta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C80"/>
   </scm:complexType>
   <scm:simpleType id="C370" name="charMax3"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="3"/>
   </scm:simpleType>
   <scm:element id="C195" name="frazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C327"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:element id="C730" name="codice"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C91"
                global="false"
                containingComplexType="C729"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C643" name="enumMotivoCancellazioneAIRE"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:element id="C85" name="cognome" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C83"
                nillable="false"
                abstract="false"/>
   <scm:element id="C399" name="morte" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C247"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C680" name="presaInCarico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C344"
                global="false"
                containingComplexType="C676"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C64" name="tipoEnteConsultabile"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="INA|AE"/>
   </scm:simpleType>
   <scm:simpleType id="C503" name="enumMotivoIscrizioneAIRE"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:complexType id="C154" name="tipoToponimoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C155"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C156"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C76" name="numeroRegistro"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C561"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:element id="C300" name="sesso" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C741"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C329" name="tipoLocalita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C330"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C331"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C332"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C222" name="tipoSinonimoPersoneFisiche"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C223"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C224"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C225"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C226"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C227"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C722" name="tipoCittadinanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C723"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C724"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C725"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C732" name="tipoToponimo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C733"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C734"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C720" name="descrizioneStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C718"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C365" name="FamigliaAnagrafica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C366"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C367"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C368"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C369"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C243" name="luogoEccezionale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C81"
                global="false"
                containingComplexType="C242"
                nillable="false"
                abstract="false"/>
   <scm:element id="C336" name="dataVariazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C333"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C514" base="C161" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
      <scm:pattern value="1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|21"/>
   </scm:simpleType>
   <scm:element id="C408" name="datiRettificati"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C446"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C757" name="codiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C541"
                global="false"
                containingComplexType="C756"
                nillable="false"
                abstract="false"/>
   <scm:element id="C70" name="ufficio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C82"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:element id="C396" name="cartaIdentita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C687"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C71" name="parte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C3"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C2" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2|3|4|5|6|7|8"/>
   </scm:simpleType>
   <scm:element id="C615" name="lettera"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C82"
                global="false"
                containingComplexType="C613"
                nillable="false"
                abstract="false"/>
   <scm:element id="C244" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C217"
                global="false"
                containingComplexType="C242"
                nillable="false"
                abstract="false"/>
   <scm:element id="C688" name="numeroCI"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C91"
                global="false"
                containingComplexType="C687"
                nillable="false"
                abstract="false"/>
   <scm:element id="C44" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C217"
                global="false"
                containingComplexType="C40"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C570" name="enumCodiceParentela"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C161"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
      <scm:enumeration value="28"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="29"/>
      <scm:enumeration value="27"/>
      <scm:enumeration value="17"/>
      <scm:enumeration value="14"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="6"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="24"/>
      <scm:enumeration value="16"/>
      <scm:enumeration value="15"/>
      <scm:enumeration value="11"/>
      <scm:enumeration value="4"/>
      <scm:enumeration value="23"/>
      <scm:enumeration value="12"/>
      <scm:enumeration value="30"/>
      <scm:enumeration value="7"/>
      <scm:enumeration value="18"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="20"/>
      <scm:enumeration value="31"/>
      <scm:enumeration value="25"/>
      <scm:enumeration value="21"/>
      <scm:enumeration value="10"/>
      <scm:enumeration value="13"/>
      <scm:enumeration value="26"/>
      <scm:enumeration value="22"/>
      <scm:enumeration value="9"/>
      <scm:enumeration value="19"/>
      <scm:enumeration value="8"/>
   </scm:simpleType>
   <scm:simpleType id="C460" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2"/>
   </scm:simpleType>
   <scm:element id="C45" name="distretto"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C40"
                nillable="false"
                abstract="false"/>
   <scm:element id="C723" name="descrizioneStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C722"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C509" name="StatoCivile"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C510"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C511"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C620" name="telefono"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#string"
                global="false"
                containingComplexType="C619"
                nillable="false"
                abstract="false"/>
   <scm:element id="C526" name="attoMorteConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C188"
                global="false"
                containingComplexType="C523"
                nillable="false"
                abstract="false"/>
   <scm:element id="C102" name="ubicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C140"
                global="false"
                containingComplexType="C101"
                nillable="false"
                abstract="false"/>
   <scm:element id="C69" name="anno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C93"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C242" name="tipoLuogoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C243"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C244"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C245"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C297" name="luogoNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C278"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:element id="C234" name="cognomeConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C233"
                nillable="false"
                abstract="false"/>
   <scm:element id="C482" name="codiceFiscaleOperatore"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C480"
                nillable="false"
                abstract="false"/>
   <scm:element id="C596" name="emigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C237"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:element id="C307" name="codiceComuneIstat"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C254"
                global="false"
                containingComplexType="C305"
                nillable="false"
                abstract="false"/>
   <scm:element id="C314" name="dataRilascioPatente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C310"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C504" name="CivicoInterno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C505"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C506"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C507"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C285" name="codiceFiscalePadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C541"
                global="false"
                containingComplexType="C282"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C497" name="PermessoSoggiorno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C498"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C499"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C500"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C501"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C502"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C112" name="sinonimoPersoneFisiche"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C661"
                global="false"
                containingComplexType="C111"
                nillable="false"
                abstract="false"/>
   <scm:element id="C240" name="cancellazioneAIRE"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C461"
                global="false"
                containingComplexType="C237"
                nillable="false"
                abstract="false"/>
   <scm:element id="C641" name="attoMatrimonio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C635"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C207" name="tipoRispostaDecessoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C208"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C209"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C501" name="dataScadenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C497"
                nillable="false"
                abstract="false"/>
   <scm:element id="C261" name="residenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C317"
                global="false"
                containingComplexType="C260"
                nillable="false"
                abstract="false"/>
   <scm:element id="C367" name="dataOrigineFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C365"
                nillable="false"
                abstract="false"/>
   <scm:element id="C669" name="dataScadenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C667"
                nillable="false"
                abstract="false"/>
   <scm:element id="C25" name="codiceFiscaleMadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C541"
                global="false"
                containingComplexType="C22"
                nillable="false"
                abstract="false"/>
   <scm:element id="C410" name="enteErogatore"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C113"
                global="false"
                containingComplexType="C409"
                nillable="false"
                abstract="false"/>
   <scm:element id="C248" name="dataMorte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C247"
                nillable="false"
                abstract="false"/>
   <scm:element id="C447" name="rettifica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C173"
                global="false"
                containingComplexType="C446"
                nillable="false"
                abstract="false"/>
   <scm:element id="C390" name="coniuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C60"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C323" name="numero" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C153"
                global="false"
                containingComplexType="C322"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C729" name="Ufficio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C730"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C731"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C470" name="fornitore"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C467"
                nillable="false"
                abstract="false"/>
   <scm:element id="C472" name="nuovaCittadinanza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C722"
                global="false"
                containingComplexType="C471"
                nillable="false"
                abstract="false"/>
   <scm:element id="C313" name="tipoPatente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C692"
                global="false"
                containingComplexType="C310"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C480" name="tipoMittente"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C481"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C482"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C745" name="tipoVariazionePermessoSoggiorno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C746"/>
   </scm:complexType>
   <scm:complexType id="C36" name="Rettifica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C37"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C38"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C39"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C721" name="codiceStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C214"
                global="false"
                containingComplexType="C718"
                nillable="false"
                abstract="false"/>
   <scm:element id="C568" name="richiesta"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C676"
                global="false"
                containingComplexType="C562"
                nillable="false"
                abstract="false"/>
   <scm:element id="C62" name="nomeConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C60"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C22" name="Maternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C23"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C24"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C25"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C610" name="residenzaNelComune"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C515"
                global="false"
                containingComplexType="C605"
                nillable="false"
                abstract="false"/>
   <scm:element id="C31" name="motivoIscrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C727"
                global="false"
                containingComplexType="C26"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C166" name="tipoStoricoResidenzeConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C167"/>
   </scm:complexType>
   <scm:complexType id="C15" name="tipoDatiNascita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C16"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C17"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C82" name="charMax4"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="4"/>
   </scm:simpleType>
   <scm:complexType id="C767" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C769"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C770"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C259" name="fonteDato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C255"
                nillable="false"
                abstract="false"/>
   <scm:element id="C646" name="dataScadenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C644"
                nillable="false"
                abstract="false"/>
   <scm:element id="C617" name="civicoInterno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C504"
                global="false"
                containingComplexType="C613"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C520" name="tipoRispostaDecesso"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C521"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C522"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C398" name="cancellazioneAPR"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C337"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C192" name="parte" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C3"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C432" name="StatoCode"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C433"/>
      <scm:attributeUse required="false" ref="C434" default="0"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C435"/>
   </scm:complexType>
   <scm:element id="C770" name="codiceISTATComuneEmigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C254"
                global="false"
                containingComplexType="C767"
                nillable="false"
                abstract="false"/>
   <scm:element id="C342" name="dataDefinizionePraticaCancellazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C337"
                nillable="false"
                abstract="false"/>
   <scm:element id="C519" name="fonteDato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C271"
                global="false"
                containingComplexType="C515"
                nillable="false"
                abstract="false"/>
   <scm:element id="C506" name="scala"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C82"
                global="false"
                containingComplexType="C504"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C201" name="ResidenzaEstera"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C202"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C203"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C204"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C205"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C206"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C546" name="char2" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="2"/>
   </scm:simpleType>
   <scm:element id="C167" name="residenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C255"
                global="false"
                containingComplexType="C166"
                nillable="false"
                abstract="false"/>
   <scm:element id="C406" name="variazionePermessoSoggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C745"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C423" name="tipoCivicoInternoConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C424"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C425"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C426"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C211" name="data" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:element id="C402" name="famigliaAnagrafica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C106"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C260" name="tipoStoricoResidenzeEstereConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C261"/>
   </scm:complexType>
   <scm:element id="C354" name="dataDecorrenzaIndirizzo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C348"
                nillable="false"
                abstract="false"/>
   <scm:element id="C121" name="nomeComune"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C120"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C345" name="dataOraRichiesta" type="#dateTime" global="false"
                  containingComplexType="C344"/>
   <scm:element id="C38" name="vecchioValore"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#string"
                global="false"
                containingComplexType="C36"
                nillable="false"
                abstract="false"/>
   <scm:element id="C633" name="titoloDiStudio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C626"
                global="false"
                containingComplexType="C628"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C114" name="enumTipoRelazioneFamiliare"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:complexType id="C148" name="tipoAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C149"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C150"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C151"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C152"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C32" name="provenienza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C771"
                global="false"
                containingComplexType="C26"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C626" name="enumTitoloDiStudio"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="7"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="6"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:element id="C684" name="cognomeMadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C683"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C768" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C772"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C773"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C773" name="residenzaEstero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C201"
                global="false"
                containingComplexType="C768"
                nillable="false"
                abstract="false"/>
   <scm:element id="C761" name="comune"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C759"
                nillable="false"
                abstract="false"/>
   <scm:element id="C698" name="dataDefinizionePraticaIscrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C693"
                nillable="false"
                abstract="false"/>
   <scm:element id="C510" name="codiceStatoCivile"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C3"
                global="false"
                containingComplexType="C509"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C355" name="tipoVeicoli"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C356"/>
   </scm:complexType>
   <scm:element id="C769" name="codiceISTATStatoEsteroEmigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C254"
                global="false"
                containingComplexType="C767"
                nillable="false"
                abstract="false"/>
   <scm:element id="C560" name="comuneRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C557"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C687" name="tipoCartaIdentita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C688"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C689"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C690"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C691"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C28" name="comportaConteggio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C215"
                global="false"
                containingComplexType="C26"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C247" name="tipoMorte"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C248"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C249"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C250"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C170" name="tipo" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C91"
                global="false"
                containingComplexType="C169"
                nillable="false"
                abstract="false"/>
   <scm:element id="C42" name="numeroCivico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C50"
                global="false"
                containingComplexType="C40"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C327" name="charMax10"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="10"/>
   </scm:simpleType>
   <scm:element id="C484" name="nuovoCognome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C483"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C26" name="PraticaIscrizioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C27"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C28"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C29"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C30"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C31"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C32"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C727" name="charMax40"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="40"/>
   </scm:simpleType>
   <scm:simpleType id="C0" name="enumRegimePatrimoniale"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C1"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
      <scm:enumeration value="S"/>
      <scm:enumeration value="C"/>
   </scm:simpleType>
   <scm:complexType id="C635" name="Matrimonio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C636"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C637"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C638"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C639"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C640"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C641"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C642"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C257" name="dataInizio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C255"
                nillable="false"
                abstract="false"/>
   <scm:element id="C330" name="descrizioneLocalita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C329"
                nillable="false"
                abstract="false"/>
   <scm:element id="C485" name="nuovoNome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C483"
                nillable="false"
                abstract="false"/>
   <scm:element id="C411" name="categoriaPensione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C370"
                global="false"
                containingComplexType="C409"
                nillable="false"
                abstract="false"/>
   <scm:element id="C78" name="note"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C160" name="enumMotivoRestituzioneSenzaProvvedimento"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C161"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="7"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="6"/>
      <scm:enumeration value="10"/>
      <scm:enumeration value="5"/>
      <scm:enumeration value="9"/>
      <scm:enumeration value="4"/>
      <scm:enumeration value="8"/>
   </scm:simpleType>
   <scm:element id="C356" name="veicolo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C774"
                global="false"
                containingComplexType="C355"
                nillable="false"
                abstract="false"/>
   <scm:element id="C178" name="cittadinanza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C372"
                global="false"
                containingComplexType="C177"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C508" name="enumTipoIstanza"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:element id="C223" name="codiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C222"
                nillable="false"
                abstract="false"/>
   <scm:element id="C717" name="patente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C310"
                global="false"
                containingComplexType="C716"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C328" name="tipoProtocolloAnagrafe"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:complexType id="C65" name="PraticaIscrizioneCancellazioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C66"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C92" name="tipoProtocollo"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="20"/>
   </scm:simpleType>
   <scm:element id="C518" name="dataFine"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C515"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C303" name="CorpoInterrogazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C304"/>
   </scm:complexType>
   <scm:simpleType id="C585" name="numerico5"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="5"/>
   </scm:simpleType>
   <scm:element id="C105" name="fonteDato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C101"
                nillable="false"
                abstract="false"/>
   <scm:element id="C637" name="luogo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C162"
                global="false"
                containingComplexType="C635"
                nillable="false"
                abstract="false"/>
   <scm:element id="C280" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C278"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C735" name="char4" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="4"/>
   </scm:simpleType>
   <scm:element id="C587" name="generalita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C177"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:element id="C332" name="codiceStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C214"
                global="false"
                containingComplexType="C329"
                nillable="false"
                abstract="false"/>
   <scm:element id="C298" name="attoNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C188"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:element id="C459" name="cancellazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C461"
                global="false"
                containingComplexType="C457"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C771" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C775"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C776"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C156" name="denominazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C611"
                global="false"
                containingComplexType="C154"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C657" name="tipoVariazionePaternitaMaternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C658"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C659"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C660"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C318" name="ubicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C747"
                global="false"
                containingComplexType="C317"
                nillable="false"
                abstract="false"/>
   <scm:element id="C374" name="codiceStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C214"
                global="false"
                containingComplexType="C372"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C562" name="tipoRichiestaSAIA"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="true" ref="C563"/>
      <scm:attributeUse required="true" ref="C564"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C565"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C566"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C567"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C568"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C569"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C322" name="tipoNumeroCivico"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C323"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C324"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C325"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C326"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C23" name="cognomeMadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C22"
                nillable="false"
                abstract="false"/>
   <scm:element id="C725" name="dataValidita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C722"
                nillable="false"
                abstract="false"/>
   <scm:element id="C658" name="nuovaPaternita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C702"
                global="false"
                containingComplexType="C657"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C124" base="#string" variety="atomic" primitiveType="#string">
      <scm:pattern value="A|C|M|R"/>
   </scm:simpleType>
   <scm:element id="C289" name="attoDivorzio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C286"
                nillable="false"
                abstract="false"/>
   <scm:element id="C325" name="colore" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C309"
                global="false"
                containingComplexType="C322"
                nillable="false"
                abstract="false"/>
   <scm:element id="C142" name="numeroCivico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C322"
                global="false"
                containingComplexType="C140"
                nillable="false"
                abstract="false"/>
   <scm:element id="C43" name="cap" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C585"
                global="false"
                containingComplexType="C40"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C217" name="tipoComuneConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C218"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C219"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C220"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C391" name="matrimonio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C737"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C198" name="PersonaAnagrafica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C96"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C199"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C200"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C663" name="cognome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C661"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C18" name="tipoStatoCode"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C19"/>
      <scm:attributeUse required="false" ref="C20" default="0"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C21"/>
   </scm:complexType>
   <scm:element id="C172" name="testo" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C169"
                nillable="false"
                abstract="false"/>
   <scm:element id="C634" name="descrizioneDiplomaLaurea"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C727"
                global="false"
                containingComplexType="C628"
                nillable="false"
                abstract="false"/>
   <scm:element id="C39" name="dataRettifica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C36"
                nillable="false"
                abstract="false"/>
   <scm:element id="C136" name="descrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C134"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C537" name="charMax12"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="12"/>
   </scm:simpleType>
   <scm:element id="C304" name="oggettoConsultazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C756"
                global="false"
                containingComplexType="C303"
                nillable="false"
                abstract="false"/>
   <scm:element id="C574" name="residenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C101"
                global="false"
                containingComplexType="C573"
                nillable="false"
                abstract="false"/>
   <scm:element id="C517" name="dataInizio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C515"
                nillable="false"
                abstract="false"/>
   <scm:element id="C144" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C140"
                nillable="false"
                abstract="false"/>
   <scm:element id="C580" name="dataDecorrenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C576"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C496" base="#string" variety="atomic" primitiveType="#string">
      <scm:maxLength value="15"/>
      <scm:pattern value="variazione|annullamento"/>
   </scm:simpleType>
   <scm:element id="C505" name="corte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C82"
                global="false"
                containingComplexType="C504"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C466" name="charMax8"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="8"/>
   </scm:simpleType>
   <scm:element id="C163" name="luogoEccezionale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C81"
                global="false"
                containingComplexType="C162"
                nillable="false"
                abstract="false"/>
   <scm:element id="C9" name="comunePrecedenteIscrizioneAIRE"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#anyType"
                global="false"
                containingComplexType="C8"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C702" name="tipoPaternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C703"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C704"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C705"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C736" name="enumMotivoConsultazioneAnagrafica"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:complexType id="C448" name="tipoLocalitaConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C449"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C450"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C451"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C129" name="Vedovanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C130"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C131"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C132"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C133"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C61" name="cognomeConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C60"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C676" name="tipoRichiesta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C677"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C678"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C679"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C680"/>
            <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C681"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:attribute id="C673" name="ultimoId" type="#string" global="false"
                  containingComplexType="C671"/>
   <scm:simpleType id="C726" name="charMax14"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="14"/>
   </scm:simpleType>
   <scm:element id="C326" name="civicoInterno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C357"
                global="false"
                containingComplexType="C322"
                nillable="false"
                abstract="false"/>
   <scm:element id="C653" name="comuneRichiedente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C305"
                global="false"
                containingComplexType="C652"
                nillable="false"
                abstract="false"/>
   <scm:element id="C407" name="variazionePaternitaMaternita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C657"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C453" name="tipoComunicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C496"
                global="false"
                containingComplexType="C452"
                nillable="false"
                abstract="false"/>
   <scm:element id="C21" name="protocolloComunicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C92"
                global="false"
                containingComplexType="C18"
                nillable="false"
                abstract="false"/>
   <scm:element id="C208" name="dataDecesso"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C207"
                nillable="false"
                abstract="false"/>
   <scm:element id="C364" name="dataRevoca"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C361"
                nillable="false"
                abstract="false"/>
   <scm:element id="C694" name="motivoIscrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C599"
                global="false"
                containingComplexType="C693"
                nillable="false"
                abstract="false"/>
   <scm:element id="C229" name="ubicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C181"
                global="false"
                containingComplexType="C228"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C376" name="RispostaAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C377"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C378"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C379"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C748" name="toponimo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C154"
                global="false"
                containingComplexType="C747"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C113" name="charMax16"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="16"/>
   </scm:simpleType>
   <scm:complexType id="C263" name="DocumentoIndividuale"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C264"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C265"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C266"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C267"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C762" name="frazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C759"
                nillable="false"
                abstract="false"/>
   <scm:element id="C139" name="corpo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C303"
                global="false"
                containingComplexType="C137"
                nillable="false"
                abstract="false"/>
   <scm:element id="C584" name="codiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C583"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C682" name="enumStatoCivile"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:element id="C751" name="altro" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C747"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C747" name="tipoUbicazioneEsteraConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C748"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C749"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C750"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C751"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C385" name="tipoComGenerica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C386"/>
      <scm:attributeUse required="true" ref="C387"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C388"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C389"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C390"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C391"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C392"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C393"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C394"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C395"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C396"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C397"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C398"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C399"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C400"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C401"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C402"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C403"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C404"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C405"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C406"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C407"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C408"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C252" name="specie"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C604"
                global="false"
                containingComplexType="C251"
                nillable="false"
                abstract="false"/>
   <scm:element id="C403" name="variazioneNomeCognome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C483"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C175" name="vecchioValore"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C173"
                nillable="false"
                abstract="false"/>
   <scm:element id="C41" name="toponimo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C154"
                global="false"
                containingComplexType="C40"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C88" name="tipoConsultazionePerCodiceFiscale"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C89"/>
   </scm:complexType>
   <scm:complexType id="C583" name="tipoElencoCodiciFiscali"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C584"/>
   </scm:complexType>
   <scm:element id="C549" name="numeroCivico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C613"
                global="false"
                containingComplexType="C547"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C605" name="PiedeRisposta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C606"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C607"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C608"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C609"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C610"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C30" name="tipoIstanza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C508"
                global="false"
                containingComplexType="C26"
                nillable="false"
                abstract="false"/>
   <scm:element id="C381" name="maternita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C198"
                global="false"
                containingComplexType="C380"
                nillable="false"
                abstract="false"/>
   <scm:element id="C266" name="dataScadenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C263"
                nillable="false"
                abstract="false"/>
   <scm:element id="C108" name="dataOrigineFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C106"
                nillable="false"
                abstract="false"/>
   <scm:element id="C80" name="cittadino"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C586"
                global="false"
                containingComplexType="C79"
                nillable="false"
                abstract="false"/>
   <scm:element id="C703" name="cognomePadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C702"
                nillable="false"
                abstract="false"/>
   <scm:element id="C674" name="protocolloSAIA"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C92"
                global="false"
                containingComplexType="C671"
                nillable="false"
                abstract="false"/>
   <scm:element id="C545" name="comuneRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C542"
                nillable="false"
                abstract="false"/>
   <scm:element id="C191" name="ufficio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:element id="C550" name="localita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C718"
                global="false"
                containingComplexType="C547"
                nillable="false"
                abstract="false"/>
   <scm:element id="C77" name="data"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:element id="C268" name="espatrio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C493"
                global="false"
                containingComplexType="C262"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C372" name="Cittadinanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C373"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C374"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C375"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C478" name="dataScadenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C474"
                nillable="false"
                abstract="false"/>
   <scm:element id="C349" name="cap" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C585"
                global="false"
                containingComplexType="C348"
                nillable="false"
                abstract="false"/>
   <scm:element id="C749" name="numeroCivico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C50"
                global="false"
                containingComplexType="C747"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C677" name="versione" type="C90" global="false"
                  containingComplexType="C676"/>
   <scm:element id="C632" name="condizioneNonProfessionale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C161"
                global="false"
                containingComplexType="C628"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C434" name="ultimoId" type="#string" global="false"
                  containingComplexType="C432"/>
   <scm:element id="C668" name="identificativo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C667"
                nillable="false"
                abstract="false"/>
   <scm:element id="C66" name="tipoPratica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C59"
                global="false"
                containingComplexType="C65"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C575" name="enumTipoConfermaCancellazione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:element id="C469" name="versione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C90"
                global="false"
                containingComplexType="C467"
                nillable="false"
                abstract="false"/>
   <scm:element id="C578" name="motivoCancellazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C727"
                global="false"
                containingComplexType="C576"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C216" name="enumProvvedimentoIscrizione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
      <scm:enumeration value="4"/>
   </scm:simpleType>
   <scm:element id="C190" name="anno" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C93"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:element id="C312" name="carattereDiControllo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C1"
                global="false"
                containingComplexType="C310"
                nillable="false"
                abstract="false"/>
   <scm:element id="C287" name="sentenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C711"
                global="false"
                containingComplexType="C286"
                nillable="false"
                abstract="false"/>
   <scm:element id="C218" name="nomeComune"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C217"
                nillable="false"
                abstract="false"/>
   <scm:element id="C359" name="scala" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C357"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C711" name="Sentenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C712"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C713"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C714"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C454" name="motivoComunicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C514"
                global="false"
                containingComplexType="C452"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C305" name="Comune"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C306"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C307"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C308"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C678" name="comunicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C385"
                global="false"
                containingComplexType="C676"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C439" name="idConsultazione" type="#long" global="false"
                  containingComplexType="C437"/>
   <scm:simpleType id="C487" base="#string" variety="atomic" primitiveType="#string">
      <scm:pattern value="([0-9])*|s.n.c."/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:element id="C499" name="Soggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C656"
                global="false"
                containingComplexType="C497"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C414" name="versione" type="C90" global="false"
                  containingComplexType="C413"/>
   <scm:element id="C340" name="dataCancellazioneAPR"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C337"
                nillable="false"
                abstract="false"/>
   <scm:element id="C73" name="numeroAtto"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C627"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C94" name="numerico"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
   </scm:simpleType>
   <scm:element id="C352" name="toponimo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C732"
                global="false"
                containingComplexType="C348"
                nillable="false"
                abstract="false"/>
   <scm:element id="C476" name="tipoSoggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C536"
                global="false"
                containingComplexType="C474"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C446" name="tipoDatiRettificati"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C447"/>
   </scm:complexType>
   <scm:element id="C152" name="flagResidenteDallaNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C493"
                global="false"
                containingComplexType="C148"
                nillable="false"
                abstract="false"/>
   <scm:element id="C601" name="ufficio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C33"
                global="false"
                containingComplexType="C600"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C457" name="DatiAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C458"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C459"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C63" name="codiceFiscaleConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C60"
                nillable="false"
                abstract="false"/>
   <scm:element id="C758" name="datiNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C95"
                global="false"
                containingComplexType="C756"
                nillable="false"
                abstract="false"/>
   <scm:element id="C138" name="testata"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C553"
                global="false"
                containingComplexType="C137"
                nillable="false"
                abstract="false"/>
   <scm:element id="C636" name="data"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C635"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C125" name="DatiEmigrazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C126"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C127"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C128"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C599" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2|3|4|5|6|7"/>
   </scm:simpleType>
   <scm:element id="C430" name="causale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C272"
                global="false"
                containingComplexType="C428"
                nillable="false"
                abstract="false"/>
   <scm:element id="C377" name="testata"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C652"
                global="false"
                containingComplexType="C376"
                nillable="false"
                abstract="false"/>
   <scm:element id="C86" name="sesso" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C741"
                global="false"
                containingComplexType="C83"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C282" name="Paternita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C283"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C284"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C285"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C426" name="interno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C423"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C728" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="0|1"/>
   </scm:simpleType>
   <scm:element id="C691" name="comuneRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C687"
                nillable="false"
                abstract="false"/>
   <scm:element id="C89" name="codiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C88"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C309" base="C3" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2"/>
   </scm:simpleType>
   <scm:simpleType id="C492" name="numerico8"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="8"/>
   </scm:simpleType>
   <scm:complexType id="C186" name="tipoStoricoResidenzeEstere"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C187"/>
   </scm:complexType>
   <scm:element id="C213" name="tribunale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C106" name="tipoFamigliaAnagrafica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C107"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C108"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C109"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C110"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C219" name="codiceComuneIstat"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C254"
                global="false"
                containingComplexType="C217"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C534" name="StoricoResidenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C535"/>
   </scm:complexType>
   <scm:simpleType id="C215" name="enumComportaConteggio"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C246" name="enumTipoIscrizioneAnagrafe"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C1"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
      <scm:enumeration value="D"/>
      <scm:enumeration value="R"/>
      <scm:enumeration value="C"/>
      <scm:enumeration value="B"/>
      <scm:enumeration value="A"/>
      <scm:enumeration value="U"/>
      <scm:enumeration value="N"/>
   </scm:simpleType>
   <scm:complexType id="C619" name="ResponsabileAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C96"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C620"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C621"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C613" name="NumeroCivico"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C614"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C615"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C616"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C617"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C444" name="interrogazioneAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C137"
                global="false"
                containingComplexType="C442"
                nillable="false"
                abstract="false"/>
   <scm:element id="C642" name="trascrizioneAttoMatrimonio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C635"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C254" name="numerico6"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="6"/>
   </scm:simpleType>
   <scm:element id="C690" name="cartaceaElettronica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C728"
                global="false"
                containingComplexType="C687"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C286" name="Divorzio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C287"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C288"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C289"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C290"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C72" name="serie"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C627"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:element id="C689" name="dataRilascioCI"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C687"
                nillable="false"
                abstract="false"/>
   <scm:element id="C117" name="dataValidita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C115"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C493" name="tipoSN" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C494"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="2"/>
      <scm:pattern value="SI|NO|si|no|Si|No|s|n|S|N"/>
   </scm:simpleType>
   <scm:attribute id="C672" name="dataOraRichiesta" type="#dateTime" global="false"
                  containingComplexType="C671"/>
   <scm:simpleType id="C48" name="alfabetico"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="([a-zA-Z])*"/>
   </scm:simpleType>
   <scm:complexType id="C40" name="tipoUbicazioneConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C41"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C42"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C43"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C44"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C45"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C46"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C739" name="luogoMatrimonio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C278"
                global="false"
                containingComplexType="C737"
                nillable="false"
                abstract="false"/>
   <scm:element id="C363" name="dataScadenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C361"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C552" name="tipoStatoCivile"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:pattern value="1|2|3|4|5"/>
   </scm:simpleType>
   <scm:complexType id="C742" name="tipoDatiNascitaConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C743"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C744"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C445" name="rispostaAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C376"
                global="false"
                containingComplexType="C442"
                nillable="false"
                abstract="false"/>
   <scm:element id="C35" name="descrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C33"
                nillable="false"
                abstract="false"/>
   <scm:element id="C315" name="enteRilascioPatente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C310"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C120" name="tipoComune"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C121"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C122"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C123"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C479" name="questuraRilascioSoggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C474"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C523" name="tipoVedovanza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C524"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C525"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C526"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C100" name="datiNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C538"
                global="false"
                containingComplexType="C95"
                nillable="false"
                abstract="false"/>
   <scm:element id="C530" name="cap"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C585"
                global="false"
                containingComplexType="C527"
                nillable="false"
                abstract="false"/>
   <scm:element id="C293" name="cognome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:element id="C697" name="numeroPraticaIscrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C693"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C661" name="tipoSinonimoPersoneFisicheConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C662"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C663"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C664"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C665"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C666"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C525" name="luogoMorteConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C278"
                global="false"
                containingComplexType="C523"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C429" name="dataOraRichiesta" type="#dateTime" global="false"
                  containingComplexType="C428"/>
   <scm:simpleType id="C168" name="enumTipoCancellazioneAnagrafe"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C1"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
      <scm:enumeration value="H"/>
      <scm:enumeration value="G"/>
      <scm:enumeration value="I"/>
      <scm:enumeration value="M"/>
      <scm:enumeration value="F"/>
      <scm:enumeration value="E"/>
   </scm:simpleType>
   <scm:element id="C302" name="statoCivile"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C552"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:element id="C441" name="consultazionePerNomeCognome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C83"
                global="false"
                containingComplexType="C437"
                nillable="false"
                abstract="false"/>
   <scm:element id="C738" name="dataMatrimonio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C737"
                nillable="false"
                abstract="false"/>
   <scm:element id="C724" name="codiceStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C214"
                global="false"
                containingComplexType="C722"
                nillable="false"
                abstract="false"/>
   <scm:element id="C253" name="denominazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C611"
                global="false"
                containingComplexType="C251"
                nillable="false"
                abstract="false"/>
   <scm:element id="C159" name="datiNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C538"
                global="false"
                containingComplexType="C158"
                nillable="false"
                abstract="false"/>
   <scm:element id="C52" name="lettera" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C50"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C272" name="CausaleConsultazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C273"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C274"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C275"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C276"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C277"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C194" name="numeroAtto"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C157"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C158" name="PersonaAnagraficaDN"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C96"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C97"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C98"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C99"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C159"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C648" name="tipoConfermaDatiAnagrafici"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C649"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C650"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C651"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C558" name="numero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C91"
                global="false"
                containingComplexType="C557"
                nillable="false"
                abstract="false"/>
   <scm:element id="C299" name="dataNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:element id="C245" name="localita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C448"
                global="false"
                containingComplexType="C242"
                nillable="false"
                abstract="false"/>
   <scm:element id="C75" name="tipoRegistro"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C1"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:element id="C654" name="rispostaDisponibile"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C493"
                global="false"
                containingComplexType="C652"
                nillable="false"
                abstract="false"/>
   <scm:element id="C205" name="fonteDato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C271"
                global="false"
                containingComplexType="C201"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C173" name="tipoRettifica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C174"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C175"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C176"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C609" name="numeroConvivenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C675"
                global="false"
                containingComplexType="C605"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C437" name="tipoConsultazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C438"/>
      <scm:attributeUse required="true" ref="C439"/>
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C440"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C441"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C110" name="codiceRelazioneParentela"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C436"
                global="false"
                containingComplexType="C106"
                nillable="false"
                abstract="false"/>
   <scm:element id="C306" name="nomeComune"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C305"
                nillable="false"
                abstract="false"/>
   <scm:element id="C197" name="numeroRegistro"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C561"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:element id="C212" name="numero" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C618"
                global="false"
                containingComplexType="C210"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C169" name="tipoMessaggio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C170"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C171"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C172"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C693" name="tipoIscrizioneAPR"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C694"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C695"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C696"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C697"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C698"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C699"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C700"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C701"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C176" name="dataRettifica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C173"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C4" name="PraticaEmigrazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C5"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C612" name="enumStatoDocumento"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C494"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="2"/>
      <scm:enumeration value="PE"/>
      <scm:enumeration value="PI"/>
      <scm:enumeration value="N"/>
      <scm:enumeration value="CE"/>
   </scm:simpleType>
   <scm:element id="C733" name="specie" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C732"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C474" name="tipoPermessoSoggiorno"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C475"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C476"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C477"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C478"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C479"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C440" name="consultazionePerCodiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C88"
                global="false"
                containingComplexType="C437"
                nillable="false"
                abstract="false"/>
   <scm:element id="C422" name="dataAssegnazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C419"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C162" name="Luogo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C163"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C164"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C165"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C127" name="dataEmigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C125"
                nillable="false"
                abstract="false"/>
   <scm:element id="C569" name="richiestaConsultazioneAnagrafica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C413"
                global="false"
                containingComplexType="C562"
                nillable="false"
                abstract="false"/>
   <scm:element id="C731" name="descrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C271"
                global="false"
                containingComplexType="C729"
                nillable="false"
                abstract="false"/>
   <scm:element id="C665" name="datiNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C742"
                global="false"
                containingComplexType="C661"
                nillable="false"
                abstract="false"/>
   <scm:element id="C548" name="toponimo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C251"
                global="false"
                containingComplexType="C547"
                nillable="false"
                abstract="false"/>
   <scm:element id="C681" name="consultazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C437"
                global="false"
                containingComplexType="C676"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C542" name="LibrettoSanitario"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C543"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C544"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C545"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C531" name="comune"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C527"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C671" name="PresaInCarico"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C672"/>
      <scm:attributeUse required="false" ref="C673" default="0"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C674"/>
   </scm:complexType>
   <scm:complexType id="C67" name="Atto"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C68"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C69"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C70"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C71"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C72"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C73"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C74"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C75"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C76"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C77"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C78"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C284" name="nomePadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C465"
                global="false"
                containingComplexType="C282"
                nillable="false"
                abstract="false"/>
   <scm:element id="C435" name="protocolloSAIA"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C92"
                global="false"
                containingComplexType="C432"
                nillable="false"
                abstract="false"/>
   <scm:element id="C10" name="codiceStatoEsteroEmigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C214"
                global="false"
                containingComplexType="C8"
                nillable="false"
                abstract="false"/>
   <scm:element id="C196" name="tipoRegistro"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C1"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C137" name="InterrogazioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C138"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C139"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C233" name="Coniuge"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C234"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C235"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C236"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C319" name="dataInizio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C317"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C147" name="Colore"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:pattern value="1|2"/>
   </scm:simpleType>
   <scm:element id="C516" name="ubicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C527"
                global="false"
                containingComplexType="C515"
                nillable="false"
                abstract="false"/>
   <scm:element id="C232" name="fonteDato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C228"
                nillable="false"
                abstract="false"/>
   <scm:element id="C743" name="luogoNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C278"
                global="false"
                containingComplexType="C742"
                nillable="false"
                abstract="false"/>
   <scm:element id="C740" name="attoMatrimonio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C188"
                global="false"
                containingComplexType="C737"
                nillable="false"
                abstract="false"/>
   <scm:element id="C463" name="data"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C461"
                nillable="false"
                abstract="false"/>
   <scm:element id="C189" name="comuneRegistrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:element id="C296" name="maternita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C683"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C465" name="charMax80"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="80"/>
   </scm:simpleType>
   <scm:element id="C209" name="fonteDato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C207"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C706" name="DatiMorte"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C707"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C708"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C316" name="provinciaRilascioPatente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C546"
                global="false"
                containingComplexType="C310"
                nillable="false"
                abstract="false"/>
   <scm:element id="C141" name="toponimo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C732"
                global="false"
                containingComplexType="C140"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C333" name="tipoVariazioneSesso"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C334"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C335"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C336"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C649" name="ufficio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C33"
                global="false"
                containingComplexType="C648"
                nillable="false"
                abstract="false"/>
   <scm:element id="C524" name="dataMorteConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C523"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C716" name="tipoPatenti"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C717"/>
   </scm:complexType>
   <scm:simpleType id="C371" name="enumTipoRichiestaCancellazione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="3"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:complexType id="C774" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C777"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C778"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C46" name="altro" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C40"
                nillable="false"
                abstract="false"/>
   <scm:element id="C238" name="datiEmigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C125"
                global="false"
                containingComplexType="C237"
                nillable="false"
                abstract="false"/>
   <scm:element id="C401" name="veicoli"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C355"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C772" name="residenzaItalia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C515"
                global="false"
                containingComplexType="C768"
                nillable="false"
                abstract="false"/>
   <scm:element id="C393" name="cessazioneAnnullamentoMatrimonio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C115"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C265" name="dataRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C263"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C564" name="versioneXMLSAIA" type="C90" global="false"
                  containingComplexType="C562"/>
   <scm:simpleType id="C611" name="charMax100"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="100"/>
   </scm:simpleType>
   <scm:element id="C12" name="numeroDistintivoAIRE"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#anyType"
                global="false"
                containingComplexType="C8"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C181" name="tipoUbicazioneEstera"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C182"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C183"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C184"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C185"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C494" name="charMax2"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="2"/>
   </scm:simpleType>
   <scm:element id="C685" name="nomeMadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C683"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C452" name="tipoCausale"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C453"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C454"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C455"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C456"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C55" name="enumMotivoSpedizionePratica"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C3"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="1"/>
      <scm:enumeration value="2"/>
      <scm:enumeration value="1"/>
   </scm:simpleType>
   <scm:element id="C362" name="identificativo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C604"
                global="false"
                containingComplexType="C361"
                nillable="false"
                abstract="false"/>
   <scm:element id="C230" name="dataInizio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C228"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C433" name="dataOraRichiesta" type="#dateTime" global="false"
                  containingComplexType="C432"/>
   <scm:element id="C449" name="descrizioneLocalita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C448"
                nillable="false"
                abstract="false"/>
   <scm:element id="C710" name="sinonimoPersoneFisiche"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C222"
                global="false"
                containingComplexType="C709"
                nillable="false"
                abstract="false"/>
   <scm:element id="C462" name="pratica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C513"
                global="false"
                containingComplexType="C461"
                nillable="false"
                abstract="false"/>
   <scm:element id="C123" name="siglaProvinciaIstat"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C546"
                global="false"
                containingComplexType="C120"
                nillable="false"
                abstract="false"/>
   <scm:element id="C331" name="descrizioneStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C329"
                nillable="false"
                abstract="false"/>
   <scm:element id="C699" name="luogoImmigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C278"
                global="false"
                containingComplexType="C693"
                nillable="false"
                abstract="false"/>
   <scm:element id="C281" name="localita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C329"
                global="false"
                containingComplexType="C278"
                nillable="false"
                abstract="false"/>
   <scm:element id="C631" name="settoreAttivitaEconomica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C3"
                global="false"
                containingComplexType="C628"
                nillable="false"
                abstract="false"/>
   <scm:element id="C486" name="dataVariazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C483"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C427" name="enumTipoConsultazione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:enumeration value="richiesta da comune"/>
      <scm:enumeration value="risposta da comune"/>
   </scm:simpleType>
   <scm:element id="C84" name="nome" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C83"
                nillable="false"
                abstract="false"/>
   <scm:element id="C565" name="mittente"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C480"
                global="false"
                containingComplexType="C562"
                nillable="false"
                abstract="false"/>
   <scm:element id="C130" name="dataMorteConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C129"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C586" name="Cittadino"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C587"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C588"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C589"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C590"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C591"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C592"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C593"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C594"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C595"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C596"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C597"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C598"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C51" name="numero" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C94"
                global="false"
                containingComplexType="C50"
                nillable="false"
                abstract="false"/>
   <scm:element id="C366" name="codiceFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C221"
                global="false"
                containingComplexType="C365"
                nillable="false"
                abstract="false"/>
   <scm:element id="C104" name="dataFine"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C101"
                nillable="false"
                abstract="false"/>
   <scm:element id="C647" name="dataRevoca"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C644"
                nillable="false"
                abstract="false"/>
   <scm:element id="C7" name="dataPratica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C5"
                nillable="false"
                abstract="false"/>
   <scm:element id="C679" name="statoCode"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C18"
                global="false"
                containingComplexType="C676"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C1" name="char1" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
   </scm:simpleType>
   <scm:simpleType id="C90" name="tipoVersione"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C91"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:element id="C639" name="regimePatrimoniale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C0"
                global="false"
                containingComplexType="C635"
                nillable="false"
                abstract="false"/>
   <scm:element id="C421" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C217"
                global="false"
                containingComplexType="C419"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C718" name="Localita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C719"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C720"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C721"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C591" name="residenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C515"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C573" name="tipoStoricoResidenze"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C574"/>
   </scm:complexType>
   <scm:element id="C239" name="iscrizioneAIRE"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C8"
                global="false"
                containingComplexType="C237"
                nillable="false"
                abstract="false"/>
   <scm:element id="C775" name="codiceISTATStatoEsteroProvenienza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C254"
                global="false"
                containingComplexType="C771"
                nillable="false"
                abstract="false"/>
   <scm:element id="C29" name="dataDecorrenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C26"
                nillable="false"
                abstract="false"/>
   <scm:element id="C629" name="descrizioneProfessione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C727"
                global="false"
                containingComplexType="C628"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C56" name="Morte"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C57"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C58"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C598" name="statistiche"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C779"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C779" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C780"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C781"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C781" name="censito"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C493"
                global="false"
                containingComplexType="C779"
                nillable="false"
                abstract="false"/>
   <scm:element id="C143" name="cap" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C585"
                global="false"
                containingComplexType="C140"
                nillable="false"
                abstract="false"/>
   <scm:element id="C572" name="codiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C571"
                nillable="false"
                abstract="false"/>
   <scm:element id="C664" name="nome" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C661"
                nillable="false"
                abstract="false"/>
   <scm:element id="C597" name="datiAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C782"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C782" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C783"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C784"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C783" name="iscrizioneAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C26"
                global="false"
                containingComplexType="C782"
                nillable="false"
                abstract="false"/>
   <scm:element id="C784" name="cancellazioneAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C576"
                global="false"
                containingComplexType="C782"
                nillable="false"
                abstract="false"/>
   <scm:element id="C590" name="famigliaAnagrafica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C365"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:element id="C269" name="interdizioneEspatrio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C493"
                global="false"
                containingComplexType="C262"
                nillable="false"
                abstract="false"/>
   <scm:element id="C539" name="luogoNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C162"
                global="false"
                containingComplexType="C538"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C262" name="CartaIdentita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="C263"
                    derivationMethod="extension"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C264"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C265"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C266"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C267"/>
               </scm:sequence>
            </scm:modelGroupParticle>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:sequence>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C268"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C269"/>
                  <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C270"/>
               </scm:sequence>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C103" name="dataInizio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C101"
                nillable="false"
                abstract="false"/>
   <scm:element id="C778" name="targa" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C466"
                global="false"
                containingComplexType="C774"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C419" name="tipoAssegnazioneCodiceFiscaleConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C420"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C421"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C422"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C182" name="toponimo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C732"
                global="false"
                containingComplexType="C181"
                nillable="false"
                abstract="false"/>
   <scm:element id="C292" name="codiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C291"
                nillable="false"
                abstract="false"/>
   <scm:element id="C415" name="statoCode"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C432"
                global="false"
                containingComplexType="C413"
                nillable="false"
                abstract="false"/>
   <scm:element id="C58" name="datiMorte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C706"
                global="false"
                containingComplexType="C56"
                nillable="false"
                abstract="false"/>
   <scm:element id="C68" name="comuneRegistrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C67"
                nillable="false"
                abstract="false"/>
   <scm:element id="C705" name="codiceFiscalePadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C702"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C380" name="Nascita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C381"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C382"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C383"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C384"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C529" name="numeroCivico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C613"
                global="false"
                containingComplexType="C527"
                nillable="false"
                abstract="false"/>
   <scm:element id="C473" name="dataVariazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C471"
                nillable="false"
                abstract="false"/>
   <scm:element id="C258" name="dataFine"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C255"
                nillable="false"
                abstract="false"/>
   <scm:element id="C183" name="numeroCivico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C322"
                global="false"
                containingComplexType="C181"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C5" name="Pratica"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C6"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C7"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C536" base="C327" variety="atomic" primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="10"/>
      <scm:pattern value="PERMESSO|CARTA"/>
   </scm:simpleType>
   <scm:element id="C122" name="codiceComuneIstat"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C254"
                global="false"
                containingComplexType="C120"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C344" name="tipoPresaInCarico"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:attributeUse required="false" ref="C345"/>
      <scm:attributeUse required="false" ref="C346" default="0"/>
      <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C347"/>
   </scm:complexType>
   <scm:element id="C351" name="frazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C348"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C618" name="numericoMax20"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:element id="C145" name="distretto"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C735"
                global="false"
                containingComplexType="C140"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C346" name="ultimoId" type="#string" global="false"
                  containingComplexType="C344"/>
   <scm:simpleType id="C214" name="char3" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="3"/>
   </scm:simpleType>
   <scm:element id="C224" name="cognome"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C222"
                nillable="false"
                abstract="false"/>
   <scm:element id="C339" name="descrizioneMotivoCancellazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C337"
                nillable="false"
                abstract="false"/>
   <scm:element id="C577" name="tipoCancellazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C168"
                global="false"
                containingComplexType="C576"
                nillable="false"
                abstract="false"/>
   <scm:element id="C171" name="codice" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C327"
                global="false"
                containingComplexType="C169"
                nillable="false"
                abstract="false"/>
   <scm:element id="C528" name="toponimo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C251"
                global="false"
                containingComplexType="C527"
                nillable="false"
                abstract="false"/>
   <scm:element id="C375" name="dataValidita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C372"
                nillable="false"
                abstract="false"/>
   <scm:element id="C651" name="data" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C648"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C436" base="C161" variety="atomic" primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:maxLength value="2"/>
      <scm:pattern value="1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|20|21|22|23"/>
   </scm:simpleType>
   <scm:complexType id="C134" name="tipoUfficioConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C135"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C136"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C667" name="tipoTesseraSanitariaConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C668"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C669"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C670"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C561" name="charMax5"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="5"/>
   </scm:simpleType>
   <scm:element id="C116" name="sentenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C210"
                global="false"
                containingComplexType="C115"
                nillable="false"
                abstract="false"/>
   <scm:element id="C324" name="lettera"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C322"
                nillable="false"
                abstract="false"/>
   <scm:element id="C6" name="numeroPratica"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C118"
                global="false"
                containingComplexType="C5"
                nillable="false"
                abstract="false"/>
   <scm:element id="C382" name="attoNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C380"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C753" name="tipoUtente"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C754"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C755"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C695" name="descrizioneMotivoIscrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C693"
                nillable="false"
                abstract="false"/>
   <scm:element id="C704" name="nomePadre"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C465"
                global="false"
                containingComplexType="C702"
                nillable="false"
                abstract="false"/>
   <scm:element id="C607" name="descrizioneConvivenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C611"
                global="false"
                containingComplexType="C605"
                nillable="false"
                abstract="false"/>
   <scm:element id="C544" name="dataRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C542"
                nillable="false"
                abstract="false"/>
   <scm:element id="C701" name="numeroFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C726"
                global="false"
                containingComplexType="C693"
                nillable="false"
                abstract="false"/>
   <scm:element id="C746" name="nuovoPermessoSoggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C474"
                global="false"
                containingComplexType="C745"
                nillable="false"
                abstract="false"/>
   <scm:element id="C288" name="dataValidita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C286"
                nillable="false"
                abstract="false"/>
   <scm:element id="C193" name="serie" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C327"
                global="false"
                containingComplexType="C188"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C737" name="tipoMatrimonio"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C738"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C739"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C740"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C752" name="enumSesso"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C741"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="M|F|m|f"/>
      <scm:enumeration value="M"/>
      <scm:enumeration value="m"/>
      <scm:enumeration value="F"/>
      <scm:enumeration value="f"/>
   </scm:simpleType>
   <scm:complexType id="C60" name="tipoConiuge"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C61"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C62"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C63"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C759" name="Indirizzo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C760"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C761"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C762"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C763"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C764"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C765"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C490" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C217"
                global="false"
                containingComplexType="C488"
                nillable="false"
                abstract="false"/>
   <scm:element id="C655" name="numeroPersone"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="#anyType"
                global="false"
                containingComplexType="C652"
                nillable="false"
                abstract="false"/>
   <scm:element id="C511" name="regimePatrimoniale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C1"
                global="false"
                containingComplexType="C509"
                nillable="false"
                abstract="false"/>
   <scm:element id="C165" name="localita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C718"
                global="false"
                containingComplexType="C162"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C652" name="TestataRisposta"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C653"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C654"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C655"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C187" name="residenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C228"
                global="false"
                containingComplexType="C186"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C337" name="tipoCancellazioneAPR"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C338"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C339"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C340"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C341"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C342"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C343"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:complexType id="C547" name="UbicazioneEstera"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C548"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C549"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C550"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C551"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C231" name="dataFine"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C228"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C571" name="tipoElencoCodiciFiscaliConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C572"/>
   </scm:complexType>
   <scm:element id="C400" name="patenti"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C716"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C270" name="dataScadenzaInterdizioneEspatrio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C262"
                nillable="false"
                abstract="false"/>
   <scm:element id="C389" name="generalita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C291"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:element id="C227" name="residenza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C101"
                global="false"
                containingComplexType="C222"
                nillable="false"
                abstract="false"/>
   <scm:element id="C133" name="trascrizioneAttoMorteConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C129"
                nillable="false"
                abstract="false"/>
   <scm:element id="C417" name="comunicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C428"
                global="false"
                containingComplexType="C413"
                nillable="false"
                abstract="false"/>
   <scm:element id="C670" name="dataRevoca"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C667"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C153" base="#string" variety="atomic" primitiveType="#string">
      <scm:pattern value="([0-9])*|s.n.c."/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:element id="C507" name="interno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C82"
                global="false"
                containingComplexType="C504"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C111" name="tipoElencoSinonimiPersoneFisicheConsultazioneAE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="0" maxOccurs="unbounded" ref="C112"/>
   </scm:complexType>
   <scm:element id="C379" name="piede"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C605"
                global="false"
                containingComplexType="C376"
                nillable="false"
                abstract="false"/>
   <scm:element id="C533" name="altro"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C271"
                global="false"
                containingComplexType="C527"
                nillable="false"
                abstract="false"/>
   <scm:element id="C54" name="civicoInterno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C423"
                global="false"
                containingComplexType="C50"
                nillable="false"
                abstract="false"/>
   <scm:element id="C766" name="richiestaSAIA"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C562"
                global="true"
                nillable="false"
                abstract="false"/>
   <scm:element id="C755" name="password"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C604"
                global="false"
                containingComplexType="C753"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C461" name="CancellazioneAIRE"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C462"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C463"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C464"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C489" name="ufficio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C134"
                global="false"
                containingComplexType="C488"
                nillable="false"
                abstract="false"/>
   <scm:element id="C277" name="tipoConsultazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C427"
                global="false"
                containingComplexType="C272"
                nillable="false"
                abstract="false"/>
   <scm:element id="C420" name="ufficio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C134"
                global="false"
                containingComplexType="C419"
                nillable="false"
                abstract="false"/>
   <scm:element id="C412" name="numeroLibrettoPensione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C537"
                global="false"
                containingComplexType="C409"
                nillable="false"
                abstract="false"/>
   <scm:element id="C475" name="numeroSoggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C91"
                global="false"
                containingComplexType="C474"
                nillable="false"
                abstract="false"/>
   <scm:element id="C500" name="dataRilascio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C497"
                nillable="false"
                abstract="false"/>
   <scm:element id="C640" name="cessazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C785"
                global="false"
                containingComplexType="C635"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C785" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:choice>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C786"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C787"/>
         </scm:choice>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C787" name="divorzio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C286"
                global="false"
                containingComplexType="C785"
                nillable="false"
                abstract="false"/>
   <scm:element id="C786" name="vedovanza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C129"
                global="false"
                containingComplexType="C785"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C188" name="tipoAtto"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C189"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C190"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C191"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C192"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C193"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C194"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C195"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C196"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C197"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C34" name="codice" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C91"
                global="false"
                containingComplexType="C33"
                nillable="false"
                abstract="false"/>
   <scm:element id="C614" name="numero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C487"
                global="false"
                containingComplexType="C613"
                nillable="false"
                abstract="false"/>
   <scm:element id="C416" name="presaInCarico"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C671"
                global="false"
                containingComplexType="C413"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C251" name="Toponimo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C252"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C253"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C131" name="luogoMorteConiuge"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C162"
                global="false"
                containingComplexType="C129"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C715" name="alfabetico3"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C48"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="([a-zA-Z])*"/>
      <scm:length value="3"/>
   </scm:simpleType>
   <scm:element id="C185" name="altro" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C181"
                nillable="false"
                abstract="false"/>
   <scm:element id="C481" name="comuneDiComunicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C480"
                nillable="false"
                abstract="false"/>
   <scm:element id="C384" name="paternita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C198"
                global="false"
                containingComplexType="C380"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C527" name="Ubicazione"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C528"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C529"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C530"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C531"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C532"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C533"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C522" name="fonteDato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C271"
                global="false"
                containingComplexType="C520"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C91" name="charMax20"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:minLength value="1"/>
      <scm:maxLength value="20"/>
   </scm:simpleType>
   <scm:complexType id="C83" name="tipoConsultazionePerNomeCognome"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C84"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C85"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C86"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C87"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C395" name="indirizzo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C348"
                global="false"
                containingComplexType="C385"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C157" name="charMax10NoMin"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="([0-9])*"/>
      <scm:maxLength value="10"/>
   </scm:simpleType>
   <scm:complexType id="C515" name="Residenza"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C516"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C517"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C518"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C519"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C594" name="pensioni"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C788"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C788" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C789"/>
   </scm:complexType>
   <scm:element id="C789" name="pensione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C409"
                global="false"
                containingComplexType="C788"
                nillable="false"
                abstract="false"/>
   <scm:element id="C543" name="numero"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C91"
                global="false"
                containingComplexType="C542"
                nillable="false"
                abstract="false"/>
   <scm:element id="C780" name="iscrittoListeElettorali"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C493"
                global="false"
                containingComplexType="C779"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C348" name="tipoIndirizzo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C349"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C350"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C351"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C352"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C353"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C354"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C595" name="documenti"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C790"
                global="false"
                containingComplexType="C586"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C790" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C791"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C792"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C793"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C793" name="librettoDiLavoro"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C263"
                global="false"
                containingComplexType="C790"
                nillable="false"
                abstract="false"/>
   <scm:element id="C791" name="librettoSanitario"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C263"
                global="false"
                containingComplexType="C790"
                nillable="false"
                abstract="false"/>
   <scm:element id="C792" name="cartaIdentita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C262"
                global="false"
                containingComplexType="C790"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C278" name="tipoLuogo"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C279"/>
            <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
               <scm:choice>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C280"/>
                  <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C281"/>
               </scm:choice>
            </scm:modelGroupParticle>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C621" name="fax"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#string"
                global="false"
                containingComplexType="C619"
                nillable="false"
                abstract="false"/>
   <scm:element id="C226" name="datiNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C15"
                global="false"
                containingComplexType="C222"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C177" name="Generalita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C178"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C179"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C180"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C776" name="codiceISTATComuneProvenienza"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C254"
                global="false"
                containingComplexType="C771"
                nillable="false"
                abstract="false"/>
   <scm:element id="C290" name="trascrizioneAttoDivorzio"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C67"
                global="false"
                containingComplexType="C286"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C361" name="TesseraSanitaria"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C362"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C363"/>
            <scm:elementParticle minOccurs="0" maxOccurs="1" ref="C364"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C650" name="comune" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C120"
                global="false"
                containingComplexType="C648"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C741" name="tipoSesso"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:pattern value="M|F|m|f"/>
   </scm:simpleType>
   <scm:complexType id="C467" name="tipoApplicazioneAnagrafe"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C468"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C469"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C470"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:simpleType id="C59" name="enumTipoPratica"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C1"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="1"/>
      <scm:enumeration value="D"/>
      <scm:enumeration value="R"/>
      <scm:enumeration value="H"/>
      <scm:enumeration value="C"/>
      <scm:enumeration value="B"/>
      <scm:enumeration value="G"/>
      <scm:enumeration value="I"/>
      <scm:enumeration value="M"/>
      <scm:enumeration value="A"/>
      <scm:enumeration value="F"/>
      <scm:enumeration value="U"/>
      <scm:enumeration value="N"/>
      <scm:enumeration value="E"/>
   </scm:simpleType>
   <scm:element id="C174" name="nomeCampo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C173"
                nillable="false"
                abstract="false"/>
   <scm:element id="C750" name="localita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C448"
                global="false"
                containingComplexType="C747"
                nillable="false"
                abstract="false"/>
   <scm:element id="C360" name="interno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C357"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C538" name="DatiNascita"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C539"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C540"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C250" name="attoMorte"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C188"
                global="false"
                containingComplexType="C247"
                nillable="false"
                abstract="false"/>
   <scm:element id="C502" name="questuraRilascioSoggiorno"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C305"
                global="false"
                containingComplexType="C497"
                nillable="false"
                abstract="false"/>
   <scm:element id="C777" name="tipo" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C124"
                global="false"
                containingComplexType="C774"
                nillable="false"
                abstract="false"/>
   <scm:element id="C150" name="datiIscrizioneAnagrafe"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C794"
                global="false"
                containingComplexType="C148"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C794" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C795"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C796"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C797"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C795" name="particella"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C794"
                nillable="false"
                abstract="false"/>
   <scm:element id="C797" name="numero" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C618"
                global="false"
                containingComplexType="C794"
                nillable="false"
                abstract="false"/>
   <scm:element id="C796" name="serie" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C82"
                global="false"
                containingComplexType="C794"
                nillable="false"
                abstract="false"/>
   <scm:element id="C180" name="datiAnagrafici"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C198"
                global="false"
                containingComplexType="C177"
                nillable="false"
                abstract="false"/>
   <scm:element id="C378" name="corpo"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C79"
                global="false"
                containingComplexType="C376"
                nillable="false"
                abstract="false"/>
   <scm:element id="C343" name="luogoEmigrazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C278"
                global="false"
                containingComplexType="C337"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C541" name="tipoCodiceFiscale"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="16"/>
      <scm:pattern value="([A-Z0-9])*"/>
   </scm:simpleType>
   <scm:element id="C608" name="numeroFamiglia"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C675"
                global="false"
                containingComplexType="C605"
                nillable="false"
                abstract="false"/>
   <scm:element id="C275" name="comuneDestinazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C305"
                global="false"
                containingComplexType="C272"
                nillable="false"
                abstract="false"/>
   <scm:element id="C662" name="codiceFiscale"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C541"
                global="false"
                containingComplexType="C661"
                nillable="false"
                abstract="false"/>
   <scm:element id="C458" name="iscrizione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C8"
                global="false"
                containingComplexType="C457"
                nillable="false"
                abstract="false"/>
   <scm:element id="C455" name="entiDestinatari"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C798"
                global="false"
                containingComplexType="C452"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C798" base="#anyType" derivationMethod="restriction" abstract="false"
                    variety="element-only">
      <scm:elementParticle minOccurs="1" maxOccurs="unbounded" ref="C799"/>
   </scm:complexType>
   <scm:element id="C799" name="ente" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C327"
                global="false"
                containingComplexType="C798"
                nillable="false"
                abstract="false"/>
   <scm:element id="C696" name="dataIscrizioneAPR"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C693"
                nillable="false"
                abstract="false"/>
   <scm:element id="C616" name="colore"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#anyType"
                global="false"
                containingComplexType="C613"
                nillable="false"
                abstract="false"/>
   <scm:element id="C256" name="ubicazione"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C40"
                global="false"
                containingComplexType="C255"
                nillable="false"
                abstract="false"/>
   <scm:element id="C491" name="data" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#date"
                global="false"
                containingComplexType="C488"
                nillable="false"
                abstract="false"/>
   <scm:element id="C13" name="data"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#anyType"
                global="false"
                containingComplexType="C8"
                nillable="false"
                abstract="false"/>
   <scm:complexType id="C557" name="LibrettoDiLavoro"
                    targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                    base="#anyType"
                    derivationMethod="restriction"
                    abstract="false"
                    variety="element-only">
      <scm:modelGroupParticle minOccurs="1" maxOccurs="1">
         <scm:sequence>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C558"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C559"/>
            <scm:elementParticle minOccurs="1" maxOccurs="1" ref="C560"/>
         </scm:sequence>
      </scm:modelGroupParticle>
   </scm:complexType>
   <scm:element id="C540" name="dataNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C538"
                nillable="false"
                abstract="false"/>
   <scm:element id="C200" name="datiNascita"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="C538"
                global="false"
                containingComplexType="C198"
                nillable="false"
                abstract="false"/>
   <scm:element id="C606" name="tipoRelazioneFamiliare"
                targetNamespace="http://saia.ancitel.it/xmlmanager/consultazione"
                type="C114"
                global="false"
                containingComplexType="C605"
                nillable="false"
                abstract="false"/>
   <scm:element id="C451" name="codiceStato"
                targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="#string"
                global="false"
                containingComplexType="C448"
                nillable="false"
                abstract="false"/>
   <scm:attribute id="C20" name="ultimoId" type="#string" global="false"
                  containingComplexType="C18"/>
   <scm:element id="C135" name="codice" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                type="C91"
                global="false"
                containingComplexType="C134"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C93" name="numerico4"
                   targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="C94"
                   variety="atomic"
                   primitiveType="#string">
      <scm:whiteSpace value="collapse"/>
      <scm:pattern value="(\d)+"/>
      <scm:length value="4"/>
   </scm:simpleType>
   <scm:element id="C625" name="data"
                targetNamespace="http://saia.ancitel.it/xmlmanager/anagrafico"
                type="#date"
                global="false"
                containingComplexType="C622"
                nillable="false"
                abstract="false"/>
   <scm:simpleType id="C495" name="char16" targetNamespace="http://saia.ancitel.it/xmlmanager/ap5"
                   base="#string"
                   variety="atomic"
                   primitiveType="#string">
      <scm:length value="16"/>
   </scm:simpleType>
</scm:schema>