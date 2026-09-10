package net.sf.saxon.trace;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Manages the coverage data for a single module.
 *
 * <p>This class was introduced to avoid a map-of-maps that seemed to confuse the
 * transpiler.</p>
 */

public class CoverageRecord {
    Map<Integer, Integer> coverage = new HashMap<>();
    void addLocation(int line) {
        if (!coverage.containsKey(line)) {
            coverage.put(line, 0);
        }
    }
    int getCover(int line) {
        Integer count = coverage.get(line);
        if (count == null) {
            return 0;
        }
        return count;
    }
    void addCover(int line) {
        if (coverage.containsKey(line)) {
            coverage.put(line, coverage.get(line) + 1);
        } else {
            coverage.put(line, 1);
        }
    }
    List<Integer> coveredLines() {
        List<Integer> result = new ArrayList<>(coverage.keySet());
        //#if CSHARP==false
        result.sort(Integer::compareTo);
        //#endif
        return result;
    }
}
