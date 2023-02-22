# Writing Java code that converts to C#

The SaxonCS product is generated from the Saxon-J source code by an automated conversion process. This document describes the rules you need to follow when writing Java to make sure that the conversion is successful.

## Naming of variables

C# has stricter rules than Java on reusing names. The converter doesn't fix any naming conflicts; you need to follow the rules in your Java code. The main rules are:


- Don't use the same name for a variable and a method

  `int next = next();`

- Don't have two variables with the same name and overlapping scope:


```
      if (1==2) {
         int x = 3;
         process(x);
      }
      int x = 4;
      process(x);
```
## Ternary Operator
 
C# requires the two branches of a ternary conditional to have matching types. This means you can't do:
```
NumericValue n = condition ? new DoubleValue(3) : new FloatValue(3);
```
The converter can't sort this out for you (its type inferencing isn't up to the job). You need either to write it out as an "if" statement, or to insert a cast:

```
NumericValue n = condition ? (NumericValue)new DoubleValue(3) : (NumericValue)new FloatValue(3);
```
## Fallthrough in switch statements

C# doesn't allow fallthrough: you can't do

```
case '\n':
    incrementLineNumber();
    // fallthrough
case '\t'
case '\s':
    processWhitespace();
```
For this to generate correct C#, you need to write:
```
case '\n':
    incrementLineNumber();
    // fallthrough
    CSharp.emitCode("goto case '\t'");    
case '\t'
case '\s':
    processWhitespace();
```
The method `CSharp.emitCode()` is a no-op when executed, and hopefully gets optimised away; but it is recognised by the converter and injects the value of the argument directly into the C# output. So far, this is the only situation in which I have used it. The argument must be a string literal.

*(Note, we can't use preprocessing directives here, because any code subject to preprocessing directives needs to be valid Java so the IDE editor doesn't redline it.)*

Also note: the last branch in a switch statement (usually the default branch) needs to end with a `break` or `return` statement. You can't simply fall off the end as in Java. Simply add the additional statement; Java doesn't mind.

##Covariant return types

Java allows an interface to declare a method 
```
Item next(); 
```
which a subclass can implement as 
```
AtomicValue next() { return ....}
```
C# doesn't allow this. The converter, however, tries to take care of this. It will convert the method on the subclass to:
```
Item next() { return ....}
```
and it will attempt to add a cast to all calls on the method, so they become
```
AtomicValue nextValue = (AtomicValue)next();
```
However, detection of calls on the method isn't 100% reliable, especially if there are multiple overloads of the method with different argument types. You may need to insert a cast manually on occasions.

## Method Inheritance

C# doesn't allow you to inherit from two different interfaces that declare the same method, even if the declarations are identical. For example, the types `SchemaType` and `ItemType` have shared subclasses but no shared superclass, so they cannot both define the method `getTypeName()`. (Actually C# does allow this, but you have to distinguish the two declarations when you call the method).

To avoid this problem, put the common method `getTypeName()` in its own interface, and have both `SchemaType` and `ItemType` extend this common interface.

Some other inheritance issues that you shouldn't need to worry about, because the converter should take care of them:


- Default methods defined in interfaces are copied into the code of all classes that implement the interface. (So keep them simple.)
- An abstract class needs to contain a (concrete) implementation of every method defined in any interface that the abstract class implements (no idea why!). The converter generates a suitable dummy implementation.
- The rules for using the `override` modifier are a little different from the rules for `@Override` in Java. The converter should sort this out for you.
- In C#, methods that are overridden need to be declared `virtual`. The converter inserts this modifier automatically.

C# has stricter rules on visibility/access than Java, for example a public method can't override a protected method. The converter isn't currently resolving this; change the Java code to follow the C# rules.

C# doesn't have a direct equivalent to Java's "default visibility", i.e. when methods have no `private | public | protected` modifier. Again, the cconverter isn't handling this (purely because it's not something we do much). Always use explicit qualifiers.

## Inner classes

C# is much more restrictive than Java in the kind of inner classes that it allows. In C# they must always have a name (anonymous inner classes are not allowed), and they are always effectively static (the inner class can't refer to instance-level data in the outer class).

Because we use inner classes a lot, the converter makes strenuous efforts to generate a C# equivalent, but it can't handle all cases:


- Anonymous inner classes become named inner classes (the converter generates a name).
- Non-static inner classes are passed an extra parameter set to `this`, which is bound to a variable in the inner class called `outerInstance`, so the inner class can refer to data in the outer class via this variable.
- The converter analyses all unqualified names used in the inner class to try to establish whether they refer to local variables in the containing method, local fields/methods in the inner class, or local fields/methods in the outer class; names in the last category are qualified by `outerInstance`.
- The construct `OuterClass.this` is also handled.

But there are restrictions:

- You can only have one level of nesting (inner classes can't have inner classes).
- The inner class can't refer to local variables (as distinct from local fields) in the outer class. If you want to do this, you're probably better off using a lambda expression.

## Generics

Java and C# use similar syntax for generics, and in many simple cases the same construct means the same thing, but the underlying model is very different, so many things don't convert easily.

Straightforward use of built-in generic types like `List` and `Map` usually converts without any problem. The main thing to remember is that C# is a bit stricter about enforcement. You can't get away with some of the hacks available in Java, like casting a `List<AtomicValue>` to a plain unparameterised `List`, or (worse) to a `List<Item>`. Careless use of generics needs to be sorted out.

Use of `instance of` (which becomes `is` in C#) doesn't work the way a Java programmer would expect. For example in the Java code we use `item instanceof ObjectValue` to test if something is an external wrapped Java object. But in C#, `ObjectValue<SQLConnection>` and `ObjectValue<Map<string, integer>>` are completely different types, and in both cases `x is ObjectValue` will return false (or fail to compile).

Java is friendlier than C# in allowing the diamond operator, for example `Map<A,B> m = new HashMap<>()`, where the type of the `HashMap` is inferred from the context. The converter tries to sort this out when it can; the type parameters are inferred in cases where:

- an instantiation appears as part of a return clause (`return new HashMap<>()`)
- an instantiation appears as part of a variable declaration (`Map<X,Y> m = new HashMap<>()`)
- TBA

But this is work in progress, it's not always getting it right, but will be improved.

As a general principle, we're no longer using generics for our own code that handles XDM sequences or sequence iterators. I've blogged about the reasons for that: basically, most of the time, we don't have any compile-time knowledge of the type of items that a particular construct will be handling. This makes the conversion to C# a lot simpler. I'm still having problems where XDM sequences interact with generic Java collection types, for example where some code constructs a `List<NodeInfo>` and then wants to pass this to a constructor for a `SequenceExtent` that expects `List<? extends Item>`.

## Lambda Expressions

Lambda expressions (such as `item -> item.name()` in Java, `item => item.name()` in C#) translate quite easily between the two languages, on the whole. The difference lies in where they can be used.

Java is much more liberal in allowing lambda expressions in a wide range of contexts: basically, anywhere where the required type is a function interface. A functional interface is any class that defines a single method; it doesn't actually need to be annotated with @FunctionalInterface. So you can do, for example,

```
transform.setURIResolver((base, href) -> return emptySource());
```
Even though the `URIResolver` interface was designed long before lambda expressions entered the language. You can also, of course, write your own class that implements `URIResolver` and pass an instance of that class instead.

In C# you can only use a lambda expression where the expected type is a `delegate`, and if it's defined as a `delegate`, then you can't write a concrete class that implements it. In effect this means that if an expression *can* be supplied as a lambda expression, then it *must* be supplied as a lambda expression.

We have a number of interfaces where we rely on the ability to supply either a predicate or a subclass; an example is `NodeInfo.iterateAxis(axis, condition)` where the condition can be either a predicate supplied as a lambda expression, or a class that implements `NodeTest`. I'm still working on the best approach to converting these interfaces; one approach (used in this case) is for the C# code to provide two methods, `NodeInfo.iterateAxis(axis, NodeTest)`, and `NodeInfo.iterateAxis(axis, Predicate<NodeInfo>)` so that both kinds of call continue to work.
