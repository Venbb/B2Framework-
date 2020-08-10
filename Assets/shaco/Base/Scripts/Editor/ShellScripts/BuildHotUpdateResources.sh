#!/bin/sh

rootPathTmp=$(cd `dirname $0`;pwd)
build_function_tmp="shacoEditor.HotUpdateExportWindow.BuildResourcesShell"
EXPORT_PROJECT_PATH=${rootPathTmp%/Assets/*}
EXPORT_BATCH_BUILD_LOG_PATH="${EXPORT_PROJECT_PATH%/}/batch_build_resources.log"

echo "EXPORT_PROJECT_PATH=$EXPORT_PROJECT_PATH"
echo "EXPORT_BATCH_BUILD_LOG_PATH=$EXPORT_BATCH_BUILD_LOG_PATH"
echo "build_function_tmp=$build_function_tmp"
echo "IMPORT_PATH=$IMPORT_PATH"
echo "EXPORT_PATH=$EXPORT_PATH"
echo "EXPORT_OVERWRITE=$EXPORT_OVERWRITE"
echo "EXPORT_AUTO_ENCRYT=$EXPORT_AUTO_ENCRYT"
echo "EXPORT_AUTO_COMPRESS=$EXPORT_AUTO_ENCRYT"
echo "EXPORT_ALL_IN_ONE_AB=$EXPORT_ALL_IN_ONE_AB"

#如果设置了机器人，告之现在开始打包了
bash "$rootPathTmp/SendMessage.sh" "Build resources start...\n$EXPORT_PROJECT_PATH"

if [[ $DONT_AUTO_PROJECT_UPDATE == 1 ]]; then
     echo "BuildHotUpdateResources dont auto update project..."
else
     #更新git项目
     bash "$rootPathTmp/CheckProjectUpdate.sh" "Github" "DiscardAllLocalChanges"
fi

$EXPORT_UNITY_PATH -quit -batchmode \
            -projectPath $EXPORT_PROJECT_PATH \
            -logFile $EXPORT_BATCH_BUILD_LOG_PATH \
            -executeMethod "$build_function_tmp" \
            -arg "IMPORT_PATH=$IMPORT_PATH" \
            -arg "EXPORT_PATH=$EXPORT_PATH" \
            -arg "EXPORT_OVERWRITE=$EXPORT_OVERWRITE" \
            -arg "EXPORT_AUTO_ENCRYT=$EXPORT_AUTO_ENCRYT" \
            -arg "EXPORT_AUTO_COMPRESS=$EXPORT_AUTO_COMPRESS" \
            -arg "EXPORT_ALL_IN_ONE_AB=$EXPORT_ALL_IN_ONE_AB" \
            -arg "EXPORT_AUTO_BUILD_LOG=$EXPORT_AUTO_BUILD_LOG"

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
check_success_txt_path=$EXPORT_PATH/"is_build_resources_success_tmp.txt"
if [ "$REBOT_TOKEN" != "" ] && [ $EXPORT_AUTO_BUILD_LOG == 1 ]; then

     echo "will send build log"
     if [[ -f $check_success_txt_path ]]; then

          build_log_path=$EXPORT_PATH/"BuildResources.log"
          build_text_tmp=$(sed s/$/"\\\n"/ $build_log_path | tr -d '\n')

        #   build_text_tmp=$(cat $build_log_path)
        #   build_text_tmp="$build_text_tmp"

          echo "send build hotupdate resources success message"
          bash "$rootPathTmp/SendMessage.sh" "<Build resources log.>\n\n$build_text_tmp" | xargs

          #删除打包成功标记文件
          rm $check_success_txt_path
     else
          echo "send build hotupdate resources failed message"
         
          #获取svn地址方法
          # cd $EXPORT_PATH
          # svn_url_tmp=$(svn info | grep 'URL: svn://' | awk '{print $2}')
          # svn_url_tmp="${svn_url_tmp#*@} 

          #获取本机ip地址
          # current_ip_address=$(ifconfig -a|grep inet|grep -v 127.0.0.1|grep -v inet6|awk '{print $2}'|tr -d "addr:")
          bash "$rootPathTmp/SendMessage.sh" "<Build resources error.>\n【Log path】\n$EXPORT_BATCH_BUILD_LOG_PATH"
     fi
else
     echo "don't send buid log"
fi

#删除打包是否成功标记文件
if [[ -f $check_success_txt_path ]]; then
     rm -rf $check_success_txt_path
fi

#打包完毕上传svn
bash "${rootPathTmp}/UploadAllToSvn.sh" $EXPORT_PATH $EXPORT_SVN_USER_NAME $EXPORT_SVN_PASSWORD