#!/bin/sh

#测试参数
# is_upload_success_txt_tmp=1
# REBOT_TOKEN="7be7872060214bdaf665eed27e76812159f0f5db874329ac3b9d68ca461568b5"
# UPLOAD_PRODUCT_NAME="测试名字"
# UPLOAD_BUNDLE_ID="com.shaco.test"
# UPLOAD_BUNDLE_VERSION="1.0.0"
# UPLOAD_BUILD_CODE="1"
# download_path_tmp="https://www.baidu.com"
# IS_BUILD_DEBUG="true"
# BUILD_SERVER="DEV"
# BUILD_CHANNEL="Default"
# UPLOAD_PACKAGE_PATH="Desktop/Test.apk"

rootPathTmp=$(cd `dirname $0`;pwd)

#没有token直接跳过
if [ "$REBOT_TOKEN" == "" ]; then
    exit 1
fi

debug_flag_string_tmp=""
if [[ $IS_BUILD_DEBUG = "true" ]]; then
  debug_flag_string_tmp="Debug"
else
  debug_flag_string_tmp="Release"
fi

#获取文件下载地址
download_txt_path_tmp=$rootPathTmp/"download_path_tmp.txt"
if [[ -f $download_txt_path_tmp ]]; then
     download_path_tmp=$(cat $download_txt_path_tmp)
     rm -rf $download_txt_path_tmp
     echo "SendDingDingMessage delete path1=$download_txt_path_tmp"
else
     download_path_tmp=""
fi

#获取上传包是否成功标记
is_upload_success_txt_tmp=$rootPathTmp/"is_upload_success_txt_tmp.txt"
if [[ -f $is_upload_success_txt_tmp ]]; then
     is_upload_success_tmp=$(cat $is_upload_success_txt_tmp)
     rm -rf $is_upload_success_txt_tmp
     echo "SendDingDingMessage delete path2=$is_upload_success_txt_tmp"
else
     is_upload_success_tmp=0
fi

#如果上传失败，可能是打的文件夹，例如Xcode目录，这个时候钉钉只需要发送Xcode打包成功的路径信息
if [ "$is_upload_success_tmp" != 1 ]; then
     if [[ -d $UPLOAD_PACKAGE_PATH ]]; then
          is_upload_success_tmp=1
     fi     
fi

echo "is_upload_success_tmp=$is_upload_success_tmp"
echo "download_txt_path_tmp=$download_txt_path_tmp"

if [ "$is_upload_success_tmp" == 1 ]; then

     msg_tmp="Build Success Information:\nPackage Path:$UPLOAD_PACKAGE_PATH\n【Download Url】$download_path_tmp\n【Version】$UPLOAD_BUNDLE_VERSION\n【Build Code】$UPLOAD_BUILD_CODE\n【Debug Mode】$IS_BUILD_DEBUG\n【Channel】$BUILD_CHANNEL\n【Server】$BUILD_SERVER\n【GlobalDefines】$UPLOAD_GLOBAL_DEFINES"
     echo "send build success messge=""$msg_tmp"
     echo "REBOT_TOKEN=$REBOT_TOKEN"
     bash "$rootPathTmp/SendMessage.sh" "$msg_tmp"

else
     echo "Upload failed
          BUILD_CHANNEL=$BUILD_CHANNEL
          BUILD_SERVER=$BUILD_SERVER
          BuildTargetAndFunction=$BUILD_TARGET.$BUILD_FUNCTION
          ProjectPath=$PROJECT_PATH"
          
     #文件存在，判定为上传失败
     if [[ -f $UPLOAD_PACKAGE_PATH || -d $UPLOAD_PACKAGE_PATH ]]; then
          echo "Upload failed"

          bash "$rootPathTmp/SendMessage.sh" "Upload failed($PROJECT_PATH)"

     #文件不存在，判定为打包失败
     else
          echo "Build failed SVN_UPLOAD_PATH=$SVN_UPLOAD_PATH"

          #获取打包日志上传到svn目录中
          if [[ $SVN_UPLOAD_PATH != "" ]]; then

               #如果拷贝目录不存在则创建一个
               if [[ ! -d $SVN_UPLOAD_PATH ]]; then
                    mkdir -p $SVN_UPLOAD_PATH
               fi
               
               current_date_tmp=`date +%Y-%m-%d_%H-%M-%S`
               log_file_name_tmp="build_error_$current_date_tmp.log"
               copy_log_path_tmp=$SVN_UPLOAD_PATH/$log_file_name_tmp
               cp $BATCH_BUILD_LOG_PATH $copy_log_path_tmp

               echo "copy from log path=$BATCH_BUILD_LOG_PATH"
               echo "copy to log path=$copy_log_path_tmp"

               #该目录如果支持svn则上传日志到svn上
               if [[ $(find . -name .svn -maxdepth 1 -type d) != "" ]]; then
                    svn add $copy_log_path_tmp
                    svm commit -m "build error log" --username $SVN_USER_NAME --password $SVN_PASSWORD --no-auth-cache
               fi
          fi

          bash "$rootPathTmp/SendMessage.sh" "Build failed\n$UPLOAD_PREFIX_URL/$log_file_name_tmp"

          # #获取日志最后50行内容，并发送到钉钉中
          # bash "$rootPathTmp/SendMessage.sh" "Build failed($PROJECT_PATH)\nBuild log:\n\n\n$build_log_tmp"
     fi
fi