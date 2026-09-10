package com.saxonica.functions.extfn;

import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;

import javax.mail.*;
import javax.mail.internet.InternetAddress;
import javax.mail.internet.MimeBodyPart;
import javax.mail.internet.MimeMessage;
import javax.mail.internet.MimeMultipart;
import java.io.UnsupportedEncodingException;
import java.util.Date;
import java.util.Properties;


/**
 * Helper class for saxon:send-mail() extension function
 */
public class Mailer2 {

    Session session;
    Address sender;
    String server;
    String user;
    String password;
    MimeMessage message;
    Multipart multipart;

    public Mailer2(String server, int port, String user, String password) {
        this.server = server;
        this.user = user;
        this.password = password;

        Properties props = new Properties();
        props.setProperty("mail.smtp.host", server);
        props.setProperty("mail.smtp.port", port + "");
        props.setProperty("mail.smtp.user", user);
        props.setProperty("mail.smtp.auth", "true");
        props.setProperty("mail.debug", "false");

        session = Session.getDefaultInstance(props, new MyAuthenticator());
        session.setDebug(false);
        message = new MimeMessage(session);
        multipart = new MimeMultipart();
    }

    public Mailer2(Properties props, String password) {
        this.server = props.getProperty("mail.smtp.host");
        this.user = props.getProperty("mail.smtp.user");
        this.password = password;

        props.setProperty("mail.smtp.auth", "true");
        props.setProperty("mail.debug", "false");

        session = Session.getDefaultInstance(props, new MyAuthenticator());
        session.setDebug(false);
        message = new MimeMessage(session);
        multipart = new MimeMultipart();
    }

    public void setSender(String emailAddress, String realName)
            throws MessagingException, UnsupportedEncodingException {
        if (realName == null) {
            sender = new InternetAddress(emailAddress);
        } else {
            sender = new InternetAddress(emailAddress, realName);
        }
        message.setFrom(sender);
        Address[] replyTo = {sender};
        message.setReplyTo(replyTo);
    }

    public void addRecipients(Message.RecipientType recipientType, Address[] addresses) throws XPathException {
        try {
            message.addRecipients(recipientType, addresses);
        } catch (MessagingException e) {
            throw new XPathException("Invalid recipient in send-mail:" + e.getMessage(), SaxonErrorCode.SXJM0002);
        }
    }

    public void addBodyPart(BodyPart part) throws XPathException {
        try {
            multipart.addBodyPart(part);
        } catch (MessagingException e) {
            throw new XPathException("Invalid content of email:" + e.getMessage(), SaxonErrorCode.SXJM0002);
        }
    }

    public void sendMail(String subject, String content) throws MessagingException {
        message.setSubject(subject);
        message.setSentDate(new Date());

        //message.addRecipients(Message.RecipientType.TO, (InternetAddress[])torecipient.toArray());

        MimeBodyPart body = new MimeBodyPart();
        if (content.contains("<html") || content.contains("<xhtml")) {
            body.setText(content, "UTF-8", "html");
        } else {
            body.setText(content);
        }
        multipart.addBodyPart(body);

        /* if (attachments != null && attachments.size() > 0) {
            for (int i=0; i<attachments.size(); i++) {
                BodyPart attachment = new MimeBodyPart();
                String filename = (String)attachments.get(i);
                DataSource source = new FileDataSource(filename);
                attachment.setDataHandler(new DataHandler(source));
                int sep = filename.length()-1;
                while (filename.charAt(sep) != '/' && filename.charAt(sep) != '\\') {
                    sep--;
                }
                if (attachments.size() > 1) {
                    sep--;
                    while (filename.charAt(sep) != '/' && filename.charAt(sep) != '\\') {
                        sep--;
                    }
                }
                filename = filename.substring(sep+1);
                attachment.setFileName(filename);
                multipart.addBodyPart(attachment);
            }
        }*/

        message.setContent(multipart);
        Transport transport = session.getTransport("smtp");
        transport.connect(server, user, password);
        transport.sendMessage(message, message.getAllRecipients());
        transport.close();
    }

    private class MyAuthenticator extends Authenticator {
        @Override
        protected PasswordAuthentication getPasswordAuthentication() {
            //System.err.println("Callback to password authentication");
            return new PasswordAuthentication(user, password);
        }
    }

}

// Copyright (c) 2012-2022 Saxonica Limited

