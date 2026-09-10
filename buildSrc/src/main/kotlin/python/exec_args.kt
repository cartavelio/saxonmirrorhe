package com.saxonica.build.python

import org.gradle.api.provider.Provider

interface ArgStringifier {
  fun argToString(arg: Any): String {
    return if (arg is Provider<*>) {
      arg.get()
    } else {
      arg
    }.toString()
  }
}

class StringArg(private var argHolder: Any? = null) : ArgStringifier {
  fun set(arg: String) {
    argHolder = arg
  }

  fun set(argProvider: Provider<*>) {
    argHolder = argProvider
  }

  fun get(): String {
    return argHolder?.let { argToString(it) }.orEmpty()
  }

  val isPresent: Boolean get() = argHolder?.let { true } ?: false
}

class ListArgs(private val args: MutableList<Any> = mutableListOf()) : ArgStringifier {
  fun add(arg: String) {
    args.add(arg)
  }

  fun add(argProvider: Provider<*>) {
    args.add(argProvider)
  }

  fun add(addedArgs: Iterable<Any>) {
    addedArgs.forEach { arg ->
      if (arg is String) {
        add(arg)
      } else if (arg is Provider<*>) {
        add(arg)
      } else {
        add(arg.toString())
      }
    }
  }

  fun add(vararg addedArgs: Any) {
    add(addedArgs.toList())
  }

  fun set(newArgs: Iterable<Any>) {
    args.clear()
    add(newArgs)
  }

  fun get(): List<String> {
    return args.map { argToString(it) }
  }
}

typealias ArgPair = Pair<String, Any>
class MapArgs(private val args: MutableMap<String, Any> = mutableMapOf()) : ArgStringifier {
  fun put(name: String, value: String) {
    args[name] = value
  }

  fun put(name: String, value: Provider<*>) {
    args[name] = value
  }

  fun put(entry: ArgPair) {
    val (name, value) = entry

    if (value is String) {
      put(name, value)
    } else if (value is Provider<*>) {
      put(name, value)
    } else {
      put(name, value.toString())
    }
  }

  fun put(addedArgs: Iterable<ArgPair>) {
    addedArgs.forEach { put(it) }
  }

  fun put(addedArgs: Map<String, Any>) {
    put(addedArgs.map { (name, value) -> name to value })
  }

  fun put(vararg addedArgs: ArgPair) {
    put(addedArgs.toList())
  }

  fun set(newArgs: Map<String, Any>) {
    args.clear()
    put(newArgs)
  }

  fun get(): Map<String, String> {
    return args.map { (name, value) -> name to argToString(value) }.toMap()
  }

  fun isEmpty(): Boolean {
    return args.isEmpty()
  }
}
