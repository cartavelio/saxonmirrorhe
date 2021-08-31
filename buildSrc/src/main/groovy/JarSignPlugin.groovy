import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.internal.os.OperatingSystem;

class JarSignPluginExtension {
  String alias = "THIS-IS-NOT-THE-ALIAS"
  String password = "THIS-IS-NOT-THE-PASSWORD"
  String keystore = "THIS-IS-NOT-THE-KEYSTORE"
  ArrayList signer = ["jarsigner"]
  boolean signJars = false

  public boolean getCanSignJars() {
    if (signer[0] == "echo") {
      return true
    }

    return !(alias.startsWith("THIS-IS-NOT")
             || password.startsWith("THIS-IS-NOT")
             || keystore.startsWith("THIS-IS-NOT"))
  }

  public boolean getMakeSignedJars() {
    if (signJars && !getCanSignJars()) {
      throw new GradleException("Build is not configured to sign jars.")
    }
    return signJars
  }    

  public ArrayList signJarCommand(String jarfile) {
    ArrayList command = signer
    command += ["-storetype", "PKCS12",
                "-keystore", keystore,
                "-tsa", "http://rfc3161timestamp.globalsign.com/advanced"]
    return command += [jarfile, alias, "-storepass", password]
  }
}

public class JarSignPlugin implements Plugin<Project> {
  void apply(Project project) {
    def extension = project.extensions.create("jarsigner", JarSignPluginExtension)

    String prop = project.findProperty("saxonSignAlias") ?: System.getenv("SAXON_SIGN_ALIAS")
    if (prop != null) {
      extension.alias = prop
    }

    prop = project.findProperty("saxonSignPassword") ?: System.getenv("SAXON_SIGN_PASSWORD")
    if (prop != null) {
      extension.password = prop
    }

    prop = project.findProperty("saxonSignKeystore") ?: System.getenv("SAXON_SIGN_KEYSTORE")
    if (prop != null) {
      extension.keystore = prop
      File kstore = new File(prop)
      if (!kstore.exists()) {
        println("Warning: signing keystore file does not exist: ${prop}")
      }
    }

    if (project.findProperty("testSignJars") == "true") {
      extension.signer = ["echo", "JARSIGNER"]
    }

    prop = project.findProperty("saxonSignJars") ?: System.getenv("SAXON_SIGN_JARS")
    extension.signJars = (prop == "true")
  }
}
