import com.github.javaparser.StaticJavaParser;
import com.github.javaparser.ast.CompilationUnit;
import com.github.javaparser.printer.XmlPrinter;

import java.io.File;
import java.io.FileNotFoundException;

public class ToXml {


    public static void main(String[] args) {
        try {
            // Parse the code you want to inspect:
            CompilationUnit cu = StaticJavaParser.parse(new File("/Users/mike/repo/src/he/java/net/sf/saxon/Configuration.java"));
            // Now comes the inspection code:
            XmlPrinter printer = new XmlPrinter(true);
            System.out.println(printer.output(cu));
        } catch (FileNotFoundException e) {
            System.err.println(e.getMessage());
        }
    }
    
}

