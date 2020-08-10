#!/bin/sh

rootPathTmp=$(cd `dirname $0`;pwd)
build_function_tmp="shacoEditor.HotUpdateExportWindow.SyncResourcesFolder"
EXPORT_PROJECT_PATH=${rootPathTmp%/Assets/*}
EXPORT_BATCH_BUILD_LOG_PATH="$EXPORT_PROJECT_PATH/batch_build_resources.log"

echo "EXPORT_BATCH_BUILD_LOG_PATH=$EXPORT_BATCH_BUILD_LOG_PATH"
echo "build_function_tmp=$build_function_tmp"
echo "IMPORT_PATH=$IMPORT_PATH"
echo "EXPORT_PATH=$EXPORT_PATH"
echo "IS_EDITOR=$IS_EDITOR"

#如果设置了机器人，告之现在开始同步资源了
bash "$rootPathTmp/SendMessage.sh" "<Sync Resources Start>\n【IMPORT_PATH】$IMPORT_PATH"

#更新git项目
bash "$rootPathTmp/CheckProjectUpdate.sh" "Github" "DiscardAllLocalChanges"

$EXPORT_UNITY_PATH -quit -batchmode \
            -projectPath $EXPORT_PROJECT_PATH \
            -logFile $EXPORT_BATCH_BUILD_LOG_PATH \
            -executeMethod "$build_function_tmp" \
            -arg "IMPORT_PATH=$IMPORT_PATH" \
            -arg "EXPORT_PATH=$EXPORT_PATH" \
            -arg "IS_EDITOR=$IS_EDITOR" \
            -arg "IS_COPY_MANIFEST=$IS_COPY_MANIFEST" \
            -arg "IGNORE_FOLDER=$IGNORE_FOLDER"

#可能打包资源修改了导出目录，需要重新获取一次
export_path_check_file_path="$EXPORT_PROJECT_PATH/export_path_check_tmp.txt"
echo "export_path_check_file_path=$export_path_check_file_path"
if [[ -f $export_path_check_file_path ]]; then

    EXPORT_PATH=$(cat $export_path_check_file_path)
    rm -rf $export_path_check_file_path

    export_path_check_meta_path="$export_path_check_file_path.meta"
    if [[ -f export_path_check_meta_path ]]; then
        rm -rf $export_path_check_meta_path
    fi
fi

#自动发送打包日志
echo "will send sync resources log"
bash "$rootPathTmp/SendMessage.sh" "<Sync resources success.>\n【EXPORT_PATH】$EXPORT_PATH"

#打包完毕上传svn
bash "${rootPathTmp}/UploadAllToSvn.sh" $EXPORT_PATH $EXPORT_SVN_USER_NAME $EXPORT_SVN_PASSWORD