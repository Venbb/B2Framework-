-----------------------------------------------------------
-- FileName:    AudioManager.lua
-- Author:      Administrator
-- date:        2020-06-24 13:53:04
-- 
-----------------------------------------------------------

local AudioManager = class("AudioManager")

function AudioManager:ctor(...)
    -- TODO:do something on ctor
    local root = GameObject.Find("AudioManager")
    if root == nil then
        ResMgr:LoadAssetAsync("Prefabs/Models/AudioManager.prefab", typeof(GameObject), function(go)
            root = GameObject.Instantiate(go)
            root.gameObject.name = "AudioManager"
            UnityEngine.Object.DontDestroyOnLoad(root)
            self._root = CommonFunc:GenerateTree(root.transform)
            self._BGSource = self._root.BG.gameObject:GetComponent(typeof(AudioSource))
            self._SESource = self._root.SE.gameObject:GetComponent(typeof(AudioSource))
        end)
    else
        UnityEngine.Object.DontDestroyOnLoad(root)
        self._root = CommonFunc:GenerateTree(root.transform)
        self._BGSource = self._root.BG.gameObject:GetComponent(typeof(AudioSource))
        self._SESource = self._root.SE.gameObject:GetComponent(typeof(AudioSource))
    end
    self.BGPath = nil
    self.BGPath_Loading = nil
    self.BGCache = {}
    self.SECache = {}
    -----------------------------------------------------------
    self.BGVolume = 1
    self.SEVolume = 1
end

function AudioManager:PlayBackgroundMusic(path, callback)
    local play = function(audioClip)
        if self._bgmGroupCo then
            CSCoroutine.stop(self._bgmGroupCo)
        end
        self._BGSource.clip = nil
        self._BGSource.clip = audioClip
        self._BGSource.volume = self.BGVolume
        if not self._BGSource.isPlaying then
            self._BGSource:Play()
        end
        if callback then
            callback(audioClip)
        end
    end

    if self.BGPath == nil or self.BGPath ~= path then
        if self.BGCache[path] then
            play(self.BGCache[path])
        else
            self.BGPath_Loading = path
            ResMgr:LoadAssetAsync(path, typeof(AudioClip), function(audioClip)
                if self.BGPath_Loading and self.BGPath_Loading ~= path then
                    return
                end
                self.BGPath_Loading = nil
                self.BGPath = path
                play(audioClip)
            end)
        end
    end
end

function AudioManager:playSound(path, singlePlay, isCache, callback)
    local play = function(audioClip)
        self._SESource.volume = self.SEVolume
        if singlePlay then
            self._SESource.clip = nil
            self._SESource.clip = audioClip
            self._SESource.volume = self.BGVolume
            if not self._SESource.isPlaying then
                self._SESource:Play()
            end
        else
            self._SESource:PlayOneShot(audioClip)
        end
        if self.SEVolume > 0 and audioClip.length > 1 then
            self:FadeOutBGMVolume(0.5, 0.5, audioClip.length)
        end
        if callback then
            callback(audioClip)
        end
    end

    if self.SECache[path] then
        play(self.SECache[path])
        return
    end

    ResMgr:LoadAssetAsync(path, typeof(AudioClip), function(audioClip)
        if isCache then
            self.SECache[path] = audioClip
            play(audioClip)
        end
    end)
end

function AudioManager:StopMusic()
    self._BGSource.clip = nil
end

function AudioManager:StopSound()
    self._SESource.clip = nil
    if self._bgmCo then
        CSCoroutine.stop(self._bgmCo)
    end
    self:FadeBackBGVolume(1)
end


function AudioManager:PlayBackgroundMusicGroup(pathes, loopIndex, nowPlayIndex)
    dump(pathes)
    assert(loopIndex <= #pathes, "最后循环的长度应该小于列表长度")
    if #pathes < 1 then
        return
    end
    nowPlayIndex = nowPlayIndex or 1
    self:PlayBackgroundMusic(pathes[nowPlayIndex], function(audioClip)
        if loopIndex > 1 then
            loopIndex = loopIndex - 1
            table.remove(pathes, 1)
            self._bgmGroupCo = CSCoroutine.start(function()
                coroutine.yield(UnityEngine.WaitForSeconds(audioClip.length))
                self._BGSource:Stop()
                self:PlayBackgroundMusicGroup(pathes, loopIndex)
            end)
        else
            nowPlayIndex = nowPlayIndex + 1
            if nowPlayIndex > #pathes then
                nowPlayIndex = 1
            end
            self:PlayBackgroundMusicGroup(pathes, loopIndex, nowPlayIndex)
        end
    end)
end

function AudioManager:LoadBG(path, callback)
    ResMgr:LoadAssetAsync(path, typeof(AudioClip), function(audioClip)
        print_e(path,audioClip)
        self.BGCache[path] = audioClip
        if callback then
            callback(path)
        end
        dump(self.BGCache)
    end)
end

function AudioManager:LoadSE(path, callback)
    ResMgr:LoadAssetAsync(path, typeof(AudioClip), function(audioClip)
        self.SECache[path] = audioClip
        if callback then
            callback(path)
        end
        dump(self.SECache)
    end)
end

function AudioManager:ClearCache_BG(path, callback)
    self.BGCache = {}
end

function AudioManager:ClearCache_SE(path, callback)
    self.SECache = {}
end

function AudioManager:UnLoadBG(path)
    -- GameObject.DestroyImmediate(self.BGCache[path], true)
    self.BGCache[path] = nil
    dump(self.BGCache)
end

function AudioManager:UnLoadSE(path)
    self.SECache[path] = nil
end

function AudioManager:FadeOutBGMVolume(volume, time, extime)
    if self._bgmCo then
        CSCoroutine.stop(self._bgmCo)
    end
    if not self._BGSource.isPlaying then
        return
    end
    volume = volume or 0.2
    if self._BGSource.volume > volume then
        time = time or self.fadeTime
        local fadeTime = time
        self._bgmCo = CSCoroutine.start(function()
            while fadeTime > 0 do
                fadeTime = fadeTime - Time.deltaTime
                local rate = (time - fadeTime) / time
                self._BGSource.volume = self.BGVolume * (1 - rate) + volume * rate
                coroutine.yield(UnityEngine.WaitForEndOfFrame())
            end
            if extime then
                extime = extime - time
                while extime > 0 do
                    extime = extime - Time.deltaTime
                    coroutine.yield(UnityEngine.WaitForEndOfFrame())
                end
                self:FadeBackBGVolume(1)
            end
        end)
    end
end

function AudioManager:FadeBackBGVolume(time)
    if self._bgmCo then
        CSCoroutine.stop(self._bgmCo)
    end
    if not self._BGSource.isPlaying then
        return
    end
    if self._BGSource.volume < self.BGVolume then
        time = time or self.fadeTime
        local fadeTime = time
        local volume = self._BGSource.volume
        self._bgmCo = CSCoroutine.start(function()
            while fadeTime > 0 do
                fadeTime = fadeTime - Time.deltaTime
                local rate = (time - fadeTime) / time
                self._BGSource.volume = volume * (1 - rate) + self.BGVolume * rate
                coroutine.yield(UnityEngine.WaitForEndOfFrame())
            end
        end)
    end
end

return AudioManager