import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.internal.os.OperatingSystem;

class JarSignPluginExtension {
  String alias = "THIS-IS-NOT-THE-ALIAS"
  String password = "THIS-IS-NOT-THE-PASSWORD"
  String keystore = "THIS-IS-NOT-THE-KEYSTORE"
  ArrayList jarSigner = ["jarsigner"]
  ArrayList exeSigner = ["signtool"]
  boolean signJars = false
  boolean signExes = false

  public boolean getCanSignJars() {
    if (jarSigner[0] == "echo") {
      return true
    }

    return !(alias.startsWith("THIS-IS-NOT")
             || password.startsWith("THIS-IS-NOT")
             || keystore.startsWith("THIS-IS-NOT"))
  }

  public boolean getCanSignExes() {
    if (exeSigner[0] == "echo") {
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

  public boolean getMakeSignedExes() {
    if (signExes && !getCanSignExes()) {
      throw new GradleException("Build is not configured to sign executables.")
    }
    return signExes
  }    

  public ArrayList signJarCommand(String jarfile) {
    ArrayList command = jarSigner
    command += ["-storetype", "PKCS12",
                "-keystore", keystore,
                "-tsa", "http://rfc3161timestamp.globalsign.com/advanced"]
    return command += [jarfile, alias, "-storepass", password]
  }

  public ArrayList signExeCommand(String exefile) {
    ArrayList command = exeSigner
    command += ["sign", "/fd", "sha1",
                "/f", keystore,
                "/t", "http://timestamp.digicert.com",
                "/p", password,
                exefile]
    return command
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
      extension.jarSigner = ["echo", "JARSIGNER"]
    }

    prop = project.findProperty("saxonSignJars") ?: System.getenv("SAXON_SIGN_JARS")
    extension.signJars = (prop == "true")

    if (project.findProperty("testSignExes") == "true") {
      extension.exeSigner = ["echo", "EXESIGNER"]
    }

    prop = project.findProperty("saxonSignExes") ?: System.getenv("SAXON_SIGN_EXES")

    extension.signExes = (prop == "true")

    prop = project.findProperty("windowsSignTool") ?: System.getenv("WINDOWS_SIGN_TOOL")

    if (prop != null) {
      extension.exeSigner = [prop]
    }

  }
}
