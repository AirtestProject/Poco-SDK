package com.netease.open.libpoco.sdk;

import org.json.JSONArray;
import org.json.JSONException;

import java.util.HashMap;
import java.util.Map;

/**
 * Created by adolli on 2017/7/13.
 */

public class DefaultMatcher implements IMatcher {
    private Map<String, Comparator> predicates = new HashMap<>();

    public DefaultMatcher() {
        predicates.put("attr=", new EqualizationComparator());
        predicates.put("attr.*=", new RegexpComparator());
    }

    public boolean match(JSONArray cond, INode node) throws JSONException {
        String op = cond.getString(0);
        JSONArray args = cond.getJSONArray(1);

        if (op.equals("or")) {
            for (int i = 0; i < args.length(); i++) {
                JSONArray arg = args.getJSONArray(i);
                if (this.match(arg, node)) {
                    return true;
                }
            }
            return false;
        }

        if (op.equals("and")) {
            for (int i = 0; i < args.length(); i++) {
                JSONArray arg = args.getJSONArray(i);
                if (!this.match(arg, node)) {
                    return false;
                }
            }
            return true;
        }

        Comparator comparator = this.predicates.get(op);
        if (comparator != null) {
            String attribute = args.getString(0);
            Object value = args.get(1);
            Object targetValue = node.getAttr(attribute);
            return comparator.compare(targetValue, value);
        }

        return false;
    }

    public interface Comparator {
        boolean compare(Object l, Object r);
    }

    public class EqualizationComparator implements Comparator {

        @Override
        public boolean compare(Object l, Object r) {
            if (l == null) {
                return false;
            }
            return l.equals(r);
        }
    }

    public class RegexpComparator implements Comparator {

        @Override
        public boolean compare(Object origin, Object pattern) {
            if (origin == null) {
                return false;
            }
            return ((String) origin).matches((String) pattern);
        }
    }
}
