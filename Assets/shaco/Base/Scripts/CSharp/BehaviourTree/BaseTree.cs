using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class BaseTree
    {
        public BaseTree root { get { return _root; } }
        public BaseTree parent { get { return _parent; } }
        public BaseTree child { get { return _child; } }
        public BaseTree next { get { return _next; } }
        public BaseTree prev { get { return _prev; } }
        public BaseTree first
        {
            get
            {
                BaseTree retValue = null;
                if (null != parent)
                    retValue = parent._child;
                else
                {
                    ForeachSibling((BaseTree tree) =>
                    {
                        if (tree.IsFirstChild())
                        {
                            retValue = tree;
                            return false;
                        }
                        else
                            return true;
                    });
                }
                return retValue;
            }
        }
        public BaseTree last
        {
            get
            {
                BaseTree retValue = null;
                if (null != parent)
                    retValue = parent._child;
                else
                {
                    ForeachSibling((BaseTree tree) =>
                    {
                        if (tree.IsLastChild())
                        {
                            retValue = tree;
                            return false;
                        }
                        else
                            return true;
                    });
                }
                return retValue;
            }
        }

        public string name 
        {
            get { return _name; }
            set { _name = value; }
        }

        public string fullName
        {
            get 
            {
                var retValue = new System.Text.StringBuilder();
                var allParent = new List<BaseTree>();
                var parentTmp = this._parent;
                while (parentTmp != null)
                {
                    allParent.Add(parentTmp);
                    parentTmp = parentTmp._parent;
                }
                for (int i = allParent.Count - 1; i >= 0; --i)
                {
                    retValue.Append(allParent[i]._name);
                    retValue.Append(".");
                }

                if (retValue.Length == 0)
                {
                    return this._name;
                }
                else 
                {
                    retValue.Append(this._name);
                    return retValue.ToString();
                }
            }
        }

        public IEnumerator<BaseTree> GetEnumerator()
        {
            var children = new List<BaseTree>();
            ForeachChildren((BaseTree child)=>
            {
                children.Add(child);
                return true;
            });

            while (children.Count > 0)
            {
                yield return children[0];
                children[0].ForeachChildren((BaseTree child) =>
                {
                    children.Add(child);
                    return true;
                });
                children.RemoveAt(0);
            }
        }

        private string _name = "BaseTree";

        public int Count
        {
            get
            {
                return _count;
            }
        }

        protected BaseTree _parent = null;
        protected BaseTree _child = null;
        protected BaseTree _next = null;
        protected BaseTree _prev = null;
        protected BaseTree _root = null;

        private int _count = 0;

        public override string ToString()
        {
            return name;
        }

        public BaseTree AddChild(BaseTree child)
        {
            return LinkChild(this, child);
        }

        public BaseTree InsertChild(BaseTree child, int index)
        {
            BaseTree retValue = null;
            if (index < 0)
            {
                Log.Error("BaseTree InserChild error: out of range index=" + index);
                return retValue;
            }
            var parent = GetChild(index);

            if (null == parent)
            {
                retValue = this.AddChild(child);
            }
            else if (0 == index)
            {
                parent = parent.parent;
                child._parent = parent;
                child._next = parent._child;
                child._prev = parent._child._prev;
                parent._child._prev._next = child;
                parent._child._prev = child;
                parent._child = child;
                retValue = parent;

                ++this._count;
            }
            else
            {
                parent = parent.prev;
                retValue = parent.AddSibling(child);
            }
            return retValue;
        }

        public BaseTree InsertSibling(BaseTree tree, int index)
        {
            BaseTree retValue = null;
            BaseTree findTree = null;

            int indexTmp = 0;
            ForeachSibling((BaseTree tree2) =>
            {
                findTree = tree2;
                return ++indexTmp < index;
            });

            if (findTree.IsFirstChild() && findTree._parent != null)
            {
                findTree._parent.InsertChild(tree, 0);
            }
            else
            {
                findTree.prev.AddSibling(tree);
            }
            return retValue;
        }

        public BaseTree AddSibling(BaseTree tree)
        {
            return LinkSibling(this, tree);
        }

        public void RemoveChild(BaseTree child)
        {
            if (null == child)
            {
                Log.Error("BaseTree RemoveChild error: tree is null !");
                return;
            }
            CutLink(child);
        }

        public void RemoveChild(int index)
        {
            var child = GetChild(index);
            if (null == child)
            {
                Log.Error("BaseTree RemoveChild error: tree is null ! index=" + index);
                return;
            }
            CutLink(child);
        }

        public void RemoveMe(bool isRetainChildren = false)
        {
            CutLink(this, isRetainChildren);
        }

        public BaseTree GetChild(int index)
        {
            BaseTree retValue = null;
            int indexForeach = 0;
            if (index >= 0)
            {
                ForeachChildren((BaseTree tree) =>
                {
                    if (indexForeach++ == index)
                    {
                        retValue = tree;
                        return false;
                    }
                    else
                        return true;
                });
            }
            return retValue;
        }

        public void RemoveChildren()
        {
            if (this == _root)
            {
                ForeachChildren((BaseTree child) =>
                {
                    child._parent = null;
                    return true;
                });
            }
            else 
            {
                ForeachAllChildren((BaseTree child, int index, int level) =>
                {
                    //只清理第一层树的父节点
                    if (0 == level)
                    {
                        child._parent = null;
                    }
                    return true;
                });
            }

            this._count = 0;
            this._child = null;
        }

        public void ForeachSibling(System.Func<BaseTree, bool> callback)
        {
            ForeachSibling(this, callback);
        }

        //System.Func<tree, break flag>
        public void ForeachChildren(System.Func<BaseTree, bool> callback)
        {
            if (null != _child)
                ForeachSibling(_child, callback);
        }

        //System.Func<tree, index, tree level, break flag>
        public void ForeachAllChildren(System.Func<BaseTree, int, int, bool> callback)
        {
            ForeachAllChildrenBase(this._child, 0, 0, callback);
        }

        public BaseTree CloneWithoutLinkRelation()
        {
            var retValue = new BaseTree();
            retValue.name = this.name;
            return retValue;
        }

        public BaseTree Clone()
        {
            var retValue = new BaseTree();
            retValue._root = this._root;
            retValue.name = this.name;

            this.ForeachChildren((BaseTree tree) =>
            {
                retValue.AddChild(tree.Clone());
                return true;
            });
            return retValue;
        }

        public void PrintAllChildren()
        {
            var logPrint = new System.Text.StringBuilder();
            logPrint.Append("Root:" + "[" + this.name + "]\n");
            logPrint.Append(PrintAllChildren(this));
            Log.Info(logPrint.ToString());
        }

        public bool IsRoot()
        {
            return _root == this;
        }

        public bool IsFirstChild()
        {
            if (null != _parent)
                return _parent._child == this;
            else
                return _prev != null && _prev._next == this;
        }

        public bool IsLastChild()
        {
            if (null != parent)
                return _parent._child._prev == this;
            else
                return _next != null && _next._prev == this;
        }

        public bool IsProcessChild()
        {
            return null == _child;
        }

        public int GetSiblingIndex()
        {
            int retValue = -1;
            if (_parent == null)
            {
                Log.Error("BaseTree GetSiblingIndex error: parent is null!");
                return retValue;
            }
            var firstChild = _parent._child;
            var nextChild = firstChild;
            int count = _parent.Count;

            for (int i = 0; i < count; ++i)
            {
                if (nextChild == this)
                {
                    retValue = i;
                    break;
                }
                nextChild = nextChild._next;
            }

            if (retValue < 0)
                Log.Error("BaseTree GetSiblingIndex error: not find child by parent=" + _parent + " child=" + this);

            return retValue;
        }

        public void SetSiblingIndex(int index)
        {
            var parentTmp = this._parent;
            this.RemoveMe();
            parentTmp.InsertChild(this, index);
        }

        public void Replace(BaseTree newTree)
        {
            int siblingIndex = this.GetSiblingIndex();
            var oldParent = this._parent;
            var oldChildren = new List<BaseTree>();

            newTree._root = this._root;
            newTree._next = this._next;
            newTree._prev = this._prev;

            if (this._child != null)
            {
                ForeachChildren((BaseTree child) =>
                {
                    oldChildren.Add(child);
                    return true;
                });
            }
            
            this.RemoveMe();
            if (null != oldParent)
            {
                oldParent.InsertChild(newTree, siblingIndex);
            }

            for (int i = 0; i < oldChildren.Count; ++i)
            {
                newTree.AddChild(oldChildren[i]);
            }
        }

        public BaseTree()
        {
            this._root = this;
            this._next = this;
            this._prev = this;
        }

        private void ForeachSibling(BaseTree tree, System.Func<BaseTree, bool> callback)
        {
            if (!callback(tree)) return;

            var next = tree._next;
            while (next != null && next != tree)
            {
                if (!callback(next))
                {
                    break;
                }
                next = next._next;
            }
        }

        private System.Text.StringBuilder PrintAllChildren(BaseTree parent)
        {
            var retValue = new System.Text.StringBuilder();
            if (null == parent)
            {
                return retValue;
            }

            var children = new List<BaseTree>();

            parent.ForeachChildren((BaseTree tree) =>
            {
                retValue.Append("[" + parent + "->" + tree + "]");
                children.Add(tree);
                return true;
            });

            if (null != parent._child)
                retValue.Append("\n");

            for (int i = 0; i < children.Count; ++i)
            {
                retValue.Append(PrintAllChildren(children[i] as BaseTree));
            }

            return retValue;
        }

        private void ForeachAllChildrenBase(BaseTree child, int index, int level, System.Func<BaseTree, int, int, bool> callback)
        {
            if (child == null) return;

            var children = new List<BaseTree>();

            if (!callback(child, index++, level)) return;
            children.Add(child);

            var next = child._next;
            while (next != null && next._parent != null && next != child)
            {
                if (!callback(next, index++, level))
                {
                    break;
                }
                children.Add(next);
                next = next._next;
            }

            ++level;

            for (int i = 0; i < children.Count; ++i)
                ForeachAllChildrenBase(children[i]._child, 0, level, callback);
        }

        private BaseTree LinkChild(BaseTree parent, BaseTree child)
        {
            if (child.parent != null)
            {
                Log.Error("BaseTree LinkChild error: this tree has be used ! created a tree from tree=" + child);
                child = child.CloneWithoutLinkRelation();
            }

            child._parent = parent;
            if (parent._child != null)
            {
                child._prev = parent._child._prev;
                child._next = parent._child;
                parent._child._prev._next = child;
                parent._child._prev = child;
            }
            else
            {
                child._prev = child;
                child._next = child;
                parent._child = child;

                if (null == parent._root)
                {
                    parent._root = this;
                }
            }
            child._root = parent._root;

            ++parent._count;
            return parent;
        }

        private BaseTree LinkSibling(BaseTree left, BaseTree right)
        {
            if (null == left)
            {
                Log.Error("BaseTree LinkSibling error: left tree is null!");
                return left;
            }

            if (right.parent != null)
            {
                Log.Error("BaseTree LinkSibling error: this tree has be used ! created a tree from tree=" + right);
                right = right.CloneWithoutLinkRelation();
            }

            if (left != null && left == left.root)
            {
                Log.Error("BaseTree LinkSibling error: this funciton can't use on root");
                return left;
            }

            if (left._next != null && left._next._prev != null)
                left._next._prev = right;
            right._parent = left.parent;
            right._prev = left;
            right._next = left._next;
            left._next = right;
            right._root = left._root;

            if (null != left._parent)
            {
                ++left._parent._count;
            }
            return left;
        }

        private BaseTree CutLink(BaseTree tree, bool isRetainChildren = false)
        {
            if (tree._child != null && !isRetainChildren)
            {
                tree.RemoveChildren();
            }

            if (tree._parent != null && tree._parent._child == tree)
            {
                tree._parent._child = tree._parent.Count == 1 ? null : tree._next;
            }

            if (tree._prev != null)
            {
                tree._prev._next = tree._next;
            }

            if (tree._next != null)
            {
                tree._next._prev = tree._prev;
            }

            if (null != tree._parent)
            {
                --tree._parent._count;
                tree._parent = null;
            }
            return tree;
        }
    }
}