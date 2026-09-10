package s9apitest;

/**
 * Created by mike on 07/12/15.
 */
public class MyFoo implements Foo<String> {
    @Override
    public String foo() {
        return "foo";
    }
}
