package com.saxonica.build

import org.gradle.api.DefaultTask
import org.gradle.api.GradleException
import org.gradle.api.file.Directory
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.file.RegularFile
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.model.ObjectFactory
import org.gradle.api.provider.Property
import org.gradle.api.provider.Provider
import org.gradle.api.tasks.*
import org.gradle.kotlin.dsl.property
import software.amazon.awssdk.auth.credentials.DefaultCredentialsProvider
import software.amazon.awssdk.regions.Region
import software.amazon.awssdk.services.cloudformation.CloudFormationClient
import software.amazon.awssdk.services.cloudformation.model.Stack
import software.amazon.awssdk.services.s3.S3AsyncClient
import software.amazon.awssdk.services.sqs.SqsClient
import software.amazon.awssdk.services.sqs.model.Message
import software.amazon.awssdk.services.sqs.model.MessageAttributeValue
import software.amazon.awssdk.transfer.s3.S3TransferManager
import software.amazon.awssdk.transfer.s3.model.DownloadFileRequest
import java.nio.file.Paths
import java.util.*
import javax.inject.Inject


class CloudFormationStackDetails(val region: Property<String>, val stackName: Property<String>) {
  val signingInputsQueueUrl: String
    get() = stackOutputValue("CodeSigningInputsQueueUrl")

  val signingOutputsQueueUrl: String
    get() = stackOutputValue("CodeSigningOutputsQueueUrl")

  val signingInputsBucket: String
    get() = stackOutputValue("CodeSigningInputsBucketName")

  val signingOutputsBucket: String
    get() = stackOutputValue("CodeSigningOutputsBucketName")

  val stackDetails: Stack
    get() = fetchStackDetails()

  private fun fetchStackDetails(): Stack {
    println("Fetching stack details...")
    val cfClient = CloudFormationClient.builder()
      .region(Region.of(region.get()))
      .build()
    try {
      return cfClient.describeStacks {
        it.stackName(stackName.get())
      }.stacks().first()
    } catch (e: Exception) {
      throw GradleException("CloudFormation stack query failed", e)
    }
  }

  private fun stackOutputValue(key: String): String {
    return stackDetails.outputs().find { output -> output.outputKey() == key }!!.outputValue()
  }
}

abstract class AwsSigningResources: DefaultTask() {
  @get:Input
  abstract val region: Property<String>

  @get:Input
  abstract val stackName: Property<String>

  @get:Internal
  abstract val codeSigningInputsQueueUrl: Property<String>

  @get:Internal
  abstract val codeSigningInputsBucket: Property<String>

  @get:Internal
  abstract val codeSigningOutputsQueueUrl: Property<String>

  @get:Internal
  abstract val codeSigningOutputsBucket: Property<String>

  @TaskAction
  fun execute() {
    val details = CloudFormationStackDetails(region, stackName)
    codeSigningInputsQueueUrl.set(details.signingInputsQueueUrl)
    codeSigningInputsBucket.set(details.signingInputsBucket)
    codeSigningOutputsQueueUrl.set(details.signingOutputsQueueUrl)
    codeSigningOutputsBucket.set(details.signingOutputsBucket)
  }
}

abstract class AbstractS3ArtifactHandlingTask: DefaultTask() {
  @get:Input
  abstract val bucket: Property<String>

  @get:Input
  abstract val region: Property<String>

  @get:Input
  abstract val queueUrl: Property<String>

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

  @get:Internal
  protected val sqsClient: SqsClient by lazy {
    SqsClient.builder()
      .region(Region.of(region.get()))
      .build()
  }
}

abstract class UnsignedArtifactUploadTask
@Inject constructor(@Internal val objects: ObjectFactory): AbstractS3ArtifactHandlingTask() {
  @get:InputDirectory
  abstract val inputDir: DirectoryProperty

  @get:Input
  abstract val baseKeyPrefix: Property<String>

  private val generatedJobId: String by lazy { UUID.randomUUID().toString() }

  @get:Internal
  abstract val jobFileDir: DirectoryProperty

  @get:OutputFile
  val jobFile: Provider<RegularFile> = jobFileDir.map { it.file("job-file.txt") }

  @get:Internal
  val jobId: Provider<String> = objects.property<String>().map { generatedJobId }

  private val jobKeyPrefix: String
    get() = "${baseKeyPrefix.get()}/${generatedJobId}"

  @TaskAction
  fun upload() {
    val directoryUpload = transferManager.uploadDirectory { builder ->
      builder.source(Paths.get(inputDir.get().asFile.absolutePath))
      .s3Prefix(jobKeyPrefix)
      .bucket(bucket.get())
    }
    val completedDirectoryUpload = directoryUpload.completionFuture().join()
    completedDirectoryUpload.failedTransfers().forEach { fail ->
      logger.warn("Object [${fail}] failed to transfer")
    }
    if (completedDirectoryUpload.failedTransfers().isNotEmpty()) {
      throw GradleException("Could not transfer all files. See logs for more details")
    }

    val messageAttributes = mapOf(
      "jobId" to MessageAttributeValue.builder().stringValue(generatedJobId).dataType("String").build(),
    )
    val payload = inputDir.map { it.asFileTree.files.map { file -> file.relativeTo(it.asFile).toString() } }.get()
    sqsClient.sendMessage { builder ->
      builder.queueUrl(queueUrl.get())
        .messageBody(payload.joinToString("\n"))
        .messageAttributes(messageAttributes)
    }

    jobFile.get().asFile.writeText(generatedJobId)
  }
}

abstract class SignedArtifactDownloadTask: AbstractS3ArtifactHandlingTask() {
  @get:Internal
  abstract val outputDir: DirectoryProperty

  @get:OutputDirectory
  val output: Provider<Directory> = outputDir.map { it }

  @get:InputFile
  abstract val jobFile: RegularFileProperty

  private val jobId by lazy {
    jobFile.get().asFile.readText().trim()
  }
  private fun targetFilePath(file: String): Provider<RegularFile> {
    return outputDir.map { it.file(file) }
  }

  private fun downloadRequest(commonPrefix: String, keySuffix: String, target: Provider<RegularFile>): DownloadFileRequest {
    val key = "${commonPrefix}/${keySuffix}"
    return DownloadFileRequest.builder()
      .getObjectRequest { b -> b.bucket(bucket.get()).key(key) }
      .destination(target.get().asFile)
      .build()
  }

  private fun jobMessage(): Message {
    while (true) {
      val response = sqsClient.receiveMessage { builder ->
        builder.queueUrl(queueUrl.get())
          .maxNumberOfMessages(1)
          .messageAttributeNames("jobId", "commonPrefix")
          .waitTimeSeconds(10)
      }
      if (response.hasMessages()) {
        response.messages().filter { it.hasMessageAttributes() }.filter { it.messageAttributes()["jobId"]!!.stringValue() == jobId }.forEach { message ->
          return message
        }
      }
    }
  }

  @TaskAction
  fun download() {
    val sqsMessage = jobMessage()
    val objects = sqsMessage.body().trim().lines()
    val commonPrefix = sqsMessage.messageAttributes()["commonPrefix"]!!.stringValue()

    objects.map { obj ->
      val target = targetFilePath(obj)
      target.get().asFile.parentFile.mkdirs()
      downloadRequest(commonPrefix, obj, target)
    }.map { req ->
      transferManager.downloadFile(req)
    }.forEach { download ->
      download.completionFuture().join()
      logger.info("File downloaded [{}]", download)
    }

    sqsClient.deleteMessage { builder ->
      builder.queueUrl(queueUrl.get())
      builder.receiptHandle(sqsMessage.receiptHandle())
    }
  }
}
