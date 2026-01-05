package com.saxonica.build

import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import org.gradle.api.provider.Property
import java.io.Serializable

/**
 * Create a wrapper around a class serializable using kotlinx.serialization that can be used as an @Input property
 * for a gradle task:
 *
 *    @get:Input
 *    val prop = AsInput.from<MyKotlinxSerializableClass>(instance)
 *    instance = prop.get<MyKotlinxSerializableClass>()
 */
class AsInput<T>(var inputSerialization: String) : Serializable {
  companion object {
    private const val serialVersionUID = 1L

    inline fun <reified T> from(input: T): AsInput<T> {
      return AsInput<T>(Json.Default.encodeToString(input))
    }

    inline fun <reified T> from(property: Property<T & Any>): AsInput<T> {
      @Suppress("UNCHECKED_CAST")
      return property.map { this.from(it) }.get() as AsInput<T>
    }
  }

  inline fun <reified T> get(): T {
    return Json.Default.decodeFromString<T>(inputSerialization)
  }
}