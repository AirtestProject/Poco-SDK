package com.netease.open.libpoco.sdk;

import org.json.JSONArray;
import org.json.JSONException;

/**
 * Created by adolli on 2017/7/12.
 */

public interface ISelector <NodeType> {

    NodeType[] select(JSONArray cond) throws JSONException;
    NodeType[] select(JSONArray cond, boolean multiple) throws JSONException;
}
