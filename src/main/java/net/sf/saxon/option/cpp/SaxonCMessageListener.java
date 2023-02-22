////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.*;

import javax.xml.transform.Source;
import javax.xml.transform.SourceLocator;
import java.io.*;
import java.util.ArrayList;
import java.util.List;
public class SaxonCMessageListener implements MessageListener2 {
    int mode = 0;
    String messageFilename = null;
    Source sourceFile = null;
    List<XdmNode> xslMessages = new ArrayList<>();
    Processor processor = null;

    public SaxonCMessageListener(Processor proc, String cwd, String mode) throws SaxonApiException {
        processor = proc;
        if (mode.equals("-:on")) {
            this.mode = 1;
        } else if (mode.equals("-:off")) {
            this.mode = 0;
        }  else {
            this.mode = 2;
            messageFilename = cwd + mode.substring(1);
            sourceFile = SaxonCAPI.resolveFileToSource(processor, cwd, messageFilename, ResourceRequest.XML_NATURE);
        }
    }
    @Override
    public void message(XdmNode content, QName errorCode, boolean terminate, SourceLocator locator) {
        if(mode == 0 ) {
            return;
        } else if (mode == 1) {
            xslMessages.add(content);
            System.err.println(content.getStringValue());
        } else if (mode == 2) {
            xslMessages.add(content);
            try (FileWriter fw = new FileWriter(new File(sourceFile.getSystemId()), true); //TODO change this to use Source
                 BufferedWriter bw = new BufferedWriter(fw);
                 PrintWriter out = new PrintWriter(bw)) {
                out.println(content.getStringValue());
            } catch (IOException e) {
                System.err.println("Error could not write message to file:" + messageFilename + " xsl:mesage=" + content.getStringValue());
            }
        }
    }
    /**
     * Get the xsl:messages  as an array of XdmNode objects
     * @return array of XdmNode objects
     */
    public XdmNode[] getXslMessages() {
        if(xslMessages == null) {
            return null;
        } else {
            return xslMessages.toArray(new XdmNode[xslMessages.size()]);
        }
    }
}