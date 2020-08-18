using System;
using System.Collections.Generic;
using UnityEngine;

namespace B2Framework
{
    [Serializable]
    public class SensitiveWordsChecker
    {
        class Node
        {
            public int depth = 1;
            public Dictionary<string, Node> next = null;
        }
        Dictionary<string, Node> dic = new Dictionary<string, Node>();
        public void Init(string path)
        {
            TextAsset request = (TextAsset)AssetsManger.LoadAsset(path, typeof(TextAsset)).asset;
            string[] words = request.text.Split(new string[1]{"\r\n"}, StringSplitOptions.None);
            for(int i = 0; i < words.Length; ++i)
            {
                string singleWord = words[i];
                if(singleWord.Length > 0)
                {
                    Dictionary<string, Node> next = null;
                    Dictionary<string, Node> nowDic = dic;
                    if (!nowDic.ContainsKey(singleWord[0].ToString()))
                    {
                        Node rootNode = new Node();
                        rootNode.depth = 1;
                        nowDic.Add(singleWord[0].ToString(), rootNode);
                        next = rootNode.next;
                    }
                    else
                    {
                        next = nowDic[singleWord[0].ToString()].next;
                    }

                    Node nextNode = null;
                    for (int j = 1; j < singleWord.Length; ++j)
                    {
                        if (next == null)
                        {
                            next = new Dictionary<string, Node>();
                            nextNode = new Node();
                            nextNode.depth = j + 1;
                            next.Add(singleWord[j].ToString(), nextNode);
                            nowDic[singleWord[j - 1].ToString()].next = next;
                        }
                        else
                        {
                            if(!next.ContainsKey(singleWord[j].ToString()))
                            {
                                nextNode = new Node();
                                nextNode.depth = j + 1;
                                next.Add(singleWord[j].ToString(), nextNode);
                            }
                            else
                            {
                                nextNode = next[singleWord[j].ToString()];
                            }
                        }
                        nowDic = next;
                        next = nextNode.next;
                    }
                }
            }
            //PrintNode("", dic);
        }

        void PrintNode(string s, Dictionary<string, Node> dic)
        {
            if(dic != null && dic.Count > 0)
            {
                foreach (KeyValuePair<string, Node> kvp in dic)
                {
                    string k = kvp.Key;
                    string snext = s + k;
                    PrintNode(snext, dic[k].next);
                }
            }
            else
            {
                Log.Debug(s);
            }
        }

        public string CheckAndReplace (String s)
        {
            for(int i = 0; i < s.Length; ++i)
            {
                if(dic.ContainsKey(s[i].ToString()))
                {
                    int findIndex = -1;
                    int findCount = 1;
                    string findstr = s[i].ToString();
                    string stars = "*";
                    Node node = dic[s[i].ToString()];
                    Dictionary<string, Node> nextDic = node.next;
                    if (nextDic == null)
                    {
                        findCount = node.depth;
                        findIndex = i + 1;
                    }
                    else
                    {
                        for (int j = i + 1; j < s.Length; ++j)
                        {
                            if (nextDic.ContainsKey(s[j].ToString()))
                            {
                                node = nextDic[s[j].ToString()];
                                findstr = findstr + s[j].ToString();
                                stars = stars + "*";
                            }
                            else
                            {
                                break;
                            }
                            nextDic = node.next;
                            if (nextDic == null)
                            {
                                findCount = node.depth;
                                findIndex = j;
                                break;
                            }
                        }
                    }
                    if(findIndex != -1)
                    {
                        s = s.Replace(findstr, stars);
                    }
                    i += findCount - 1;
                }
            }
            return s;
        }
    }
}