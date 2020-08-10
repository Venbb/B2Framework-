#!/bin/sh

#------------------------------------------------------------------------------------------------
#以下是打包脚本通用方法和参数，一般不用做修改
#------------------------------------------------------------------------------------------------

#强制设定utf8格式，以保证非英文路径识别
export LC_ALL="en_US.UTF-8"

#打包方法名字 - 默认参数，可以不用修改
BUILD_FUNCTION="shacoEditor.BuildHelperWindow.StartBuildProcessWithShell"
export BUILD_FUNCTION

#获取当前根目录
rootPathTmp=$(cd `dirname $0`;pwd)

#遍历根目录目录，找到匹配的文件夹名字
#参数1：文件夹根目录
#参数2：查找文件夹名字
function FindTargetFolder()
{
    #判断结尾字符如果是本身查找目录直接返回
    if [[ "$1" == *$2 ]]; then
        echo $1
        return
    fi

    for element in $1/*;
    do  
        if [[ -d $element ]]; then

            #判断结尾字符是要找的目录，立即返回
            if [[ "$element" == *$2 ]]; then
            	echo $element
            	return
           	else
           		FindTargetFolder $element $2
            fi
        fi  
    done
}

SHELL_SCRIPT_PATH=$(FindTargetFolder $rootPathTmp "shaco/Base/Scripts/Editor/ShellScripts")

#如果查找目录为空则从上级目录递归查找
while [ "$SHELL_SCRIPT_PATH" == "" ]
do
    rootPathTmp=${rootPathTmp%/*}
    SHELL_SCRIPT_PATH=$(FindTargetFolder $rootPathTmp "shaco/Base/Scripts/Editor/ShellScripts")
done
export SHELL_SCRIPT_PATH

#获取项目支持更新方式，Svn or Github
PROJECT_UPDATE_TYPE="Github"
if [[ $(find . -name .svn -maxdepth 1 -type d) != "" ]]; then
    PROJECT_UPDATE_TYPE="Svn"
fi
export PROJECT_UPDATE_TYPE

#更新项目
#param1: 使用Github或者SVN更新项目
#param2: 更新模式，目前仅支持DiscardAllLocalChanges表示更新前回滚所有代码，如果为空，则不做任何处理
is_update_project_success_path_tmp=$SHELL_SCRIPT_PATH"/is_update_project_success_tmp.txt"
if [[ -f $is_update_project_success_path_tmp ]]; then
    #如果更新标记事先存在，则先删除它
    rm -f $is_update_project_success_path_tmp
fi

bash "$SHELL_SCRIPT_PATH/CheckProjectUpdate.sh" $PROJECT_UPDATE_TYPE "DiscardAllLocalChanges"

#判断项目是否更新失败
if [[ -f $is_update_project_success_path_tmp ]]; then
    rm -f $is_update_project_success_path_tmp
    echo "update project success"
else
    echo "update project error, see detail log"
    exit
fi

#执行打包脚本
bash "$SHELL_SCRIPT_PATH/BuildUnity.sh"