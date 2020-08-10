#!/bin/sh

#当前sh文件所在文件夹目录
rootPathTmp=$(cd `dirname $0`;pwd)

echo "\n<<<<<<will start upload to fir>>>>>>\n"

#没有token不上传包
if [ "$FIR_UPLOAD_API_TOKEN" == "" ]; then
    echo "don't upload package, no fir token"

#文件不存在不上传包
elif [[ ! -f $UPLOAD_PACKAGE_PATH ]]; then
    if [[ ! -d $UPLOAD_PACKAGE_PATH ]]; then
        echo "don't upload package, package is missing"
    else
        echo "don't upload package, package is folder"
    fi

#准备上传包
else

    #设置默认的平台为android
    if [ "$FIR_UPLOAD_PLATFORM_TYPE" = "" ]; then
        FIR_UPLOAD_PLATFORM_TYPE="android"
    fi

    #获取服务器请求
    result_tmp=$(curl -X "POST" "http://api.fir.im/apps" \
        -H "Content-Type: application/json" \
        -d "{\"type\":\"$FIR_UPLOAD_PLATFORM_TYPE\", \"bundle_id\":\"$UPLOAD_BUNDLE_ID\", \"api_token\":\"$FIR_UPLOAD_API_TOKEN\"}")
        
    path_tmp_json=$rootPathTmp"/tmp.json"

    #写入临时json文件到本地
    echo $result_tmp > $path_tmp_json

    # echo "------------------------------------------------------------------------------------------------------------------------------------------------------------------"
    # echo "result=$result_tmp"

    #获取服务器发来的信息
    key_binary_tmp=$(jq -r .cert.binary.key $path_tmp_json)
    token_binary_tmp=$(jq -r .cert.binary.token $path_tmp_json)
    url_tmp=$(jq -r .cert.binary.upload_url $path_tmp_json)

    #获取安装包short路径
    short_url_tmp=$(jq -r .short $path_tmp_json)
    download_path_tmp="https://fir.im/$short_url_tmp"
    
    #保存下载路径到本地文件中
    echo $download_path_tmp > $rootPathTmp/"download_path_tmp.txt"

    echo "------------------------------------------------------------------------------------------------------------------------------------------------------------------"

    echo "key=$key_binary_tmp"
    # echo "token=$token_binary_tmp"
    echo "file=$UPLOAD_PACKAGE_PATH"
    echo "x:name=$UPLOAD_PRODUCT_NAME($UPLOAD_BUNDLE_ID)"
    echo "x:build=$UPLOAD_BUILD_CODE"
    echo "x:version=$UPLOAD_BUNDLE_VERSION"

    #删除本地临时json文件
    rm -rf $path_tmp_json
    echo "UpLoadToFir delete path=$path_tmp_json"
        
    #上传安装包
    result_tmp=$(curl -F "key=$key_binary_tmp" \
        -F "token=$token_binary_tmp"\
        -F "file=@$UPLOAD_PACKAGE_PATH" \
        -F "x:name=$UPLOAD_BUNDLE_ID" \
        -F "x:build=$UPLOAD_BUILD_CODE" \
        -F "x:version=$UPLOAD_BUNDLE_VERSION" \
        $url_tmp)

    if [[ $result_tmp == *"completed"* ]]; then
        echo "<<<<<<<<<<<uploda success>>>>>>>>>>"
        
        #记录上传成功标记
        echo "1">$rootPathTmp/"is_upload_success_txt_tmp.txt"
    else
        echo "<<<<<<<<<<<uploda failed>>>>>>>>>>"
        echo "result=$result_tmp"
    fi
fi