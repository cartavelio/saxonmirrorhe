package net.sf.saxon.transpile;
import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * This annotation appears on the declation of a method that creates an instance of
 * an anonymous inner class, and it is used to define what information needs to be
 * passed to the inner class.
 *
 * The parameter {@code outer} indicates whether a reference to the outer class ("this")
 * should be passed. This defaults to true and should be set to false only if the
 * containing method is non-static.
 *
 * The parameter {@code extra} is multi-valued, and contains one entry for each extra
 * parameter to be passed through. Parameters already present in the constructor of the
 * inner class are included automatically.
 */

@Retention(RetentionPolicy.SOURCE)
@Target(ElementType.METHOD)
public @interface CSharpInnerClass {
    boolean outer() default false;
    String[] extra() default {};
}
