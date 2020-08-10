#!/bin/sh
rootPathTmp=$(cd `dirname $0`;pwd)

# SVN_UPLOAD_PATH="/Users/liuchang/Desktop/PitayaSVN/DK5/BuildPackages"
# UPLOAD_PACKAGE_PATH="/Users/liuchang/Desktop/1.txt"
# UPLOAD_PREFIX_URL="http://10.10.20.8:8888"
# BATCH_BUILD_LOG_PATH="/Users/liuchang/Desktop/1.log"

#记录上传成功标记
echo "<<<<<<will start upload to svn>>>>>>"

if [[ $SVN_UPLOAD_PATH != "" ]]; then

	#安装包丢失
	if [[ ! -f $UPLOAD_PACKAGE_PATH && ! -d $UPLOAD_PACKAGE_PATH ]]; then
		echo "upload svn failed: missing package, upload package path=$UPLOAD_PACKAGE_PATH"
	else

		#如果拷贝目录不存在则创建一个
		if [[ ! -d $SVN_UPLOAD_PATH ]]; then
			mkdir -p $SVN_UPLOAD_PATH
		fi

		#进入svn目录
		echo "cd SVN_UPLOAD_PATH=$SVN_UPLOAD_PATH"
		cd $SVN_UPLOAD_PATH

		#拷贝文件到svn目录
		filename_tmp=${UPLOAD_PACKAGE_PATH##*/}
		copy_package_path_tmp=$SVN_UPLOAD_PATH/$filename_tmp
		cp -R $UPLOAD_PACKAGE_PATH $copy_package_path_tmp

		#查看该目录是否支持svn操作
		if [[ $(find . -name .svn -maxdepth 1 -type d) != "" ]]; then
			echo "is svn folder="$SVN_UPLOAD_PATH
			is_support_svn_tmp=1
		else
			echo "not svn folder="$SVN_UPLOAD_PATH
			is_support_svn_tmp=0
		fi

		if [[ $is_support_svn_tmp == 1 ]]; then
			#上传包到svn
			svn update --username $SVN_USER_NAME --password $SVN_PASSWORD --no-auth-cache
			svn add $filename_tmp

			echo "svn add filename_tmp=$filename_tmp, and will commit it"
			echo "svn commit SVN_USER_NAME=$SVN_USER_NAME SVN_PASSWORD=$SVN_PASSWORD"

			commit_message_tmp="commit new package, version=$UPLOAD_BUNDLE_VERSION buildcode=$UPLOAD_BUILD_CODE channel=$BUILD_CHANNEL server=$BUILD_SERVER defines=$UPLOAD_GLOBAL_DEFINES"
			svn commit -m "$commit_message_tmp" --username $SVN_USER_NAME --password $SVN_PASSWORD --no-auth-cache
		else
			echo "not svn version control folder, copy file mode only"
		fi

		#删除7天以前的文件(只查找最上层目录)
		will_delete_paths_tmp=$(find $SVN_UPLOAD_PATH -mtime +7 -maxdepth 1)
		if [[ $will_delete_paths_tmp != "" ]]; then
			will_delete_path_array_tmp=($will_delete_paths_tmp) 
			for iter in ${will_delete_path_array_tmp[@]} 
			do 
				#忽略svn文件夹
				if [[ $iter =~ ".svn" ]]; then
					continue
				fi

				echo "svn will delete path=$iter"

				if [[ $is_support_svn_tmp == 1 ]]; then
					svn delete $iter"@" --force
				else
					#这里必须添加-rf循环删除，否则无法删除文件夹
					rm -rf $iter
					echo "svn delete path=$iter"
				fi
			done
			
			if [[ $is_support_svn_tmp == 1 ]]; then
				svn commit -m "delete packages for more than 7 days" --username $SVN_USER_NAME --password $SVN_PASSWORD --no-auth-cache
			fi
		fi

		#指定下载地址
		if [[ $UPLOAD_PREFIX_URL == *"{0}"* ]]; then
			download_path_tmp=${UPLOAD_PREFIX_URL/"{0}"/"$filename_tmp"}
		else
			download_path_tmp=$UPLOAD_PREFIX_URL/$filename_tmp
		fi

		#保存下载路径到本地文件中
		echo $download_path_tmp > $rootPathTmp/"download_path_tmp.txt"

		echo "Upload svn success: UPLOAD_PACKAGE_PATH=$UPLOAD_PACKAGE_PATH"
		echo "download_path_tmp=$download_path_tmp"

		#下载成功标记
		echo "1">$rootPathTmp/"is_upload_success_txt_tmp.txt"
	fi
else
	echo "Upload svn failed: missing svn upload path"
fi