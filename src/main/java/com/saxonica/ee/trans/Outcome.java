package com.saxonica.ee.trans;

/**
 * An Outcome represents the result of a task, which might be success or failure
 * <code>T</code> is the class of the result of the task in the event that it is successful
 */
public class Outcome<T> {

    private T result;
    private Exception exception;

    /**
     * Create a success outcome
     * @param result the successful outcome
     */

    public Outcome (T result) {
        this.result = result;
    }

    /**
     * Create a failure outcome
     * @param exception the unsuccessful outcome
     */

    public Outcome(Exception exception) {
        this.exception = exception;
    }

    /**
     * Ask if the task was successful
     * @return  true if the task was successful
     */

    public boolean isSuccess() {
        return result != null;
    }

    /**
     * Get the result of the task if it was successful
     * @return the result of the task if it was successful, or null otherwise
     */

    public T getResult() {
        return result;
    }

    /**
     * Get the result of the task if it was unsuccessful
     *
     * @return the result of the task if it was unsuccessful, or null otherwise
     */

    public Exception getException() {
        return exception;
    }
}

// Copyright (c) 2018-2022 Saxonica Limited