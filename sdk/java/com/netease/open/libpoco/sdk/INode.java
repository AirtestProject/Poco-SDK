package com.netease.open.libpoco.sdk;

import com.netease.open.libpoco.sdk.exceptions.NodeHasBeenRemovedException;
import com.netease.open.libpoco.sdk.exceptions.UnableToSetAttributeException;

/**
 * Created by adolli on 2017/7/19.
 */

public interface INode {
    Object getAttr(String attrName) throws NodeHasBeenRemovedException;
    void setAttr(String attrName, Object value) throws UnableToSetAttributeException, NodeHasBeenRemovedException;
}
