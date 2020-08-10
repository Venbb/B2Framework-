#!/bin/sh

SVN_ROOT_PATH=$1
if [[ $SVN_ROOT_PATH == "" ]]; then
    echo "svn root path is empty..."
    exit 1
fi


EXPORT_SVN_USER_NAME=$2
EXPORT_SVN_PASSWORD=$3

#如果目录支持svn则直接同步并上传
rootPathTmp=$(cd `dirname $0`;pwd)
cd $SVN_ROOT_PATH

echo "upload SVN_ROOT_PATH=$SVN_ROOT_PATH"
echo "will upload all to svn, is_support_svn="$is_support_svn
echo "EXPORT_SVN_USER_NAME=$EXPORT_SVN_USER_NAME"
echo "EXPORT_SVN_PASSWORD=$EXPORT_SVN_PASSWORD"

if [[ $EXPORT_SVN_USER_NAME == "" || $EXPORT_SVN_PASSWORD == "" ]]; then
    echo "dont use svn"

    #上传svn结束
    bash "$rootPathTmp/SendMessage.sh" "svn not use\n【Export Path】\n$SVN_ROOT_PATH"
    exit 1
fi

#确定导出目录是否支持svn
is_support_svn=0
if [[ $(svn status) == "" ]]; then
    
    #查找上级目录是否支持svn
    SVN_ROOT_PATH=${SVN_ROOT_PATH%/*}
    cd $SVN_ROOT_PATH
    if [[ $(svn status) == "" ]]; then
        #不再继续查找了
        is_support_svn=0
    else
        is_support_svn=1
    fi
else
    is_support_svn=1
fi

echo "final upload SVN_ROOT_PATH=$SVN_ROOT_PATH"
echo "is_support_svn=$is_support_svn"

#上传所有文件到svn
if [[ $is_support_svn == 1 ]]; then

    echo "UploadAllToSvn start..."

    #新增文件
    svn_add_str_list=$(svn status | grep '?' | awk -F '?' '{gsub(/^\ +/, "", $2);print "\""$2"@\""}')
    if [[ "$svn_add_str_list" != "" ]]; then
        echo "add svn files..."
        svn status | grep '?' | awk -F '?' '{gsub(/^\ +/, "", $2);print "\""$2"@\""}' | xargs svn add
    fi

    #删除文件
    svn_delete_str_list=$(svn status | grep '!M' | awk -F '!M' '{gsub(/^\ +/, "", $2);print "\""$2"@\""}')
    if [[ "$svn_delete_str_list" != "" ]]; then
        echo "delete svn files..."
        svn status | grep '!M' | awk -F '!M' '{gsub(/^\ +/, "", $2);print "\""$2"@\""}' | xargs svn delete --force
    fi
    
    #删除文件
    svn_delete_str_list=$(svn status | grep '!' | awk -F '!' '{gsub(/^\ +/, "", $2);print "\""$2"@\""}')
    if [[ "$svn_delete_str_list" != "" ]]; then
        echo "delete svn files..."
        svn status | grep '!' | awk -F '!' '{gsub(/^\ +/, "", $2);print "\""$2"@\""}' | xargs svn delete --force
    fi

    #更新svn
    svn cleanup --username $EXPORT_SVN_USER_NAME --password $EXPORT_SVN_PASSWORD --no-auth-cache
    svn update --username $EXPORT_SVN_USER_NAME --password $EXPORT_SVN_PASSWORD --no-auth-cache

    #提交文件
    commit_message_tmp="auto build hotupdate resources, $(date)"
    svn commit -m "$commit_message_tmp" --username $EXPORT_SVN_USER_NAME --password $EXPORT_SVN_PASSWORD --no-auth-cache

    echo "UploadAllToSvn end..."
fi

#上传svn结束
bash "$rootPathTmp/SendMessage.sh" "Svn upload success...\n【Svn Path】\n$SVN_ROOT_PATH"