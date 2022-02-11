package net.sf.saxon.transpile;
import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * Annotate a Java enum to indicate that a simple mapping to a C# enum should be used.
 * This mapping is used essentially for enum's that consist of nothing more that a set of
 * enumeration constants.
 */

@Retention(RetentionPolicy.SOURCE)
@Target(ElementType.TYPE)

public @interface CSharpSimpleEnum {

}
