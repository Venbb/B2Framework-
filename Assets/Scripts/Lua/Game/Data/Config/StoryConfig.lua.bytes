--[[
    UI :                     UIStory加载的子表现界面（如2人对话界面、故事动画界面）
    lang :                   下方文字（对话界面只支持一句，动画界面可以有多句）
    characterLeft :         （对话界面专用)，表示左边的人物资源
    characterRight :        （对话界面专用)，表示右边的人物资源
    characterSpeeking :     （对话界面专用)，1：左边人物在说，2：右边人物在说
    characterName:          （对话界面专用) 正在说话的人的名字
    chatBg:                  (对话界面专用) 人物对话的背景
    LRSpeeking:              （对话界面专用）正在说话的是左边的还是右边的, L左边，R右边
    otherCharacterHideType:  (对话界面专用) 一个人说话时，另一个的状态，0：隐藏，1：缩小变灰
    animations:              (故事动画界面专用) 播放哪个动画(数组表连续播放,最后一个做动画停留)
    langToAnimation:         (故事动画界面专用)一般点击是对话文字全显示或者切换下一段对话，这个表示对话和Animation的对应进度关系
                              当动画进度大于文字说明进度时候，无影响；当对话进度小于文字进度时，点击不仅是切换下一句文字，而且强制切换下一个动画
                              该数组元素个数需要和；lang相同，数字为当前对话对应的Animation下标
    BGM:                      (通用)，当前Step的背景音乐
    SpeekAudio                (通用)，对话或者动画剧情的音频(如果有的话，数量需要和lang的数量一致)
]]

--TODO:等lang换成key
local StoryConfig = {
    [1] = {
        needLevel = 1, 
        step = {
            [1] = {
                UI = "Prefabs/UI/Story/Story1_1.prefab", 
                lang = {"在成都天府三街的地铁站B口，传说封印着一个大恶魔。然而日复一日，人们渐渐的忘却这个传说。就在某个月圆之夜，突然一阵电闪雷鸣，大魔王彪哥突破封印了！",
                        "当日，那是地壳开裂，火山喷发，伴随着世界的轰鸣，彪哥出现了！！！！"}, 
                animations = {"Story1_1_1", "Story1_1_2"}, 
                langToAnimation = {1, 2},
                BGM = "Audio/Story1.ogg",
                SpeekAudio = {"Audio/story_dlg1.ogg", "Audio/story_dlg2.ogg"}
            },
            [2] = {
                UI = "Prefabs/UI/Story/Story1_1.prefab", 
                lang = {"当时彪哥为了补充被封印这些年损失的力量，抓走了他目光所见的无数少女！"}, 
                animations = {"Story1_1_3", "Story1_1_4"},
                SpeekAudio = {"Audio/story_dlg3.ogg"}
            },
            [3] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "L",
                lang = {"报告长官！为了拯救苍生！我请求出战！"}, 
                characterLeft = "Textures/test/hanshuo.png", 
                characterName = "韩硕士兵", 
                otherCharacterHideType = 0, 
                chatBg = "Textures/test/story1_1_2bg.png",
                BGM = "Audio/Story2.ogg",
                SpeekAudio = {"Audio/hanshuo1.ogg"}
            },
            [4] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "R",
                lang = {"不行！！！彪哥太强大了，更何况他现在吸收了千万少女的力量，我们根本无法抗衡！"}, 
                characterRight = "Textures/test/shulin.png", 
                characterName = "树林队长", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                SpeekAudio = {"Audio/shulin1.ogg"}
            },
            [5] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "L",
                lang = {"不！如果不现在打败彪哥！等他再吸食更多的少女，整个人类都将会毁灭的 :::>_<:::"}, 
                characterLeft = "Textures/test/hanshuo.png", 
                characterName = "韩硕士兵", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                SpeekAudio = {"Audio/hanshuo2.ogg"}
            },
            [6] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "R",
                lang = {"啊呀！！这可如何是好。。。这可如何是好。。。。。。"}, 
                characterRight = "Textures/test/shulin.png", 
                characterName = "树林队长", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                SpeekAudio = {"Audio/shulin2.ogg"}
            },
            [7] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "R",
                lang = {"好！！！！！！！！我答应你的需求！"}, 
                characterRight = "Textures/test/shulin.png", 
                characterName = "树林队长", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                SpeekAudio = {"Audio/shulin3.ogg"}
            },
            [8] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "L",
                lang = {"多谢长官。。。。。。德玛西亚！！！！"}, 
                characterLeft = "Textures/test/hanshuo.png", 
                characterName = "韩硕士兵", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                SpeekAudio = {"Audio/hanshuo3.ogg"}
            },
            [9] = {
                UI = "Prefabs/UI/Story/Story1_1.prefab", 
                lang = {"哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈..."}, 
                animations = {"Story1_1_4"},
                BGM = "Audio/Story1.ogg",
                SpeekAudio = {"Audio/biaoge1.ogg"}
            },
            [10] = {
                UI = "Prefabs/UI/Story/Story1_1.prefab", 
                lang = {"再过不久我就要称霸全世界！！！！！"}, 
                animations = {"Story1_1_4"},
                SpeekAudio = {"Audio/biaoge2.ogg"}
            },
            [11] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "R",
                lang = {"Emm...............................韩硕士兵！"}, 
                characterRight = "Textures/test/shulin.png", 
                characterName = "树林队长", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                BGM = "Audio/Story2.ogg",
                SpeekAudio = {"Audio/shulin4.ogg"}
            },
            [12] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "L",
                lang = {"在！！！！！！！！！！！！！！"}, 
                characterLeft = "Textures/test/hanshuo.png", 
                characterName = "韩硕士兵", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                SpeekAudio = {"Audio/hanshuo4.ogg"}
            },
            [13] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "R",
                lang = {"世界和平就靠你了，出发！！！！！！！！！！！！！"}, 
                characterRight = "Textures/test/shulin.png", 
                characterName = "树林队长", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                SpeekAudio ={"Audio/shulin5.ogg"}
            },
            [14] = {
                UI = "Prefabs/UI/Story/Story_Dialog.prefab", 
                LRSpeeking = "L",
                lang = {"是！！！！！！！！！！！！！！"}, 
                characterLeft = "Textures/test/hanshuo.png", 
                characterName = "韩硕士兵", 
                otherCharacterHideType = 1, 
                chatBg = "Textures/test/story1_1_2bg.png",
                SpeekAudio = {"Audio/hanshuo5.ogg"}
            },
        }
    },
    [2] = {
        needLevel = 99999, 
        step = {
            
        }
    }
}
return StoryConfig