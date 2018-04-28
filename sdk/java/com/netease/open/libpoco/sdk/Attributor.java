package com.netease.open.libpoco.sdk;

import com.netease.open.libpoco.sdk.AbstractNode;
import com.netease.open.libpoco.sdk.exceptions.NodeHasBeenRemovedException;

/**
 * Created by adolli on 2017/7/19.
 */

public class Attributor {
    public Attributor() {}

    public Object getAttr(AbstractNode[] nodes, String attrName) {
        return getAttr(nodes[0], attrName);
    }

    public Object getAttr(AbstractNode node, String attrName) {
        if (node == null) {
            throw new NodeHasBeenRemovedException(attrName, node);
        }
        return node.getAttr(attrName);
    }

    public void setAttr(AbstractNode[] nodes, String attrName, Object attrVal) {
        setAttr(nodes[0], attrName, attrVal);
    }

    public void setAttr(AbstractNode node, String attrName, Object attrVal) {
        if (node == null) {
            throw new NodeHasBeenRemovedException(attrName, node);
        }
        node.setAttr(attrName, attrVal);
    }
}
