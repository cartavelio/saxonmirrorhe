package performance;

public interface Measurable {

    void run();

    int expectedTimeMilliseconds();

    String name();
}

