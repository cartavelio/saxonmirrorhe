package com.saxonica.functions.extfn;

import com.sun.mail.util.MailSSLSocketFactory;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import javax.activation.DataHandler;
import javax.activation.DataSource;
import javax.activation.FileDataSource;
import javax.mail.BodyPart;
import javax.mail.Message;
import javax.mail.MessagingException;
import javax.mail.internet.AddressException;
import javax.mail.internet.InternetAddress;
import javax.mail.internet.MimeBodyPart;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.stream.StreamResult;
import java.io.StringWriter;
import java.io.UnsupportedEncodingException;
import java.net.MalformedURLException;
import java.net.URL;
import java.security.GeneralSecurityException;
import java.util.*;

/**
 * This class implements the extension function saxon:send-mail(). This function
 * provides the functionality to construct and send email messages.
 */

public class SendMail extends SystemFunction {

    public static OptionsParameter OPTION_DETAILS;

    static {
        OptionsParameter options = new OptionsParameter();
        options.addRequiredOption("smtp-server", SequenceType.SINGLE_STRING);
        options.addRequiredOption("to", SequenceType.STRING_SEQUENCE);
        options.addAllowedOption("cc", SequenceType.STRING_SEQUENCE, EmptyAtomicSequence.getInstance());
        options.addAllowedOption("bcc", SequenceType.STRING_SEQUENCE, EmptyAtomicSequence.getInstance());
        options.addAllowedOption("from", SequenceType.SINGLE_STRING);
        options.addAllowedOption("username", SequenceType.SINGLE_STRING);
        options.addAllowedOption("password", SequenceType.SINGLE_STRING);
        options.addAllowedOption("realname", SequenceType.SINGLE_STRING);
        options.addAllowedOption("html", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        //options.setAllowedValues("html", SaxonErrorCode.SXJE0051, "true", "false");
        options.addAllowedOption("ssl", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        //options.setAllowedValues("ssl", SaxonErrorCode.SXJE0051, "true", "false");
        options.addAllowedOption("port", SequenceType.SINGLE_INTEGER, new Int64Value(25));
        options.setErrorCodeForAbsentValue(SaxonErrorCode.SXJM0001);
        options.setAllowCastFromString(true);
        OPTION_DETAILS = options;
    }


    /**
     * Determine the special properties of this function. The general rule
     * is that a system function call is non-creative unless more details
     * are defined in a subclass.
     *
     * @param arguments the actual arguments supplied in a call to the function
     */
    @Override
    public int getSpecialProperties(Expression[] arguments) {
        return 0; // has side effects, so not loop-liftable
    }


    private static GroundedValue checkMailProperty(XPathContext context, MapItem emailConfigMap, String keyStr, String errorMessage, SequenceType expectedType) throws XPathException {
        try {
            Sequence sequence = emailConfigMap.get(new StringValue(keyStr));
            GroundedValue groundedValue = sequence.materialize();
            if (!expectedType.matches(groundedValue, context.getConfiguration().getTypeHierarchy())) {
                throw new XPathException("saxon:send-mail error: Invalid type for email configuration option " + keyStr, SaxonErrorCode.SXJM0001, context);
            }
            return groundedValue;
        } catch (Exception e) {
            if (errorMessage == null) {
                return null;
            } else {
                throw new XPathException(errorMessage);
            }
        }
    }


    /**
     * returns InternetAddress object. The address must be syntactically correct otherwise we throw XPathException.
     *
     * @param addrStr value which is parsed to the email address and personal name parts.
     * @param context the XPath dynamic context
     * @return the InternetAddress object corresponding to the supplied address
     * @throws XPathException if a dynamic error occurs
     */

    private static InternetAddress parseInternetAddress(String addrStr, XPathContext context) throws XPathException {
        if (addrStr.contains("<")) {
            String[] splitStr = addrStr.split("<");
            String personalName = splitStr[0].substring(0, splitStr[0].length() - 1);
            String address = splitStr[1].substring(0, splitStr[1].length() - 1);
            try {
                return new InternetAddress(address, personalName);
            } catch (UnsupportedEncodingException e) {
                throw new XPathException("saxon:send-mail error:" + e.getMessage(), SaxonErrorCode.SXJM0002, context);
            }
        } else {

            try {
                return new InternetAddress(addrStr);
            } catch (AddressException e) {
                throw new XPathException("saxon:send-mail error:" + e.getMessage(), SaxonErrorCode.SXJM0002, context);
            }
        }
    }


    private static InternetAddress[] createRecipients(Sequence seq, XPathContext context) throws XPathException {
        SequenceIterator iter = seq.iterate();
        List<InternetAddress> recipient = new ArrayList<>();
        for (Item item; (item = iter.next()) != null; ) {
            try {
                recipient.add(parseInternetAddress(item.getStringValue(), context));
            } catch (Exception e) {
                throw new XPathException("sendMail Error:" + e.getMessage(), SaxonErrorCode.SXJM0002, context);
            }
        }
        return recipient.toArray(new InternetAddress[0]);
    }


    private static BodyPart createAttachment(XPathContext context, Item item) throws XPathException, MessagingException {
        MapItem mapItem;
        if (item instanceof MapItem) {
            mapItem = (MapItem) item;
        } else {
            throw new XPathException("saxon:send-mail error: Invalid attachment type ", SaxonErrorCode.SXJE0051, context);
        }

        BodyPart attachment = new MimeBodyPart();
        String setFileName = null;
        String otherStr;

        // throw new XPathException("Invalid number of arguments  for attachment map", SaxonErrorCode.SXJE0051, context);
        if (mapItem.get(StringValue.bmp("setFilename")) != null) {
            setFileName = ((Item) checkMailProperty(context, mapItem, "setFilename", null, SequenceType.SINGLE_STRING)).getStringValue();
            attachment.setFileName(setFileName);
        }

        if (mapItem.get(StringValue.bmp("contentFromFile")) != null) {
            otherStr = checkMailProperty(context, mapItem, "contentFromFile", null, SequenceType.SINGLE_STRING)
                    .getUnicodeStringValue().toString();
            if (otherStr != null) {
                DataSource source = new FileDataSource(otherStr);
                //attachment.setContent(source, mimeType);
                attachment.setDataHandler(new DataHandler(source));
                if (setFileName == null) {
                    attachment.setFileName(source.getName());
                }
                return attachment;

            }
        }

        if (mapItem.get(StringValue.bmp("contentFromURI")) != null) {

            otherStr = ((Item) checkMailProperty(context, mapItem, "contentFromURI", null, SequenceType.SINGLE_STRING)).getUnicodeStringValue().toString();
            if (otherStr != null) {
                URL uri;
                try {
                    uri = new URL(otherStr);
                } catch (MalformedURLException e) {
                    throw new XPathException("Error in attachment value cotnentFromURL: " + e.getMessage(), SaxonErrorCode.SXJE0051, context);
                }
                attachment.setDataHandler(new DataHandler(uri));
                return attachment;
            }

        }

        //TODO handle binary
        //otherStr = ((Item) checkMailProperty(context, mapItem, "binaryContent", null));
            /*if(otherStr != null){

                return attachment
            }*/
        if (mapItem.get(StringValue.bmp("stringContent")) != null) {
            otherStr = ((Item) checkMailProperty(context, mapItem, "stringContent", null, SequenceType.SINGLE_STRING)).getUnicodeStringValue().toString();

            if (otherStr != null) {
                attachment.setText(otherStr);
                return attachment;
            }
        } else if (mapItem.get(StringValue.bmp("contentFromString")) != null) {
            otherStr = ((Item) checkMailProperty(context, mapItem, "contentFromString", null, SequenceType.SINGLE_STRING)).getUnicodeStringValue().toString();

            if (otherStr != null) {
                attachment.setText(otherStr);
                return attachment;
            }

        }

        throw new XPathException("Invalid name/number of arguments  for attachment map", SaxonErrorCode.SXJE0051, context);


    }

    private String toStringOrNull(Sequence value) {
        try {
            if (value == null) {
                return null;
            }
            Item head = value.head();
            return head == null ? null : head.getStringValue();
        } catch (XPathException e) {
            return null;
        }
    }

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        MapItem emailConfigMap;
        String subjecti;
        Item contentItem;
        subjecti = arguments[1].head().getStringValue();
        contentItem = arguments[2].head();

        //Retrieve email configuration from map

        emailConfigMap = (MapItem) arguments[0].head();
        Map<String, Sequence> options =
                OPTION_DETAILS.processSuppliedOptions(emailConfigMap, context);

        int port;
        String subject = "";

        String server = toStringOrNull(options.get("smtp-server"));
        String sender = toStringOrNull(options.get("from"));
        String username = toStringOrNull(options.get("username"));
        String password = toStringOrNull(options.get("password"));
        String realName = toStringOrNull(options.get("realName"));
        GroundedValue toEmail = options.get("to").materialize();
        GroundedValue cc = options.get("cc").materialize();
        GroundedValue bcc = options.get("bcc").materialize();
        String portNumber = toStringOrNull(options.get("port"));
        try {
            port = portNumber == null ? 25 : Integer.parseInt(portNumber);
        } catch (NumberFormatException e) {
            throw new XPathException("saxon:send-mail: Invalid port number " + e.getMessage(), SaxonErrorCode.SXJM0001, context);
        }
        String htmlValue = toStringOrNull(options.get("html"));
        boolean htmlOption = "true".equals(htmlValue);

        boolean sslEnabled = options.get("ssl").head().effectiveBooleanValue();

        Properties mailProps = new Properties();
        mailProps.setProperty("mail.smtp.host", server);
        mailProps.setProperty("mail.smtp.port", port + "");
        mailProps.setProperty("mail.smtp.user", username);
        mailProps.setProperty("mail.smtp.auth", "true");
        mailProps.setProperty("mail.debug", "false");

        if (sslEnabled) {
            MailSSLSocketFactory sf = null;
            try {
                sf = new MailSSLSocketFactory();
            } catch (GeneralSecurityException e) {
                e.printStackTrace();
            }
            sf.setTrustAllHosts(true);
            mailProps.put("mail.smtp.ssl.trust", server);
            mailProps.put("mail.smtp.ssl.socketFactory", sf);
            mailProps.setProperty("mail.smtp.starttls.enable", "true");
            mailProps.setProperty("mail.smtp.socketFactory.port", port + "");
            mailProps.setProperty("mail.smtp.socketFactory.class", "javax.net.ssl.SSLSocketFactory");
            mailProps.setProperty("mail.smtp.socketFactory.fallback", "false");
        }


        for (KeyValuePair pair : emailConfigMap.keyValuePairs()) {
            if (pair.key.getStringValue().startsWith("mail.")) {
                Sequence val = pair.value;
                Item first = val.head();
                if (first != null) {
                    mailProps.setProperty(pair.key.getStringValue(), first.getStringValue());
                }
            }
        }

        Mailer2 mailer = new Mailer2(mailProps, password);
        try {
            mailer.setSender(sender, realName);
        } catch (Exception ex) {
            throw new XPathException("saxon:send-mail error in sender details: " + ex.getMessage(), SaxonErrorCode.SXJM0001, context);
        }

        mailer.addRecipients(Message.RecipientType.TO, createRecipients(toEmail, context));
        if (cc.getLength() > 0) {
            mailer.addRecipients(Message.RecipientType.CC, createRecipients(cc, context));
        }
        if (bcc.getLength() > 0) {
            mailer.addRecipients(Message.RecipientType.BCC, createRecipients(bcc, context));
        }

        if (subjecti != null) {
            subject = subjecti;
        }


        //Retrieve email attachment details from map
        if (arguments.length == 4) {
            Sequence attachments = arguments[3];
            SequenceIterator iter = attachments.iterate();
            for (Item item; (item = iter.next()) != null; ) {
                try {
                    mailer.addBodyPart(createAttachment(context, item));
                } catch (MessagingException e) {
                    throw new XPathException("Error in send-mail attachment(s): " + e.getMessage(), SaxonErrorCode.SXJM0002, context);
                }
            }

        }

        String content = "";

        if (contentItem instanceof StringValue) {
            content = contentItem.getStringValue();
        } else if (contentItem instanceof Item || htmlOption) {

            AxisIterator axisItr = ((NodeInfo) contentItem).iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT);
            NodeInfo nodei;
            while ((nodei = axisItr.next()) != null) {
                if (nodei.getLocalPart().equals("html")) {
                    htmlOption = true;
                }
            }

            StringWriter sw = new StringWriter();
            StreamResult sr = new StreamResult(sw);

            Properties props = new Properties();

            props.setProperty(OutputKeys.METHOD, "html");
            props.setProperty(OutputKeys.INDENT, "yes");

            try {
                QueryResult.serialize((NodeInfo) contentItem, sr, props);
            } catch (XPathException err) {
                throw new XPathException("Serialization failed: " + err.getMessage(), SaxonErrorCode.SXJM0001, context);
            }

            content = sw.toString();
        }

        try {
            mailer.sendMail(subject, content);
        } catch (Exception err) {
            throw new XPathException("Send-mail authentication failure: " + err.getMessage(), SaxonErrorCode.SXJM0002, context);
        }
        return BooleanValue.TRUE;
    }

}

// Copyright (c) 2012-2022 Saxonica Limited
