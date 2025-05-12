package performance;

public interface Measurable {

    void run();

    int expectedTimeMicroseconds();

    String name();
}

