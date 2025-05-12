package com.saxonica.build.python

import org.gradle.api.DefaultTask
import org.gradle.api.Task
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.api.tasks.options.Option
import org.gradle.process.ExecOperations
import javax.annotation.Nullable
import javax.inject.Inject

interface ArgStringifier {
    fun argToString(arg: Any): String {
        return if (arg is Provider<*>) {
            arg.get()
        } else {
            arg
        }.toString()
    }
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

abstract class PytestRunner  @Inject constructor(private val ex: ExecOperations): DefaultTask() {
    private val baseArguments = ListArgs()
    private val testArguments = ListArgs()
    private val pytestTrailingArguments = ListArgs()
    private var environment = MapArgs()

    @get:InputFile
    abstract val pytest: RegularFileProperty

    @get:Internal
    abstract val workingDir: DirectoryProperty

    fun hasWorkingDir(): Boolean {
        return workingDir.isPresent()
    }

    fun baseArgs(vararg args: Any): PytestRunner {
        baseArguments.add(*args)
        return this
    }

    fun baseArgs(args: Iterable<Any>): PytestRunner {
        baseArguments.add(args)
        return this;
    }

    fun setBaseArgs(args: Iterable<Any>): PytestRunner {
        baseArguments.set(args)
        return this;
    }

    @Nullable
    @Optional
    @Input
    fun getBaseArgs(): List<String> {
        return baseArguments.get()
    }

    fun tests(vararg args: Any): PytestRunner {
        testArguments.add(*args)
        return this
    }

    @Option(option = "test", description = "set the test specifiers passed to pytest")
    fun setTests(args: List<String>): PytestRunner {
        testArguments.set(args as Iterable<Any>)
        return this;
    }

    fun setTests(args: Iterable<Any>): PytestRunner {
        testArguments.set(args)
        return this;
    }

    @Nullable
    @Optional
    @Input
    fun getTests(): List<String> {
        return testArguments.get()
    }

    @Nullable
    @Optional
    @Input
    fun getPytestTrailingArguments(): List<String> {
        return pytestTrailingArguments.get()
    }

    @Option(option = "s", description = "disable pytest capturing")
    fun setS(): PytestRunner  {
        pytestTrailingArguments.add("-s")
        return this;
    }

    @Option(option = "tee-sys", description = "disable pytest capturing")
    fun setTeeS(): PytestRunner {
        pytestTrailingArguments.add("--capture=tee-sys")
        return this;
    }

    @Option(option = "v", description = "set pytest -v")
    fun setV(): PytestRunner {
        pytestTrailingArguments.add("-v")
        return this;
    }

    @Option(option = "debug", description = "set SAXONC_DEBUG_FLAG=true")
    fun setSaxonCDebug(): PytestRunner {
        environment.put("SAXONC_DEBUG_FLAG", "true");
        return this;
    }

    fun setEnvironment(environmentVariables: Map<String, Any>) {
        environment.set(environmentVariables)
    }

    fun environment(vararg environmentVariables: Pair<String, Any>): PytestRunner {
        environmentVariables.forEach { (name, value) -> environment(name, value) }
        return this
    }

    fun environment(name: String, value: Any): PytestRunner {
        environment.put(name to value)
        return this
    }

    fun environment(environmentVariables: Map<String, Any>): PytestRunner {
        environment.put(environmentVariables)
        return this
    }

    fun hasEnvironment(): Boolean {
        return !this.environment.isEmpty()
    }

    fun resolvedEnvironment(): Map<String, String> {
        return environment.get()
    }

    @TaskAction
    fun execute() {
        val runner = this
        ex.exec {
            if (runner.hasWorkingDir()) {
                workingDir(runner.workingDir.get())
            }
            executable(runner.pytest.get())
            args(runner.getBaseArgs())
            args(runner.getTests())
            args(runner.getPytestTrailingArguments())

            if (runner.hasEnvironment()) {
                environment(runner.resolvedEnvironment())
            }
        }
    }
}

abstract class TestSuiteTask  @Inject constructor(private val ex: ExecOperations): DefaultTask() {
    private val arguments = ListArgs()
    private var environment = MapArgs()

    @get:InputFile
    abstract val python: RegularFileProperty

    @get:InputFile
    abstract val driver: RegularFileProperty

    @get:InputDirectory
    abstract val suiteDir: DirectoryProperty

    @get:OutputDirectory
    val workingDir = project.layout.buildDirectory.dir(this.name)

    @get:OutputDirectory
    abstract val outputDir: DirectoryProperty

    @get:Input
    @get:Optional
    abstract val language: Property<String>

    fun wheelInstaller(task: TaskProvider<InstallLocalWheel>): TestSuiteTask {
        this.dependsOn(task)
        return this
    }

    fun driverRequirements(task: TaskProvider<PipInstall>): TestSuiteTask {
        this.dependsOn(task)
        return this
    }

    fun environment(vararg environmentVariables: Pair<String, Any>): TestSuiteTask {
        environmentVariables.forEach { (name, value) -> environment(name, value) }
        return this
    }

    fun environment(name: String, value: Any): TestSuiteTask {
        environment.put(name to value)
        return this
    }

    fun environment(environmentVariables: Map<String, Any>): TestSuiteTask {
        environment.put(environmentVariables)
        return this
    }

    fun hasEnvironment(): Boolean {
        return !this.environment.isEmpty()
    }

    fun resolvedEnvironment(): Map<String, String> {
        return environment.get()
    }

    @Option(option = "debug", description = "set SAXONC_DEBUG_FLAG=true")
    fun setSaxonCDebug(): TestSuiteTask {
        environment.put("SAXONC_DEBUG_FLAG", "true");
        return this;
    }

    @TaskAction
    fun runTests() {
        arguments.add(driver, suiteDir, "-o", outputDir)
        if (language.isPresent) {
            arguments.add("-l", language)
        }
        val runner = this
        ex.exec {
            workingDir(runner.workingDir.get())
            executable(runner.python.get())
            args(runner.arguments.get())
            if (runner.hasEnvironment()) {
                environment(runner.resolvedEnvironment())
            }
        }
    }
}