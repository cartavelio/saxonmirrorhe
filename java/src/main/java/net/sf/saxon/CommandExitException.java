package net.sf.saxon;

public class CommandExitException extends RuntimeException {
    final int code;

    public int getCode() {
        return code;
    }

    public CommandExitException() {
        super();
        this.code = 1;
    }

    public CommandExitException(int status) {
        this.code = status;
    }

    public CommandExitException(String message) {
        super(message);
        this.code = 1;
    }

    public CommandExitException(int status, String message) {
        super(message);
        this.code = status;
    }

    public CommandExitException(int status, String message, Throwable cause) {
        super(message, cause);
        this.code = status;
    }

    public CommandExitException(String message, Throwable cause) {
        super(message, cause);
        this.code = 1;
    }

    public CommandExitException(int status, String message, Throwable cause, boolean enableSuppression, boolean writableStackTrace) {
        super(message, cause, enableSuppression, writableStackTrace);
        this.code = status;
    }

    public CommandExitException(String message, Throwable cause, boolean enableSuppression, boolean writableStackTrace) {
        super(message, cause, enableSuppression, writableStackTrace);
        this.code = 1;
    }
}
