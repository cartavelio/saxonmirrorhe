import java.io.FileReader;

import javax.xml.xquery.XQConnection;
import javax.xml.xquery.XQConstants;
import javax.xml.xquery.XQDataSource;
import javax.xml.xquery.XQExpression;
import javax.xml.xquery.XQResultSequence;

import com.saxonica.xqj.SaxonXQDataSource;

public class Run {

	public static void main(String[] args) throws Exception {
			
		XQDataSource ds = new SaxonXQDataSource();
        XQConnection conn = ds.getConnection();
        XQExpression exp = conn.createExpression();
        
        
        exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader("src/obj.xml"), null,conn.createDocumentType());
        XQResultSequence result =  exp.executeQuery(new FileReader("src/validateDoc.xq"));
        
        
        if (result.next()) {
        	System.out.println( result.getAtomicValue() );
        }
	}

}
