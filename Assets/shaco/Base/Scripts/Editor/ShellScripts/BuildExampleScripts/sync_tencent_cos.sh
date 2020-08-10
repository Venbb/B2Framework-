#!/bin/sh

echo "即将同步本地资源到cos云资源服上，请输入要同步的资源版本号(example -> 1.0.0 or 1_0_0)"
read -a input_resource_verison
input_resource_verison=${input_resource_verison//./_}

if [[ $input_resource_verison == "" ]]; then
	echo "user canel"
	exit 1
fi

#------------------------------------------------------------------------------------------------
#同步参数
#------------------------------------------------------------------------------------------------

IMPORT_PATH="/Users/liuchang/Desktop/test/$input_resource_verison"
EXPORT_PATH="/Users/liuchang/Desktop/$input_resource_verison"

#cos云路径前缀
COS_PREFIX_PATH=""

#cos云桶名字
COS_BUCKET_NAME=""

#机器人token，可以通知打包进度
REBOT_TOKEN=""
export REBOT_TOKEN
 
#通知脚本路径
SEND_MESSAGE_SHELL_PATH=""


#------------------------------------------------------------------------------------------------
#开始同步资源并上传到cos云
#------------------------------------------------------------------------------------------------


#判断目录是否存在
if [[ ! -d $IMPORT_PATH ]]; then
	echo "not found import path=$IMPORT_PATH"
	exit 1
fi

if [[ ! -d $EXPORT_PATH ]]; then
	mkdir $EXPORT_PATH
fi

#通知开始同步
bash $SEND_MESSAGE_SHELL_PATH "Sync $COS_PREFIX_PATH start"

#拷贝文件
cd $IMPORT_PATH
versionControlFolders=$(find . -name "VersionControl*")
isSucces=0
for path in ${versionControlFolders[@]} 
do 
	path=${path#./}
	pastePath=$EXPORT_PATH"/"$path

	echo "copy from path=$path"
	echo "copy to path=$pastePath"

	#删除目标目录并重新拷贝过去
	rm -rf $pastePath
	rsync -av --progress --exclude "*.manifest" --exclude ".DS_Store*" $path"/" $pastePath"/"

	#上传cos云
	cosRelativePath=$COS_PREFIX_PATH/$input_resource_verison/$path
	if [[ $COS_BUCKET_NAME == "" ]]; then
		coscmd upload -rs $pastePath $cosRelativePath
	else
		coscmd -b $COS_BUCKET_NAME upload -rs $pastePath $cosRelativePath
	fi

	isSucces=1
done

#通知同步结束
if [[ $isSucces == 1 ]]; then
	bash $SEND_MESSAGE_SHELL_PATH "Sync $COS_PREFIX_PATH success"
else
	bash $SEND_MESSAGE_SHELL_PATH "Sync $COS_PREFIX_PATH faild !!!"
fi
