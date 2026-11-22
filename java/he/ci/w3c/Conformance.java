import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Comparator;
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

    private record RoundTrip(String phase, String detail) {}

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

    private static Map<Case, String> outcomes(DocumentBuilder xml, Path file, String mode, Map<Case, String> comments)
            throws Exception {
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
                if (comments != null) {
                    comments.put(key, e.getAttribute("comment"));
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
        var factory = DocumentBuilderFactory.newNSInstance();
        factory.setAttribute(XMLConstants.ACCESS_EXTERNAL_DTD, "");
        factory.setAttribute(XMLConstants.ACCESS_EXTERNAL_SCHEMA, "");
        var xml = factory.newDocumentBuilder();
        if (args.length == 6 && Set.of("sef-check", "sef-record").contains(args[0])) {
            checkRoundTrips(xml, args);
            return;
        }
        if (args.length != 5 || !Set.of("check", "record").contains(args[0])) {
            throw new IllegalArgumentException("""
                    Usage: Conformance.java check|record MODE catalog.xml report.xml baseline.tsv
                           Conformance.java sef-check|sef-record source.xml source.tsv reloaded.xml attempts.tsv gaps.tsv
                    """);
        }
        boolean record = args[0].equals("record");
        String mode = args[1];
        var sets = catalog(xml, Path.of(args[2]));
        var actual = outcomes(xml, Path.of(args[3]), mode, null);
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

    private static void checkRoundTrips(DocumentBuilder xml, String[] args) throws Exception {
        boolean record = args[0].equals("sef-record");
        var source = outcomes(xml, Path.of(args[1]), "XT30", null);
        var comments = new HashMap<Case, String>();
        var reloaded = outcomes(xml, Path.of(args[3]), "XT30", comments);
        if (!source.keySet().equals(reloaded.keySet())) {
            throw new IllegalStateException("Source and reload reports cover different XSLT cases");
        }
        var sourceExecutables = readRoundTripStages(Path.of(args[2]), source.keySet(), Set.of("compiled"));
        var attempts = readRoundTripStages(Path.of(args[4]), source.keySet(), Set.of("export", "load", "reloaded"));
        var gaps = new HashMap<Case, RoundTrip>();
        Path gapFile = Path.of(args[5]);
        if (!record) {
            for (String line : Files.readAllLines(gapFile)) {
                if (line.startsWith("#") || line.isBlank()) continue;
                var fields = line.split("\t", 4);
                if (fields.length != 4 || fields[3].isBlank() ||
                        gaps.putIfAbsent(new Case(fields[0], fields[1]), new RoundTrip(fields[2], fields[3])) != null) {
                    throw new IllegalArgumentException("Invalid SEF gap: " + line);
                }
            }
        }

        var totals = new TreeMap<String, Integer>();
        var lines = new ArrayList<String>();
        lines.add("# Test set\tTest case\tRound-trip result\tReason");
        int changes = 0;
        for (Case key : source.keySet().stream().sorted(Comparator.comparing(Case::toString)).toList()) {
            if (!source.get(key).equals("pass")) {
                totals.merge("excluded-source", 1, Integer::sum);
                continue;
            }
            RoundTrip attempt = attempts.get(key);
            String status;
            String detail;
            if (!sourceExecutables.containsKey(key)) {
                if (attempt == null) {
                    totals.merge("excluded-no-executable", 1, Integer::sum);
                    continue;
                }
                status = "unexpected-attempt";
                detail = "Only the SEF run compiled an executable";
            } else if (attempt == null) {
                status = "missing-attempt";
                detail = "Source produced an executable, but the SEF run did not attempt export";
            } else if (!attempt.phase().equals("reloaded")) {
                status = attempt.phase();
                detail = attempt.detail();
            } else {
                status = reloaded.get(key).equals("pass") ? "pass" : "assertion-" + reloaded.get(key);
                detail = comments.get(key);
            }
            totals.merge(status, 1, Integer::sum);
            if (record && !status.equals("pass")) {
                detail = detail == null ? "" : detail.replaceAll("\\s+", " ").strip();
                if (detail.isBlank()) detail = "Reloaded stylesheet did not satisfy the catalog assertion";
                lines.add(String.join("\t", key.set(), key.name(), status, detail));
            } else if (!record) {
                RoundTrip expected = gaps.remove(key);
                String expectedStatus = expected == null ? "pass" : expected.phase();
                if (!status.equals(expectedStatus)) {
                    System.err.println(key + ": " + expectedStatus + " -> " + status + " " + detail);
                    changes++;
                }
            }
        }
        System.out.println("XT30 SEF round trip " + totals);
        if (!gaps.isEmpty() || changes != 0 || totals.getOrDefault("pass", 0) == 0) {
            throw new IllegalStateException("SEF coverage changed. Unmatched gaps: " + gaps.keySet());
        }
        if (record) Files.write(gapFile, lines);
    }

    private static Map<Case, RoundTrip> readRoundTripStages(Path file, Set<Case> cases, Set<String> stages)
            throws Exception {
        var results = new HashMap<Case, RoundTrip>();
        for (String line : Files.readAllLines(file)) {
            var fields = line.split("\t", 4);
            if (fields.length != 4 || !stages.contains(fields[2])) {
                throw new IllegalArgumentException("Invalid SEF stage: " + line);
            }
            var key = new Case(fields[0], fields[1]);
            if (!cases.contains(key) || results.putIfAbsent(key, new RoundTrip(fields[2], fields[3])) != null) {
                throw new IllegalArgumentException("Unexpected or duplicate SEF stage for " + key);
            }
        }
        return results;
    }
}
