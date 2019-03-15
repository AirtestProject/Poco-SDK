package com.netease.open.libpoco.sdk;

import org.json.JSONArray;
import org.json.JSONException;

/**
 * Created by adolli on 2017/7/13.
 */

public interface IMatcher {
    boolean match(JSONArray cond, INode node) throws JSONException;
}
