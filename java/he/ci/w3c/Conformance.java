import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeMap;
import javax.xml.XMLConstants;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import org.w3c.dom.Element;
import org.w3c.dom.Node;

/** Checks Saxon's case-level W3C results against the catalog and the reviewed HE baseline. */
class Conformance {
    private record Case(String set, String name) {
        @Override public String toString() { return set + "/" + name; }
    }

    private static List<Element> children(Element parent, String name) {
        var elements = new ArrayList<Element>();
        for (Node child = parent.getFirstChild(); child != null; child = child.getNextSibling()) {
            if (child instanceof Element e && name.equals(e.getLocalName())
                    && parent.getNamespaceURI().equals(e.getNamespaceURI())) {
                elements.add(e);
            }
        }
        return elements;
    }

    private static Map<String, List<String>> catalog(DocumentBuilder xml, Path file) throws Exception {
        var sets = new TreeMap<String, List<String>>();
        var root = xml.parse(file.toFile()).getDocumentElement();
        for (var entry : children(root, "test-set")) {
            var name = entry.getAttribute("name");
            var path = file.resolveSibling(entry.getAttribute("file"));
            var set = xml.parse(path.toFile()).getDocumentElement();
            var cases = children(set, "test-case").stream().map(e -> e.getAttribute("name")).sorted().toList();
            if (!name.equals(set.getAttribute("name"))
                    || Set.copyOf(cases).size() != cases.size() || sets.put(name, cases) != null) {
                throw new IllegalArgumentException("Invalid catalog test set: " + path);
            }
        }
        if (sets.values().stream().allMatch(List::isEmpty)) throw new IllegalArgumentException("Empty catalog: " + file);
        return sets;
    }

    private static Map<Case, String> outcomes(DocumentBuilder xml, Path file, String mode) throws Exception {
        var root = xml.parse(file.toFile()).getDocumentElement();
        boolean xslt = mode.equals("XT30");
        String namespace = xslt ? "http://www.w3.org/2012/11/xslt30-test-results"
                                : "http://www.w3.org/2012/08/qt-fots-results";
        var metadata = root.getElementsByTagNameNS(namespace, xslt ? "configuration" : "product");
        if (metadata.getLength() != 1
                || !mode.equals(((Element) metadata.item(0)).getAttribute(xslt ? "lang" : "language"))) {
            throw new IllegalArgumentException("Expected " + mode + " report: " + file);
        }
        var results = new HashMap<Case, String>();
        for (var set : children(root, "test-set")) {
            for (var e : children(set, "test-case")) {
                var key = new Case(set.getAttribute("name"), e.getAttribute("name"));
                var result = e.getAttribute("result");
                var previous = results.putIfAbsent(key, result);
                // QT3 can report the same failure twice. Contradictory entries are ambiguous.
                if (result.isBlank() || previous != null && !previous.equals(result)) {
                    throw new IllegalArgumentException("Invalid results for " + key + ": " + previous + ", " + result);
                }
            }
        }
        return results;
    }

    private static Map<Case, String> readBaseline(Path file) throws Exception {
        var expected = new HashMap<Case, String>();
        for (var line : Files.readAllLines(file)) {
            if (line.startsWith("#") || line.isBlank()) continue;
            var fields = line.split("\t", -1);
            if (fields.length != 3 || fields[2].isBlank()
                    || expected.put(new Case(fields[0], fields[1]), fields[2]) != null) {
                throw new IllegalArgumentException("Invalid baseline entry: " + line);
            }
        }
        return expected;
    }

    public static void main(String[] args) throws Exception {
        if (args.length != 5 || !Set.of("check", "record").contains(args[0])) {
            throw new IllegalArgumentException("Usage: Conformance.java check|record MODE catalog.xml report.xml baseline.tsv");
        }
        boolean record = args[0].equals("record");
        String mode = args[1];
        var factory = DocumentBuilderFactory.newNSInstance();
        factory.setAttribute(XMLConstants.ACCESS_EXTERNAL_DTD, "");
        factory.setAttribute(XMLConstants.ACCESS_EXTERNAL_SCHEMA, "");
        var xml = factory.newDocumentBuilder();
        var sets = catalog(xml, Path.of(args[2]));
        var actual = outcomes(xml, Path.of(args[3]), mode);
        var file = Path.of(args[4]);
        var expected = record ? new HashMap<Case, String>() : readBaseline(file);
        var lines = new ArrayList<String>();
        lines.add("# set\tcase (* = whole set)\texpected result; unlisted cases must pass");
        var totals = new TreeMap<String, Integer>();
        int errors = 0;
        int improvements = 0;
        for (var entry : sets.entrySet()) {
            String set = entry.getKey();
            var names = entry.getValue();
            var wildcard = new Case(set, "*");
            String defaultResult = expected.getOrDefault(wildcard, "pass");
            expected.remove(wildcard);
            if (record && !names.isEmpty()) {
                String first = actual.get(new Case(set, names.getFirst()));
                if (first != null && !first.equals("pass") && names.stream().allMatch(n -> first.equals(actual.get(new Case(set, n))))) {
                    defaultResult = first;
                    lines.add(set + "\t*\t" + first);
                }
            }
            for (String name : names) {
                var key = new Case(set, name);
                String prior = expected.getOrDefault(key, defaultResult);
                expected.remove(key);
                String result = actual.remove(key);
                if (result == null) {
                    System.err.println(mode + " missing " + key);
                    errors++;
                    continue;
                }
                totals.merge(result, 1, Integer::sum);
                if (result.equals(prior)) continue;
                if (record) {
                    lines.add(set + "\t" + name + "\t" + result);
                } else if (result.equals("pass")) {
                    System.out.println(mode + " improved " + key + ": " + prior + " -> pass");
                    improvements++;
                } else {
                    System.err.println(mode + " changed " + key + ": " + prior + " -> " + result);
                    errors++;
                }
            }
        }
        if (!actual.isEmpty() || !expected.isEmpty()) {
            throw new IllegalStateException("Cases outside the catalog: report=" + actual.keySet() + ", baseline=" + expected.keySet());
        }
        System.out.println(mode + " " + totals + ", improvements=" + improvements + ", errors=" + errors);
        if (errors != 0) throw new IllegalStateException(mode + " conformance check failed");
        if (improvements != 0) throw new IllegalStateException(mode + " has newly passing cases. Review and record the baseline update.");
        if (record) Files.write(file, lines);
    }
}
