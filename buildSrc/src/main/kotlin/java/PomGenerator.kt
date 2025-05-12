package com.saxonica.build.java

import org.gradle.api.Action
import org.gradle.kotlin.dsl.assign
import org.gradle.api.artifacts.DependencySet
import org.gradle.api.publish.maven.MavenPom
import org.gradle.api.publish.maven.MavenPomLicense
import org.gradle.api.publish.maven.MavenPomScm


data class SaxonicaDeveloper(val id: String, val name: String, val email: String)

class PomGenerator(val configuration: Action<PomGenerator>) {
  companion object {
    private fun genLicense(action: Action<MavenPomLicense>): Action<MavenPomLicense> {
      return action
    }

    val COMMERCIAL_LICENSE: Action<MavenPomLicense> = genLicense{
        name = "Saxon Commercial License"
        url = "https://www.saxonica.com/"
        distribution = "jar"
    }
  }

  val developers: List<SaxonicaDeveloper> = listOf(
    SaxonicaDeveloper("mike","Michael Kay", "mike@saxonica.com"),
    SaxonicaDeveloper("ond1","O’Neil Delpratt", "oneil@saxonica.com"),
    SaxonicaDeveloper("debbie","Debbie Lockett", "debbie@saxonica.com"),
    SaxonicaDeveloper("norm","Norman Walsh", "norm@saxonica.com"),
    SaxonicaDeveloper("matt","Matt Patterson", "matt@saxonica.com"),
  )

  var name: String = ""
  var dependencies: DependencySet? = null

  private var licenseAction: Action<MavenPomLicense>? = null
  fun license(action: Action<MavenPomLicense>) {
    licenseAction = action
  }

  private var scmAction: Action<MavenPomScm>? = null
  fun scm(action: Action<MavenPomScm>) {
    scmAction = action
  }


  fun generate(): Action<MavenPom> {
    configuration.execute(this)
    val generator = this

    return action {
      name = generator.name
      packaging = "jar"
      description = "The XSLT and XQuery Processor"
      url = "http://www.saxonica.com/"
      inceptionYear = "1999"

      organization {
        name = "Saxonica"
        url = "https://www.saxonica.com/"
      }
      if (generator.scmAction != null) {
        scm(generator.scmAction)
      }
      licenses {
        license(generator.licenseAction)
      }
      developers {
        generator.developers.forEach {
          developer {
            id = it.id
            name = it.name
            email = it.email
          }
        }
      }
      issueManagement {
        system = "Saxon Community"
        url = "https://saxonica.plan.io/projects/saxon"
      }

      mailingLists {
        mailingList {
          name = "User List"
          post = "saxon-help@lists.sourceforge.net"
          archive = "http://sourceforge.net/mailarchive/forum.php?forum_name=saxon-help"
        }
      }

      withXml {
        val prop = asNode().appendNode("properties")
        prop.appendNode("skipTests", "false")
        prop.appendNode("maven.build.timestamp.format", "yyyy-MM-dd'T'HH:mm:ss")
        prop.appendNode("project.build.sourceEncoding", "UTF-8")
        prop.appendNode("maven.javadoc.skip", true)
        prop.appendNode("maven.javadoc.failOnError", false)
//          prop.appendNode("maven.compiler.source", javaVersion)
//          prop.appendNode("maven.compiler.target", javaVersion)

        val dependenciesNode = asNode().appendNode("dependencies")

        var resolverProcessed = false
        generator.dependencies?.forEach {
          val dependencyNode = dependenciesNode.appendNode("dependency")
          dependencyNode.appendNode("groupId", it.group)
          dependencyNode.appendNode("artifactId", it.name)
          dependencyNode.appendNode("version", it.version)
          // Special case the data classifier
          if (it.group == "org.xmlresolver" && it.name == "xmlresolver") {
            if (resolverProcessed) {
              dependencyNode.appendNode("classifier", "data")
            }
            resolverProcessed = true
          } else {
            dependencyNode.appendNode("optional", "true")
          }
        }
      }
    }
  }

  private fun action(action: Action<MavenPom>): Action<MavenPom> {
    return action
  }
}
