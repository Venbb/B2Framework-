# !/bin/sh

# #Cos桶名字
# COS_BUCKET_NAME=dk5-stg-1259277404

# #Cos上传相对路径
# COS_UPLOAD_PATH="STG1/test/VersionControl@@Android"

# #同步目录
# EXPORT_PATH="/Users/liuchang/Desktop/2/1_0_9/VersionControl@@Android"

if [[ $COS_BUCKET_NAME == "" ]]; then
	echo "上传Cos错误: 缺少COS_BUCKET_NAME参数"
	exit 0
fi

if [[ $COS_UPLOAD_PATH == "" ]]; then
	echo "上传Cos错误: 缺少COS_UPLOAD_PATH参数"
	exit 0
fi

if [[ $EXPORT_PATH == "" ]]; then
	echo "上传Cos错误: 缺少EXPORT_PATH参数"
	exit 0
fi

#获取上传文件列表
updateFileList=$(find $EXPORT_PATH -type f | grep -v "/assets/" | grep -v ".DS_Store")
updateFilePath=$EXPORT_PATH"/update_file_list.txt"
updateAbList=$(cat $updateFilePath)

echo "请求的更新列表路径=$updateFilePath"

if [[ ! -f $updateFilePath ]]; then
	echo "无更新列表，默认上传整个目录, updateAbList=$updateAbList"
	coscmd -b $COS_BUCKET_NAME upload -rs $EXPORT_PATH $COS_UPLOAD_PATH
	exit 1
fi

if [[ $updateAbList == "" ]]; then
	echo "无更新内容，不上传"
	exit 1
fi

for iter in $updateFileList; do
	#获取相对路径
	relativePathTmp=${iter#$EXPORT_PATH}

	#上传文件到Cos
	uploadToPath=$COS_UPLOAD_PATH"/"$relativePathTmp
	echo "cos upload from=$iter"
	echo "cos upload to=$uploadToPath"
	coscmd -b $COS_BUCKET_NAME upload -s $iter $uploadToPath
done

for iter in $updateAbList; do
	#获取绝对路径
	fullPathTmp=$EXPORT_PATH"/"$iter

	#上传ab到Cos
	uploadToPath=$COS_UPLOAD_PATH"/"$iter
	echo "cos upload from=$fullPathTmp"
	echo "cos upload to=$uploadToPath"

	if [[ -d $fullPathTmp ]]; then
		coscmd -b $COS_BUCKET_NAME upload -rs $fullPathTmp $uploadToPath
	else
		coscmd -b $COS_BUCKET_NAME upload -s $fullPathTmp $uploadToPath
	fi
done

#删除更新列表文件
rm $updateFilePath
exit 1