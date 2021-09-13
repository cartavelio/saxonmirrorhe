import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.internal.os.OperatingSystem;

class JarSignPluginExtension {
  String alias = "THIS-IS-NOT-THE-ALIAS"
  String password = "THIS-IS-NOT-THE-PASSWORD"
  String keystore = "THIS-IS-NOT-THE-KEYSTORE"
  ArrayList signer = ["jarsigner"]
  ArrayList signtool = []
  boolean signJars = false
  boolean signInstallers = false

  public boolean getCanSignJars() {
    if (signer[0] == "echo") {
      return true
    }

    return !(alias.startsWith("THIS-IS-NOT")
             || password.startsWith("THIS-IS-NOT")
             || keystore.startsWith("THIS-IS-NOT"))
  }

  public boolean getCanSignInstallers() {
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

  public boolean getMakeSignedInstallers() {
    if (signInstallers && !getCanSignInstallers()) {
      throw new GradleException("Build is not configured to sign .NET installers.")
    }
    return signInstallers
  }    

  public ArrayList signJarCommand(String jarfile) {
    ArrayList command = signer
    command += ["-storetype", "PKCS12",
                "-keystore", keystore,
                "-tsa", "http://rfc3161timestamp.globalsign.com/advanced"]
    return command += [jarfile, alias, "-storepass", password]
  }

  public ArrayList signtoolCommand(String exefile) {
    ArrayList command = signtool
    command += ["sign", "/f", keystore,
                "/p", password,
                "/t", "http://timestamp.digicert.com"]
    return command += [exefile]
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
      File kstore = new File(prop)
      if (kstore.exists()) {
        extension.keystore = prop
      } else {
        println("Warning: signing keystore file does not exist: ${prop}")
      }
    }

    if (project.findProperty("testSignJars") == "true") {
      extension.signer = ["echo", "JARSIGNER"]
    }

    prop = project.findProperty("saxonSignJars") ?: System.getenv("SAXON_SIGN_JARS")
    extension.signJars = (prop == "true")

    prop = project.findProperty("saxonSignInstallers") ?: System.getenv("SAXON_SIGN_INSTALLERS")
    extension.signInstallers = (prop == "true")

    prop = project.findProperty("saxonSignTool") ?: System.getenv("SAXON_SIGNTOOL")
    if (prop == null) {
      extension.signInstallers = false
    } else {
      extension.signtool = [prop]
    }

    if (project.findProperty("testSignInstaller") == "true") {
      extension.signtool = ["echo", "SIGNTOOL"]
    }
  }
}
