import java.io.FileReader;

import javax.xml.xquery.XQConnection;
import javax.xml.xquery.XQConstants;
import javax.xml.xquery.XQDataSource;
import javax.xml.xquery.XQExpression;

import com.saxonica.xqj.SaxonXQDataSource;

public class Run {

	public static void main(String[] args) throws Exception {
		
		XQDataSource ds = new SaxonXQDataSource();
        XQConnection conn = ds.getConnection();
        XQExpression exp = conn.createExpression();
        
        //OK - document validation, abstract types
        exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader("src/objDoc.xml"), null,conn.createDocumentType());
        exp.executeQuery(new FileReader("src/validateDoc.xq"));
        
        //OK - element validation, no abstract types
        exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader("src/concreteObj.xml"), null,conn.createDocumentType());
        exp.executeQuery(new FileReader("src/validateElem.xq"));
       
        //BAD - element validation, abstract types
        try {
	        exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader("src/obj.xml"), null,conn.createDocumentType());
	        exp.executeQuery(new FileReader("src/validateElem.xq"));
        } catch (Exception e) {
			e.printStackTrace();
		}

        //BAD - inside declaration, abstract types
        try {
        	exp.executeQuery(new FileReader("src/validateVar.xq"));
        } catch (Exception e) {
			e.printStackTrace();
		}
        
	}

}
