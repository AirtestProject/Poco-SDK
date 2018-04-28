package com.netease.open.libpoco.sdk;

import android.annotation.SuppressLint;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.Map;

/**
 * Created by adolli on 2017/7/10.
 */

@SuppressLint("NewApi")
public abstract class AbstractDumper implements IDumper<AbstractNode> {
    public static String TAG = "AbstractDumper";

    public JSONObject dumpHierarchy() throws JSONException {
        return this.dumpHierarchy(true);
    }

    public JSONObject dumpHierarchy(boolean onlyVisibleNode) throws JSONException {
        return this.dumpHierarchyImpl(this.getRoot(), onlyVisibleNode);
    }

    public JSONObject dumpHierarchyImpl(AbstractNode node, boolean onlyVisibleNode) throws JSONException {
        if (node == null) {
            // return if still null
            return null;
        }

        JSONObject payload = new JSONObject();
        for (Map.Entry<String, Object> attr : node.enumerateAttrs().entrySet()) {
            payload.put(attr.getKey(), attr.getValue());
        }

        JSONObject result = new JSONObject();
        JSONArray children = new JSONArray();

        for (AbstractNode child : node.getChildren()) {
            if (!onlyVisibleNode || (boolean) child.getAttr("visible")) {
                children.put(this.dumpHierarchyImpl(child, onlyVisibleNode));
            }
        }
        if (children.length() > 0) {
            result.put("children", children);
        }

        result.put("name", node.getAttr("name"));
        result.put("payload", payload);

        return result;
    }

}
