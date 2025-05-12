JParser
=======

The files in this directory are work in progress to do the following:

* Convert the Java source code (of Saxon) to an XML representation of the Java syntax using the open source JavaParser

* Convert the resulting XML to Java or C#

The conversion to Java is purely for testing, to check that it round trips; but it could potentially be used
 to modify the code in various ways, e.g. adding instrumentation or tracing, or for various kinds of preprocessing
 or annotation processing.
 
The conversion to C# is intended to explore the possibility of creating a .NET product by source code conversion.
At present the conversion is mechanistic, but the idea is to add rules that convert calls on Java classes to their
C# equivalents.

The simple Java program toXml.java converts one Java module to XML. The stylesheets xml-to-java.xsl and 
xml-to-csharp.xsl also appear to process a single module. 

The file javaparservisited.pdf is a paid-for manual for the javaparser product (the code is open source, the manual is
available for purchase.)

Further code for this project is on MHK's Mac laptop in IntelliJ project "javaparser".

NOTES

Things that will cause difficulty in conversion:

* Anonymous inner classes. Some have been converted by hand to named static inner classes,
which appear to work fine. We're attempting to automate this conversion, but it's difficult
to resolve methods/properties that implicitly refer to "this" in an inherited class, or in 
the outer class, or in a superclass of the outer class, or in an outer class of the outer
class, etc.

Now handling most things with the exception of references to local/variables parameters in the
containing method. Can't see any way to handle that. Also not handling a nesting depth for inner
classes > 1. Many uses of anonymous classes can be replaced by lambda expressions.

* Enums. Conversion code written but it's complex and not handling all cases.

* Default methods in interfaces. No equivalent in C#. We're expanding the default methods into
all the derived types.

* Constants and static methods in interfaces. Need to move them elsewhere.

* Static members in generic types. Notably Feature<X>. 

* Multiple variables with the same name: C# rules are more restrictive. Need to rename
variables to avoid the problem.

* Collection classes. Java foreach over an iterable maps nicely to C# foreach over an
IEnumerable. But the Iterator classes are fundamentally different: C# MoveNext() is stateful
Java hasNext() is not. Where we have an implementation of Iterator, we may need a parallel
implementation of IEnumerator. (The problem here is the close interaction of language features
with collection classes, which makes emulation difficult.)

* Diamond syntax: new HashMap<>(). We're inferring a type for simple cases where the object creation expression
is the RHS of an assignment or declaration.

* Covariant return types. We declaring the return type to be the same as the return type of the inherited method,
and generating a cast of the result on calls where it's needed.

* Predicates, lambda expressions. The rule seems to be that using lambda expressions for a simple functional interface
works fine (the FI is translated to a delegate), but you can't have classes that subclass such an interface. So NodeTest
implementing Predicate<> seems to be not possible, instead we'll try having NodeTest.asPredicate() to get the predicate
when needed.

* Ternary operators. C# has stricter rules about the types of the then/else branches being the same. I haven't found a way
to deal with this, so we simply have to avoid using ternary operators that break the rules. (We could consider expanding all
ternary expressions into if/then/else, but I think that's messy in general).

* Visibility/accessibility. Java allows a public method/class to override a private one; C# doesn't. Changing the code
to follow the C# rules.



MHK 2021-01-26