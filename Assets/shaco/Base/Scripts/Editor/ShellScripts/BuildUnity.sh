#!/bin/sh
echo "\n<<<<<<build unity start !>>>>>>\n"

#在Mac OS环境下如果使用了crontab自动化命令，则需要刷新下环境变量路径，否则jq等第三方库路径找不到
source /etc/profile

#当前sh文件所在文件夹目录
rootPathTmp=$(cd `dirname $0`;pwd)

PROJECT_PATH=${rootPathTmp%/Assets/*}
batch_pre_build_log_path="$PROJECT_PATH/batch_pre_build.log"
BATCH_BUILD_LOG_PATH="$PROJECT_PATH/batch_build.log"
build_function_1_tmp="shacoEditor.BuildHelperWindow.StartBuildProcessWithShell"
build_function_2_tmp="shacoEditor.BuildHelperWindow.UpdateProjectDefinesEndCallBack"

echo "UNITY_PATH=$UNITY_PATH"
echo "PROJECT_PATH=$PROJECT_PATH"
echo "build_function_tmp=$build_function_tmp"
echo "BUILD_CHANNEL=$BUILD_CHANNEL"
echo "BUILD_SERVER=$BUILD_SERVER"

#如果设置了机器人，告之现在开始打包了
#小人转圈图
#http://img.b7.cn/file/20181128/bc82e738d2a2ccdc37df50882224dec4.gif
bash "$rootPathTmp/SendMessage.sh" "Build unity package start\n$PROJECT_PATH"

#检查是否已经打开过unity了，强制关闭它，否则会打包失败
opened_unity_pid_tmp=$(ps -ef | grep $UNITY_PATH | grep grep -v | awk '{print $2}')
if [[ $opened_unity_pid_tmp != "" ]]; then
    kill -9 $opened_unity_pid_tmp
fi

#打开unity，开始执行打包函数，并编译代码和刷新宏
$UNITY_PATH -quit -batchmode \
            -projectPath $PROJECT_PATH \
            -logFile $batch_pre_build_log_path \
            -executeMethod "$build_function_1_tmp" \
            -arg "BUILD_CHANNEL=$BUILD_CHANNEL" \
            -arg "BUILD_SERVER=$BUILD_SERVER" \
            -arg "IS_BUILD_DEBUG=$IS_BUILD_DEBUG" \
            -arg "IS_BUILD_XCODE=$IS_BUILD_XCODE" \
            -arg "BUILD_TARGET=$BUILD_TARGET"

#编译代码和刷新宏结束，继续打包
$UNITY_PATH -quit -batchmode \
            -projectPath $PROJECT_PATH \
            -logFile $BATCH_BUILD_LOG_PATH \
            -executeMethod "$build_function_2_tmp"

result_path=$PROJECT_PATH"/shell_build_result.txt"

#读取打包后的json文件，获取打包信息
if [[ -f $result_path ]]; then
    UPLOAD_BUNDLE_ID=$(cat $result_path | grep bundle_id= | awk -F '=' {'printf $2'})
    UPLOAD_BUNDLE_VERSION=$(cat $result_path | grep bundle_version= | awk -F '=' {'printf $2'})
    UPLOAD_BUILD_CODE=$(cat $result_path | grep bundle_code= | awk -F '=' {'printf $2'})
    FIR_UPLOAD_PLATFORM_TYPE=$(cat $result_path | grep platform= | awk -F '=' {'printf $2'})
    UPLOAD_PACKAGE_PATH=$(cat $result_path | grep package_path= | awk -F '=' {'printf $2'})
    UPLOAD_PRODUCT_NAME=$(cat $result_path | grep product_name= | awk -F '=' {'printf $2'})
    UPLOAD_GLOBAL_DEFINES=$(cat $result_path | grep global_defines= | awk -F '=' {'printf $2'})

    echo "------------------------------------------------------"

    echo "PROJECT_PATH=$PROJECT_PATH"
    echo "UPLOAD_BUNDLE_ID=$UPLOAD_BUNDLE_ID"
    echo "UPLOAD_BUNDLE_VERSION=$UPLOAD_BUNDLE_VERSION"
    echo "UPLOAD_BUILD_CODE=$UPLOAD_BUILD_CODE"
    echo "FIR_UPLOAD_PLATFORM_TYPE=$FIR_UPLOAD_PLATFORM_TYPE"
    echo "UPLOAD_PACKAGE_PATH=$UPLOAD_PACKAGE_PATH"
    echo "FIR_UPLOAD_API_TOKEN=$FIR_UPLOAD_API_TOKEN"
    echo "SVN_UPLOAD_PATH=$SVN_UPLOAD_PATH"

    # #删除临时打包信息文件
    # rm -rf $result_path
    # echo "BuildUnity delete path=$result_path"
fi

export PROJECT_PATH
export BATCH_BUILD_LOG_PATH
export UPLOAD_BUNDLE_ID
export UPLOAD_BUNDLE_VERSION
export UPLOAD_BUILD_CODE
export FIR_UPLOAD_PLATFORM_TYPE
export UPLOAD_PACKAGE_PATH
export UPLOAD_PRODUCT_NAME
export UPLOAD_GLOBAL_DEFINES

echo "\n<<<<<<build unity end !>>>>>>\n"

if [[ $FIR_UPLOAD_API_TOKEN != "" ]]; then
    bash "${rootPathTmp}/UpLoadToFir.sh"
fi
if [[ $SVN_UPLOAD_PATH != "" ]]; then
    bash "${rootPathTmp}/UpLoadToSvn.sh"
fi

bash "${rootPathTmp}/SendDingDingMessage.sh"
bash "${rootPathTmp}/DeleteNotCurrentPackages.sh"