using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Test
{
    public class TestTools : MonoBehaviour
    {
        private enum TestEnum
        {
            [Header("测试A")]
            A,
            [Header("测试A")]
            B
        }

        [SerializeField]
        [Header("测试枚举")]
        private TestEnum _testEnum = TestEnum.A;

        void Start()
        {
            Debug.Log("test enum=" + _testEnum);
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("Test coroutine foreach"))
                {
                    var listTmp = new List<int>();
                    for (int i = 0; i < 100; ++i)
                    {
                        listTmp.Add(i);
                    }
                    shaco.Base.Coroutine.Foreach(listTmp, (object data) =>
                    {
                        System.Threading.Thread.Sleep(new System.TimeSpan(100));
                        return true;
                    },
                    (float percent) =>
                    {
                        Debug.Log("loading percent=" + percent);
                    }, 0.001f);
                }

                if (TestMainMenu.DrawButton("Test coroutine sequeue"))
                {
                    float time = 0;
                    float time2 = 0;
                    var c1 = new shaco.Base.Coroutine.SequeueCallBack(() =>
                    {
                        Debug.Log("do1");
                        time += Time.deltaTime;
                    }, () =>
                    {
                        return time > 1.0;
                    });
                    var c2 = new shaco.Base.Coroutine.SequeueCallBack(() =>
                    {
                        Debug.Log("do2");
                        time2 += Time.deltaTime;
                    }, () =>
                    {
                        return time2 > 1.0;
                    });
                    shaco.Base.Coroutine.Sequeue(c1, c2);
                }

                if (TestMainMenu.DrawButton("Test coroutine while"))
                {
                    int time = 0;
                    shaco.Base.Coroutine.While(() =>
                    {
                        return time <= 100;
                    }, () =>
                    {
                        time += 1;
                        Debug.Log("time=" + time);
                        System.Threading.Thread.Sleep(5);
                    }, 3);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("Test zip file"))
                {
                    var outputPath = Application.dataPath + "/../../test_zip_file.zip";
                    var currentPath = shaco.Base.FileHelper.GetCurrentSourceFilePath();
                    shaco.GameHelper.zip.Zip(currentPath, outputPath);
                    shaco.Base.FileHelper.DeleteByUserPath(outputPath);

                    Debug.Log("zip file success path=" + outputPath);
                }

                if (TestMainMenu.DrawButton("Test zip folder"))
                {
                    var outputPath = Application.dataPath + "/../../test_zip_folder.zip";
                    shaco.GameHelper.zip.Zip(shaco.Base.GlobalParams.GetShacoFrameworkRootPath().ContactPath("Base/Scripts/Unuse"), outputPath);
                    shaco.Base.FileHelper.DeleteByUserPath(outputPath);

                    Debug.Log("zip folder success path=" + outputPath);
                }

                if (TestMainMenu.DrawButton("Test unzip file"))
                {
                    var inputPath = Application.dataPath + "/../../test_zip_file.zip";
                    var outputPath = Application.dataPath + "/../../test_unzip_file.zip";
                    shaco.GameHelper.zip.UnZip(inputPath, outputPath, null, (float progress) =>
                    {
                        Debug.Log("progress=" + progress);

                        if (progress >= 1)
                            Debug.Log("unzip success path=" + outputPath);
                    });
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("Test write file async"))
                {
                    var outputPath = Application.dataPath + "/../../test_write_file_async.txt";
                    shaco.Base.FileHelper.WriteAllByUserPathAsync(outputPath, "test write data", (bool success) =>
                    {
                        Debug.Log("write end, success=" + success);
                    }, (float percent) =>
                    {
                        shaco.Log.Info("write percent=" + percent);
                    });
                }

                if (TestMainMenu.DrawButton("Test read file async"))
                {
                    var currentPath = shaco.Base.FileHelper.GetCurrentSourceFilePath();
                    shaco.Base.FileHelper.ReadAllByteByUserPathAsync(currentPath, 0, 2561, (byte[] bytes) =>
                    {
                        if (!bytes.IsNullOrEmpty())
                        {
                            Debug.Log("read end, bytes.Length=" + bytes.Length);

                            shaco.Base.FileHelper.ReadAllByteByUserPathAsync(currentPath, 2561, 2000, (byte[] bytes2) =>
                            {
                                if (!bytes2.IsNullOrEmpty())
                                {
                                    Debug.Log("read end, bytes.Length2=" + bytes2.Length);
                                }

                            }, (float percent) =>
                            {
                                shaco.Log.Info("read percent2=" + percent);
                            });
                        }

                    }, (float percent) =>
                    {
                        shaco.Log.Info("read percent=" + percent);
                    });
                }
            }
            GUILayout.EndHorizontal();

            if (TestMainMenu.DrawButton("Test Bad Words"))
            {
                //建议使用异步加载屏蔽字库的方式
                shaco.Base.BadWordsFilter.Instance.LoadFromResourcesOrLocal("bad_words", (progress) =>
                {
                    Debug.Log("p=" + progress);

                    if (progress >= 1)
                    {
                        Debug.Log("have bad word=" + shaco.Base.BadWordsFilter.Instance.HasBadWords("fuck---_*"));
                        Debug.Log("get bad word=" + shaco.Base.BadWordsFilter.Instance.Filter("fuck你好shsit"));
                    }
                });

                //同步加载屏蔽字库
                // shaco.Base.BadWordsFilter.LoadFromFile(Application.dataPath + "/Resources/Shielded font library.txt");
                // Debug.Log("have bad word=" + shaco.Base.BadWordsFilter.HasBadWords("fuck---_*"));
                // Debug.Log("get bad word=" + shaco.Base.BadWordsFilter.Filter("fuck你好"));
            }

            GUILayout.BeginHorizontal();
            {
                // if (TestMainMenu.DrawButton("Init with async"))
                // {
                //     shaco.ExcelData.cfg_avatar.Instance.CheckInitAsync(percent =>
                //     {
                //         Debug.Log("load percent=" + percent);
                //     });
                // }

                // if (TestMainMenu.DrawButton("Test foreach config(sync)"))
                // {
                //     Debug.Log("config count=" + shaco.ExcelData.cfg_avatar.Count);
                //     foreach (var iter in shaco.ExcelData.cfg_avatar.Instance)
                //     {
                //         Debug.Log("TemplateID=" + iter.TemplateID);
                //     }   
                // }

                // if (TestMainMenu.DrawButton("Test get value"))
                // {
                //     var valueTmp = shaco.ExcelData.cfg_avatar.Get(1);
                //     Debug.Log("find config value=" + valueTmp.GachaOnly);
                // }

                // if (TestMainMenu.DrawButton("Test contains key(or index)"))
                // {
                //     Debug.Log("contains config key=" + shaco.ExcelData.cfg_avatar.ContainsKey(0));
                // }
            }
            GUILayout.EndHorizontal();

            TestMainMenu.DrawBackToMainMenuButton();
        }
    }
}