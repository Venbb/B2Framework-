#!/bin/sh

#------------------------------------------------------------------------------------------------
#打资源包基础参数，必填项
#------------------------------------------------------------------------------------------------

#Unity App - 打包使用的Unity可执行文件所在路径
EXPORT_UNITY_PATH="/Applications/Unity2018.4.5f1/Unity2018.4.5f1.app/Contents/MacOS/Unity"
export EXPORT_UNITY_PATH

#导入文件夹
#允许一次打包多个文件夹，路径用英文逗号隔开
IMPORT_PATH="/Users/liuchang/Desktop/PitayaGithub/shacogameframework/shaco/Assets/Test/Resources"
# IMPORT_PATH="$IMPORT_PATH,Resources_HotUpdate/ADV"
export IMPORT_PATH

#导出文件夹
EXPORT_PATH="/Users/liuchang/Desktop/VersionControl@@Android"
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

#是否覆盖原导出目录
EXPORT_OVERWRITE=1
export EXPORT_OVERWRITE

#是否自动加密打包资源
EXPORT_AUTO_ENCRYT=1
export EXPORT_AUTO_ENCRYT

#是否自动压缩打包资源
EXPORT_AUTO_COMPRESS=1
export EXPORT_AUTO_COMPRESS

#是否自动生成打包日志
EXPORT_AUTO_BUILD_LOG=1
export EXPORT_AUTO_BUILD_LOG

#是否打包为1个assetbundle
#0：1个文件打1个assetbundle
#1：所有文件打1个assetbundle
EXPORT_ALL_IN_ONE_AB=0
export EXPORT_ALL_IN_ONE_AB

rootPathTmp=$(cd `dirname $0`;pwd)

#执行打包脚本
bash "${rootPathTmp}/../BuildHotUpdateResources.sh"