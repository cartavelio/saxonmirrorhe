package performance;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.xpath.XPathEvaluator;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathFactory;
import java.io.StringReader;

public class XPathMemoryTest {

    public static void main(String[] args) throws Exception {

        final String xml = "<?xml version=\"1.0\"?> <message>    <headers>       <header name=\"CREATED_BY\"></header>       <header name=\"xx.isMultipart\">true</header>       <header name=\"JMSMessageID\">ID:ffffffffcbed081a:a5ab5a1:16A9D21B12A</header>       <header name=\"JMSExpiration\">0</header>       <header name=\"JMS_acme_ExtendedType\">x-acmeXX-multipart</header>       <header name=\"JMSRedelivered\">false</header>       <header name=\"JMSTimestamp\">1557414392106</header>       <header name=\"JMSDeliveryMode\">2</header>       <header name=\"JMSPriority\">0</header>       <header name=\"JMSDestination\">t.esb.SRIUnsolicitedInboundService1</header>       <header name=\"InboundEvent\">acme.apps.po.publish</header>       <header name=\"CHANNEL\">AUTOMATIC</header>       <header name=\"COUNTRY_CODE\">XX</header>    </headers>    <parts>       <part type=\"text/xml\" id=\"name\"> <WfEvent>    <Name>acme.apps.po.publish</Name>    <Key>acme.apps.po.publish305066</Key>    <Priority>       <test>123</test>       <test>234</test>    </Priority>    <SendDate>2019/05/01 12:21:40</SendDate>    <ReceiveDate>null</ReceiveDate>    <CorrelationId>null</CorrelationId>    <Parameters> 	     <JMSXDeliveryCount>1</JMSXDeliveryCount> 	     <JMS_OracleConnectionID>null</JMS_OracleConnectionID> 	     <JMSXGroupSeq>null</JMSXGroupSeq> 	     <JMSXAppID>null</JMSXAppID> 	     <JMSXRecvTimestamp>1556713300401</JMSXRecvTimestamp> 	     <JMS_OracleExcpQ>null</JMS_OracleExcpQ> 	     <JMS_OracleHeaderOnly>null</JMS_OracleHeaderOnly> 	     <JMS_OracleOriginalMessageID>null</JMS_OracleOriginalMessageID> 	     <JMSXGroupID>null</JMSXGroupID> 	     <JMSXUserID>null</JMSXUserID> 	     <JMS_OracleDeliveryMode>null</JMS_OracleDeliveryMode> 	     <JMS_OracleDelay>0</JMS_OracleDelay> 	     <JMSXState>0</JMSXState>    </Parameters>    <Data>       <EVENT_DATA>          <ESB_PO_PUB>             <PO_NUMBER>7340017377</PO_NUMBER>             <VENDOR_NUM>205415</VENDOR_NUM>             <VENDOR_SITE_CODE>ISP-FAI-734-NZD</VENDOR_SITE_CODE>             <ES_REQUISITION_LINE_ID>38946630</ES_REQUISITION_LINE_ID>             <QUANTITY>1</QUANTITY>             <UNIT_PRICE>36</UNIT_PRICE>             <CURRENCY_CODE>NZD</CURRENCY_CODE>             <RATE_TYPE/>             <RATE_DATE/>             <RATE/>             <NEED_BY_DATE>01-MAY-19</NEED_BY_DATE>             <ITEM_DESCRIPTION>4970438311 RTU-MEI CASH ACCEPTOR SC66 / SCL6607R</ITEM_DESCRIPTION>             <UNIT_MEAS_LOOKUP_CODE>Each</UNIT_MEAS_LOOKUP_CODE>             <SHIPMENT_NUM>1</SHIPMENT_NUM>             <DISTRIBUTION_NUM>1</DISTRIBUTION_NUM>             <PO_LINE_NUM>1</PO_LINE_NUM>             <CHARGE_ACCOUNT>734-715000000-1525-8871-000-000000-00</CHARGE_ACCOUNT>             <ES_REQUISITION_LINE_NUMBER>1</ES_REQUISITION_LINE_NUMBER>             <ES_REQUISITION_NUMBER>132201</ES_REQUISITION_NUMBER>             <CATEGORY>00000000</CATEGORY>             <ES_INSTANCE>WORLD</ES_INSTANCE>          </ESB_PO_PUB>       </EVENT_DATA>    </Data>    <FromAgent> 	     <Name>acme_PO_QAGENT</Name> 	     <System>ER64R06.WORLD</System>    </FromAgent> </WfEvent> </part>    </parts> </message> ";
        final String expression = "/message/headers/header [@name=\"COUNTRY_CODE\"]=\"GB\" or /message/headers/header [@name=\"COUNTRY_CODE\"]=\"IE\" or /message/headers/header [@name=\"COUNTRY_CODE\"]=\"IN\" or /message/headers/header [@name=\"COUNTRY_CODE\"]=\"BE\" or /message/headers/header [@name=\"COUNTRY_CODE\"]=\"JP\"";

        // XPathFactory xpathFactory =
        // XPathFactory.newInstance(XPathConstants.DOM_OBJECT_MODEL);
        XPathFactory xpathFactory = new net.sf.saxon.xpath.XPathFactoryImpl();
        XPath xpath = xpathFactory.newXPath();
        // xpath.setNamespaceContext(nc);
        final XPathEvaluator xpathEvaluator_ = (XPathEvaluator) xpath;

        for (int i = 0; i < 20; i++) {

            new Thread() {
                @Override
                public void run() {

                    try {
                        for (int c = 0; c < 10000; c++) {
                            InputSource source = new InputSource(new StringReader(xml));
                            XPathExpression xpathExpression = xpathEvaluator_.compile(expression);
                            NodeInfo doc = xpathEvaluator_.getConfiguration().buildDocumentTree(new SAXSource(source))
                                    .getRootNode();
                            Boolean result = (Boolean) xpathExpression.evaluate(doc, XPathConstants.BOOLEAN);

                            // This seems to reduce/solve it but not sure why
                            // Runtime.getRuntime().gc();
                            // ran it with -Xmx1g -XX:+UseG1GC

                            System.out.println(Runtime.getRuntime().totalMemory() / 1024 / 1000);
                            Instrumentation.report();

                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }

                }

            }.start();
        }

    }
}
