package com.netease.open.libpoco.sdk;

import com.netease.open.libpoco.sdk.exceptions.NodeHasBeenRemovedException;

import org.json.JSONArray;
import org.json.JSONException;

import java.util.LinkedList;
import java.util.List;

/**
 * Created by adolli on 2017/7/12.
 */

public class Selector implements ISelector<AbstractNode> {
    private IDumper<AbstractNode> dumper;
    private IMatcher matcher;

    public Selector(IDumper<AbstractNode> dumper) {
        this(dumper, new DefaultMatcher());
    }

    public Selector(IDumper<AbstractNode> dumper, IMatcher matcher) {
        this.dumper = dumper;
        this.matcher = matcher;
    }

    @Override
    public AbstractNode[] select(JSONArray cond) throws JSONException {
        return this.select(cond, true);
    }

    @Override
    public AbstractNode[] select(JSONArray cond, boolean multiple) throws JSONException {
        List<AbstractNode> result = null;
        try {
            result = this.selectImpl(cond, multiple, this.getRoot(), 9999, true, true);
        } catch (NodeHasBeenRemovedException e) {
            // 如果Node被移除，表示traverse过程中界面发生了变化，那就直接返回空list，表示找不到
            result = new LinkedList<>();
        }
        return makeArray(result);
    }

    private List<AbstractNode> selectImpl(JSONArray cond, boolean multiple, AbstractNode root, int maxDepth, boolean onlyVisibleNode, boolean includeRoot) throws JSONException {
        List<AbstractNode> result = new LinkedList<>();
        if (root == null) {
            return result;
        }

        String op = cond.getString(0);
        JSONArray args = cond.getJSONArray(1);

        if (op.equals(">") || op.equals("/")) {
            // 相对选择
            List<AbstractNode> parents = new LinkedList<>();
            parents.add(root);
            for (int i = 0; i < args.length(); i++) {
                JSONArray arg = args.getJSONArray(i);
                List<AbstractNode> midResult = new LinkedList<>();
                for (AbstractNode parent : parents) {
                    int _maxDepth = maxDepth;
                    if (op.equals("/") && i != 0) {
                        _maxDepth = 1;
                    }
                    List<AbstractNode> _res = this.selectImpl(arg, true, parent, _maxDepth, onlyVisibleNode, false);
                    for (AbstractNode r : _res) {
                        if (!result.contains(r)) {
                            midResult.add(r);
                        }
                    }
                }
                parents = midResult;
            }
            result = parents;
        } else if (op.equals("-")) {
            // 兄弟节点选择
            JSONArray query1 = args.getJSONArray(0);
            JSONArray query2 = args.getJSONArray(1);
            List<AbstractNode> result1 = this.selectImpl(query1, multiple, root, maxDepth, onlyVisibleNode, includeRoot);
            for (AbstractNode n : result1) {
                List<AbstractNode> sibling_result = this.selectImpl(query2, multiple, n.getParent(), 1, onlyVisibleNode, includeRoot);
                for (AbstractNode r : sibling_result) {
                    if (!result.contains(r)) {
                        result.add(r);
                    }
                }
            }
        } else if (op.equals("index")) {
            JSONArray cond1 = args.getJSONArray(0);
            int i = args.getInt(1);
            result = new LinkedList<>();
            result.add(this.selectImpl(cond1, multiple, root, maxDepth, onlyVisibleNode, includeRoot).get(i));
        } else if (op.equals("^")) {
            // parent
            // only select parent of the first matched UI element
            JSONArray query1 = args.getJSONArray(0);
            List<AbstractNode> result1 = this.selectImpl(query1, false, root, maxDepth, onlyVisibleNode, includeRoot);
            if (!result1.isEmpty()) {
                AbstractNode parent_node = result1.get(0).getParent();
                if (parent_node != null) {
                    result = new LinkedList<>();
                    result.add(parent_node);
                }
            }
        } else {
            this.selectTraverse(cond, root, result, multiple, maxDepth, onlyVisibleNode, includeRoot);
        }

        return result;
    }

    private boolean selectTraverse(JSONArray cond, AbstractNode node, List<AbstractNode> outResult, boolean multiple, int maxDepth, boolean onlyVisibleNode, boolean includeRoot) throws JSONException {
        // 剪掉不可见节点branch
        if (onlyVisibleNode && !(boolean) node.getAttr("visible")) {
            return false;
        }

        if (this.matcher.match(cond, node)) {
            if (includeRoot) {
                if (!outResult.contains(node)) {
                    outResult.add(node);
                }
                if (!multiple) {
                    return true;
                }
            }
        }

        // 最大搜索深度耗尽并不表示遍历结束，其余child节点仍需遍历
        if (maxDepth == 0) {
            return false;
        }
        maxDepth -= 1;

        for (AbstractNode child : node.getChildren()) {
            boolean finished = this.selectTraverse(cond, child, outResult, multiple, maxDepth, onlyVisibleNode, true);
            if (finished) {
                return true;
            }
        }

        return false;
    }

    public AbstractNode getRoot() {
        return this.dumper.getRoot();
    }

    private static AbstractNode[] makeArray(List<AbstractNode> list) {
        int i = 0;
        AbstractNode[] ret = new AbstractNode[list.size()];
        for (AbstractNode n : list) {
            ret[i] = n;
            i++;
        }
        return ret;
    }
}
