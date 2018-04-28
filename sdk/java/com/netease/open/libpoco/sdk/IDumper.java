package com.netease.open.libpoco.sdk;

import org.json.JSONException;
import org.json.JSONObject;

/**
 * Created by adolli on 2017/7/12.
 */

public interface IDumper <NodeType> extends Dumpable {

    NodeType getRoot();
}
