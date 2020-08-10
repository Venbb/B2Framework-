using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class BehaviourTreeConfig
    {
        public const string CHILDREN_KEY = "base.Children";

        static public bool LoadFromJsonPath(string path, BehaviourRootTree tree)
        {
            var jsonRead = FileHelper.ReadAllByUserPath(path);
            if (string.IsNullOrEmpty(jsonRead))
            {
                Log.Error("BehaviourTreeConfig InitWithJsonPath error: not find file by path=" + path);
                return false;
            }

            return LoadFromJson(jsonRead, tree);
        }

        static public bool LoadFromJson(string json, BehaviourRootTree tree)
        {
            if (string.IsNullOrEmpty(json))
            {
                Log.Error("BehaviourTreeConfig LoadFromJson error: json is empty !");
                return false;
            }

            if (null == tree)
            {
                Log.Error("BehaviourTreeConfig LoadFromJson error: tree is null!");
                return false;
            }

            //重置树节点数据
            tree.RemoveChildren();

            //检测配置根节点类型是否一致
            var jsonRoot = shaco.LitJson.JsonMapper.ToObject(json);
            if (jsonRoot["base.FullTypeName"].ToString() != tree.ToTypeString())
            {
                Log.Error("BehaviourTreeConfig LoadFromJson error: not BehaviourTree !");
                return false;
            }

            ToChildrenTree(tree, jsonRoot);
            return true;
        }

        static public bool SaveToJson(BehaviourRootTree root, string path)
        {
            var jsonRoot = root.ToJson();

            if (null != jsonRoot)
            {
                jsonRoot[CHILDREN_KEY] = ToChildrenJsonData(root, new shaco.LitJson.JsonData());
            } 

            FileHelper.WriteAllByUserPath(path, jsonRoot.ToJson());
            return true;
        }

        static private shaco.LitJson.JsonData ToChildrenJsonData(BehaviourTree parentTree, shaco.LitJson.JsonData parentJson)
        {
            parentJson.SetJsonType(shaco.LitJson.JsonType.Array);

            parentTree.ForeachChildren((tree) =>
            {
                var childTmp = tree.ToJson();
                if (null != childTmp)
                {
                    parentJson.Add(childTmp);
                    var newData = new shaco.LitJson.JsonData();
                    childTmp[CHILDREN_KEY] = ToChildrenJsonData(tree, newData);
                }
                return true;
            });
            return parentJson;
        }

        static private void ToChildrenTree(BehaviourTree parentTree, shaco.LitJson.JsonData parentJson)
        {
            var childJsonData = parentJson[CHILDREN_KEY];
            foreach (shaco.LitJson.JsonData iter in childJsonData)
            {
                var fullTypeNameTmp = iter["base.FullTypeName"].ToString();
                var childTmp = shaco.Base.Utility.Instantiate(fullTypeNameTmp) as BehaviourTree;
                if (null == childTmp)
                    Log.Error("BehaviourTreeConfig ToChildrenTree error: can't instatiate by type name=" + fullTypeNameTmp);
                else
                {
                    childTmp.FromJson(iter);
                    parentTree.AddChild(childTmp);
                    ToChildrenTree(childTmp, iter);
                }
            }  
        }
    }
}

