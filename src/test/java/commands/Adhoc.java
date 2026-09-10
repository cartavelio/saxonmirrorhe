package commands;

import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.StringReader;

public class Adhoc {

    public static void main(String[] args) throws Exception {
        String XML = fetchDataFrmXML("/Users/mike/Desktop/temp/test.xml");
        Transformer t = TransformerFactory.newInstance().newTransformer(new StreamSource(new File("/Users/mike/Desktop/temp/test.xsl")));
        System.err.println(t.getClass().getName());
        t.transform(new StreamSource(new StringReader(XML)), new StreamResult(new File("/Users/mike/Desktop/temp/test.out")));
    }

    public static String fetchDataFrmXML(String fileLocation) throws Exception {
        File file = new File(fileLocation);
        FileReader fr = new FileReader(file);
        BufferedReader br = new BufferedReader(fr);
        String temp;
        String result = "";
        while ((temp = br.readLine()) != null) {
            result += temp;
        }

        br.close();
        return result;
    }
}

//
// The contents of this file are subject to the Mozilla Public License Version 1.0 (the "License");
// you may not use this file except in compliance with the License. You may obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Saxonica Limited
//
// Portions created by (your name) are Copyright (C) (your legal entity). All Rights Reserved.
//
// Contributor(s): none.
//
