package net.sf.saxon.transpile;
import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * Annotate parameter definition of a method declaration to give a type that will be used for
 * the parameter in the C# product, in place of the (usual conversion of) the declared Java type
 */

@Retention(RetentionPolicy.SOURCE)
@Target({ElementType.PARAMETER, ElementType.FIELD})

public @interface CSharpParameterType {
    String key() default "";
}
