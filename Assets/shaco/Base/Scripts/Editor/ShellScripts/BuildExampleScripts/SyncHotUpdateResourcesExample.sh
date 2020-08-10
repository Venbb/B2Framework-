#!/bin/sh

#------------------------------------------------------------------------------------------------
#打资源包基础参数，必填项
#------------------------------------------------------------------------------------------------

#Unity App - 打包使用的Unity可执行文件所在路径
EXPORT_UNITY_PATH="/Applications/Unity2018.4.5f1/Unity2018.4.5f1_2.app/Contents/MacOS/Unity"
export EXPORT_UNITY_PATH

#要合并的资源文件夹路径
IMPORT_PATH="/Users/liuchang/Desktop/1"
export IMPORT_PATH

#被合并的资源文件夹路径
EXPORT_PATH="/Users/liuchang/Desktop/2"
export EXPORT_PATH

#------------------------------------------------------------------------------------------------
#打资源包可选参数
#------------------------------------------------------------------------------------------------

#Svn配置信息
#打包完成上传svn账号和密码(前提是要EXPORT_PATH本身是.svn文件夹所在根目录)
EXPORT_SVN_USER_NAME=""
EXPORT_SVN_PASSWORD=""
export EXPORT_SVN_USER_NAME
export EXPORT_SVN_PASSWORD

#机器人token，可以通知打包进度
REBOT_TOKEN=""
export REBOT_TOKEN

#编辑器模式：匹配MD5文件名，非编辑器模式：匹配原文件名
IS_EDITOR=1
export IS_EDITOR

#是否同步每个文件的manifest
IS_COPY_MANIFEST=0
export IS_COPY_MANIFEST

rootPathTmp=$(cd `dirname $0`;pwd)

#执行打包脚本
bash "${rootPathTmp}/../SyncHotUpdateResources.sh"