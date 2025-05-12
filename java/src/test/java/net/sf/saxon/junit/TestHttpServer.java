package net.sf.saxon.junit;

import spark.Route;
import spark.Service;

import java.io.*;
import java.net.URI;
import java.net.URISyntaxException;
import java.nio.file.Path;
import java.util.function.Consumer;

import static spark.Service.ignite;

public class TestHttpServer {
    private Service http = null;
    private final Path fixturesRoot;

    public TestHttpServer(Path fixturesRoot) {
        this.fixturesRoot = fixturesRoot;
    }

    public Route file(String filePath) {
        try {
            String fileBody = readFile(filePath);
            return (request, response) -> {
                response.status(200);
                return fileBody;
            };
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    public Route file(String filePath, String mimeType) {
        try {
            String fileBody = readFile(filePath);
            return (request, response) -> {
                response.status(200);
                response.type(mimeType);
                return fileBody;
            };
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    private String readFile(String filePath) throws IOException {
        StringBuilder resultStringBuilder = new StringBuilder();
        try (InputStream inputStream = new FileInputStream(fixturesRoot.resolve(filePath).toFile());
             BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream))) {
            String line;
            while ((line = reader.readLine()) != null) {
                resultStringBuilder.append(line).append("\n");
            }
        }
        return resultStringBuilder.toString();
    }

    public String url(String pathString) {
        try {
            return new URI("http", null, "localhost", http.port(), "/", null, null).resolve(pathString).toString();
        } catch (URISyntaxException e) {
            throw new RuntimeException(e);
        }
    }

    public void route(Consumer<Service> definition) {
        http = ignite();

        definition.accept(http);
        // wait for Spark to start up
        http.awaitInitialization();
    }

    public void stop() {
        if (http != null) {
            http.stop();
            http.awaitStop();
        }
    }
}
