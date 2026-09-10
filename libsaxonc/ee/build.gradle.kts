plugins {
    id("saxon-build")
    id("com.saxonica.build.managebranches")
    id("saxonc-libsaxonc")
}

val doxygenOutput by configurations.consumable("doxygenOutput")
doxygenOutput.outgoing.artifact(tasks.named("doxygen"))

libsaxonc {
    edition = "ee"
}
