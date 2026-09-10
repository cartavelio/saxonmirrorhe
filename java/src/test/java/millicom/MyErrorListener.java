package millicom;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Initializer;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;


public class MyErrorListener implements ErrorListener, Initializer {

    private final XMLStreamWriter writer;
    private int warningCount = 0;
    private int errorCount = 0;

    public MyErrorListener(XMLStreamWriter writer)  {
         this.writer = writer;
    }


    public int getErrorCount(){
        return errorCount;
    }

    public int getWarningCount(){
        return warningCount;
    }

    @Override
    public void error(TransformerException exception) throws TransformerException {
        errorCount++;
        try {
            writer.writeStartElement("error");
            writer.writeAttribute("line", String.valueOf(exception.getLocator().getLineNumber()));
            writer.writeAttribute("position", String.valueOf(exception.getLocator().getColumnNumber()));
            writer.writeCharacters(exception.getMessage());
            writer.writeEndElement();
        } catch (XMLStreamException e) {
            throw new TransformerException(e);
        }

    }

    @Override
    public void fatalError(TransformerException exception) throws TransformerException {
        try {
            writer.writeStartElement("fatal");
            writer.writeAttribute("line", String.valueOf(exception.getLocator().getLineNumber()));
            writer.writeAttribute("position", String.valueOf(exception.getLocator().getColumnNumber()));
            writer.writeCharacters(exception.getMessage());
            writer.writeEndElement();
        } catch (XMLStreamException e) {
            throw new TransformerException(e);
        }
    }

    @Override
    public void warning(TransformerException exception) throws TransformerException {
        warningCount++;
        try {
            writer.writeStartElement("warning");
            writer.writeAttribute("line", String.valueOf(exception.getLocator().getLineNumber()));
            writer.writeAttribute("position", String.valueOf(exception.getLocator().getColumnNumber()));
            writer.writeCharacters(exception.getMessage());
            writer.writeEndElement();
        } catch (XMLStreamException e) {
            throw new TransformerException(e);
        }
    }

    @Override
    public void initialize(Configuration config) throws TransformerException {
        //config.setErrorListener(this);
    }
}
