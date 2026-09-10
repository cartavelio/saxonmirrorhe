package net.sf.saxon;

import java.lang.annotation.*;

/**
 * Annotation for methods to indicate that the method is virtual (can be overridden in a subclass).
 * This of course has no effect on the Java compiler, but it is used when generating C# code, which
 * require virtual methods to be marked as such.
 */

@Target( {ElementType.METHOD} )
@Retention( RetentionPolicy.SOURCE)
@Inherited
public @interface Virtual {
}
