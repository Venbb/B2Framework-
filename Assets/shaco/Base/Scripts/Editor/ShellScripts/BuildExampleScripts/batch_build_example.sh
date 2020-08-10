#!/bin/sh

#------------------------------------------------------------------------------------------------
#打包基础参数，目前支持的所有参数都在下面了，可更具项目自身需求更改
#------------------------------------------------------------------------------------------------

#打包使用的Unity可执行文件所在路径
UNITY_PATH="/Applications/Unity2018.4.5f1/Unity2018.4.5f1.app/Contents/MacOS/Unity"
export UNITY_PATH

#打包平台名字
BUILD_CHANNEL="Default"
export BUILD_CHANNEL

#打包服务器环境名字
BUILD_SERVER="STG"
export BUILD_SERVER

#打包模式Debug or Release
#参数true or false
IS_BUILD_DEBUG="true"
export IS_BUILD_DEBUG

#ios专用，是否打包Xcode
#参数true or false，为false的时候会打包ipa
IS_BUILD_XCODE="true"
export IS_BUILD_XCODE

#以上参数为必填 !!!!
#------------------------------------------------------------------------------------------------
#下面参数为选填

#上传Fir包体托管平台的token - 从Fir官网个人信息中获取
#如果为空则不上传包到Fir平台
# FIR_UPLOAD_API_TOKEN="51c122030470e16c8b8dc010b86665de"
# export FIR_UPLOAD_API_TOKEN

#当前电脑上传的安装包svn目录
#如果为空则不上传包到svn上
SVN_UPLOAD_PATH=""
export SVN_UPLOAD_PATH

#svn账号和密码，在提交内容的时候使用
#如果使用密码且在事先没有设定过账号密码情况下，或者在crontab这种自动化环境下是无法正常上传svn文件的
SVN_USER_NAME=""
SVN_PASSWORD=""
export SVN_USER_NAME
export SVN_PASSWORD

#自定义下载地址前缀，可以为空
UPLOAD_PREFIX_URL=""
export UPLOAD_PREFIX_URL

#机器人的token
#如果为空则不发送机器人消息
REBOT_TOKEN=""
export REBOT_TOKEN

#------------------------------------------------------------------------------------------------
#下面参数不建议修改，已经默认自动化

#打包平台，一般来说根据当前项目所设定的平台自动设定，不需要更改
BUILD_TARGET="Automatic"
export BUILD_TARGET

#执行打包脚本
#这里推荐执行自动开始脚本，就不用手动指定打包脚本路径了
rootPathTmp=$(cd `dirname $0`;pwd)
bash "${rootPathTmp}/batch_build_auto_start.sh"