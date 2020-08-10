# !/bin/sh

#同步项目中设置过的excel导出csv目录下所有csv文件为asset序列化文件
#建议在真机都使用asset文件，这会比读取csv更加高效和节省内存

rootPathTmp=$(cd `dirname $0`;pwd)
PROJECT_PATH=${rootPathTmp%/Assets/*}
BATCH_BUILD_LOG_PATH="$PROJECT_PATH/batch_build.log"

bash "$rootPathTmp/SendMessage.sh" "【Start】Sync all excel csv to unity asset\n$PROJECT_PATH"

if [[ $DONT_AUTO_PROJECT_UPDATE == 1 ]]; then
     echo "SyncAllExcelAsset dont auto update project..."
else
     #更新git项目
     bash "$rootPathTmp/CheckProjectUpdate.sh" "Github" "DiscardAllLocalChanges"
fi

$UNITY_PATH -quit -batchmode \
            -projectPath $PROJECT_PATH \
            -logFile $BATCH_BUILD_LOG_PATH \
            -executeMethod "shacoEditor.CreateExcelScriptMenu.SyncAllExcelAsset" \

bash "$rootPathTmp/SendMessage.sh" "【End】Sync all excel csv to unity asset\n$PROJECT_PATH"