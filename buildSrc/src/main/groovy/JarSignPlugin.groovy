import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.internal.os.OperatingSystem;

class JarSignPluginExtension {
  String alias = "THIS-IS-NOT-THE-ALIAS"
  String password = "THIS-IS-NOT-THE-PASSWORD"
  String tokenfile = "THIS-IS-NOT-THE-ETOKEN"
  ArrayList signer = ["jarsigner"]
  boolean signJars = false

  public boolean getCanSignJars() {
    if (signer[0] == "echo") {
      return true
    }

    return !(alias.startsWith("THIS-IS-NOT")
             || password.startsWith("THIS-IS-NOT")
             || tokenfile.startsWith("THIS-IS-NOT"))
  }

  public boolean getMakeSignedJars() {
    if (signJars && !getCanSignJars()) {
      throw new GradleException("Build is not configured to sign jars.")
    }
    return signJars
  }    

  public ArrayList signJarCommand(String jarfile) {
    ArrayList command = signer
    if (OperatingSystem.current().isWindows()) {
      command += ["-keystore", "NONE",
                  "-storetype", "PKCS11",
                  "-tsa", "http://rfc3161timestamp.globalsign.com/advanced",
                  "-providerClass", "sun.security.pkcs11.SunPKCS11",
                  "-providerArg", tokenfile]
    } else {
      command += ["-tsa", "http://rfc3161timestamp.globalsign.com/advanced",
                  "-storetype", "PKCS11",
                  "-providerClass", "sun.security.pkcs11.SunPKCS11",
                  "-providerArg", tokenfile]
    }

    return command += [jarfile, alias, "-storepass", password]
  }
}

public class JarSignPlugin implements Plugin<Project> {
  void apply(Project project) {
    def extension = project.extensions.create("jarsigner", JarSignPluginExtension)

    String prop = project.findProperty("signAlias") ?: System.getenv("SAXON_SIGN_ALIAS")
    if (prop != null) {
      extension.alias = prop
    }

    prop = project.findProperty("signPassword") ?: System.getenv("SAXON_SIGN_PASSWORD")
    if (prop != null) {
      extension.password = prop
    }

    prop = project.findProperty("signTokenFile") ?: System.getenv("SAXON_SIGN_ETOKEN")
    if (prop != null) {
      extension.tokenfile = prop
      File etoken = new File(prop)
      if (!etoken.exists()) {
        println("Warning: signing token file does not exist: ${prop}")
      }
    }

    if (project.findProperty("testSignJars") == "true") {
      extension.signer = ["echo", "JARSIGNER"]
    }
    
    extension.signJars = (project.findProperty("signJars") == "true")
  }
}
