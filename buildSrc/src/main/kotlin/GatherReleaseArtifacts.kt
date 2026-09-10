package com.saxonica.build

import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.FileSystemOperations
import org.gradle.api.file.RegularFile
import org.gradle.api.model.ObjectFactory
import org.gradle.api.provider.ListProperty
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputDirectory
import org.gradle.api.tasks.InputFiles
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import org.gradle.api.tasks.options.Option
import org.gradle.kotlin.dsl.property
import software.amazon.awssdk.auth.credentials.DefaultCredentialsProvider
import software.amazon.awssdk.regions.Region
import software.amazon.awssdk.services.s3.S3AsyncClient
import software.amazon.awssdk.services.s3.model.ListObjectsV2Request
import software.amazon.awssdk.services.sqs.model.MessageAttributeValue
import software.amazon.awssdk.transfer.s3.S3TransferManager
import software.amazon.awssdk.transfer.s3.model.CompletedFileUpload
import software.amazon.awssdk.transfer.s3.model.FileUpload
import java.io.File
import java.nio.file.Paths
import java.util.UUID
import java.util.concurrent.CompletableFuture
import javax.inject.Inject
import kotlin.collections.joinToString
import kotlin.io.path.listDirectoryEntries
import kotlin.math.log

abstract class GatherReleaseArtifacts @Inject constructor(private val fs: FileSystemOperations) : DefaultTask() {
  @get:InputFiles
  val artifacts: ConfigurableFileCollection = project.objects.fileCollection()

  @get:Internal
  abstract val outputDir: DirectoryProperty

  @Option(option = "to", description = "where the artifacts should be copied")
  fun setOutput(path: String): GatherReleaseArtifacts {
    outputDir.set(project.file(path))
    return this;
  }

  @TaskAction
  fun gather() {
    fs.copy {
      from(artifacts)
      into(outputDir)
    }
  }
}

abstract class AwsReleaseStagingResources: DefaultTask() {
  @get:Input
  abstract val region: Property<String>

  @get:Input
  abstract val stackName: Property<String>

  @get:Internal
  abstract val releaseArtifactsStagingBucket: Property<String>


  @TaskAction
  fun execute() {
    val details = CloudFormationStackDetails(region, stackName)
    releaseArtifactsStagingBucket.set(details.releaseArtifactStagingBucket)
  }
}

abstract class UploadReleaseArtifactsToStagingBucket : DefaultTask() {
  @get:Input
  abstract val bucket: Property<String>

  @get:Input
  abstract val region: Property<String>

  @get:Internal
  protected val s3AsyncClient: S3AsyncClient by lazy {
    S3AsyncClient.builder()
      .multipartEnabled(true)
      .credentialsProvider(DefaultCredentialsProvider.create())
      .region(Region.of(region.get()))
      .build()
  }

  @get:Internal
  protected val transferManager: S3TransferManager by lazy {
    S3TransferManager.builder()
      .s3Client(s3AsyncClient)
      .build()
  }

  @get:InputFiles
  val artifacts: ConfigurableFileCollection = project.objects.fileCollection()

  private fun wheels(): List<File> {
    return artifacts.files.map { wheelsDir -> wheelsDir.toPath().listDirectoryEntries("*.whl") }.flatten().map { it.toFile() }
  }

  @TaskAction
  fun upload() {
    val uploads = mutableListOf<Pair<String, FileUpload>>()
    wheels().chunked(4).forEach { chunk ->
      chunk.forEach { file ->
        println("Uploading $file")
        val fileUpload = transferManager.uploadFile({ upload ->
          upload.source(file).putObjectRequest { put -> put.bucket(bucket.get()).key(file.name) }
        })
        uploads.add(file.name to fileUpload)
      }
    }

    uploads.map { (name, upload) ->
      name to upload.completionFuture()
    }.forEach { (name, future) ->
      future.join()
      System.err.println("+ ${name}")
    }
  }
}

abstract class CheckStagedReleaseArtifactsPresent : DefaultTask() {
  @get:Input
  abstract val bucket: Property<String>

  @get:Input
  abstract val region: Property<String>

  @get:Input
  abstract val expectedArtifacts: ListProperty<String>

  @get:Internal
  protected val s3AsyncClient: S3AsyncClient by lazy {
    S3AsyncClient.builder()
      .multipartEnabled(true)
      .credentialsProvider(DefaultCredentialsProvider.create())
      .region(Region.of(region.get()))
      .build()
  }

  @TaskAction
  fun check() {
    val initialRequest = ListObjectsV2Request.builder()
      .bucket(bucket.get())
      .maxKeys(1)
      .build()

    val paginator = s3AsyncClient.listObjectsV2Paginator(initialRequest);
    val foundKeys = mutableListOf<String>()

    paginator.subscribe({ response ->
      response.contents().forEach({ obj ->
        foundKeys.add(obj.key())
      })
    }).join()

    val expected = expectedArtifacts.get().toList()
    val diff = expected - foundKeys
    if (!diff.isEmpty()) {
      println("Missing the following artifacts:")
      println(diff.joinToString("\n"))
      throw GradleException("Missing some expected artifacts")
    }
  }
}