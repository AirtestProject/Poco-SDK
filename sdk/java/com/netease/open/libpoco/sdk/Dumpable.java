package com.netease.open.libpoco.sdk;

import org.json.JSONException;
import org.json.JSONObject;

/**
 * Created by adolli on 2017/8/21.
 */

public interface Dumpable {
    JSONObject dumpHierarchy() throws JSONException;
    JSONObject dumpHierarchy(boolean onlyVisibleNode) throws JSONException;

}
