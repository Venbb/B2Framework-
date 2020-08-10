#!/bin/sh

#获取打包根目录
packages_folder_path_tmp=${UPLOAD_PACKAGE_PATH%/*}

echo "<<<<<<will delete not current packages>>>>>>root path=$packages_folder_path_tmp"

#获取打包文件名字
file_name_current_tmp=${UPLOAD_PACKAGE_PATH##*/}

#如果是文件则去除后缀名
if [[ -f $UPLOAD_PACKAGE_PATH ]]; then
    file_name_current_tmp=${file_name_current_tmp%.*}
fi

#遍历打包目录获取需要删除的文件名列表
for element in $packages_folder_path_tmp/*; do
    file_name_old_tmp=${element##*/}

    #文件路径不能以*结尾，否则会删除整个目录内容
    if [[ $file_name_old_tmp != *$file_name_current_tmp* && $element != *"*" ]]; then
        echo "Foreach will delete path=$element"
        will_delete_packages_tmp[${#will_delete_packages_tmp[*]}]=$element
    fi
done

#遍历需要删除的文件列表，并删除它们
will_delete_packages_count_tmp=${#will_delete_packages_tmp[*]}

#只有当可以删除文件数量在1 ~ 9之间的时候才允许自动删除，否则可能导致根目录传错误删除了其他不该删除的文件
if [ $will_delete_packages_count_tmp -gt 0 ]; then
    if [ $will_delete_packages_count_tmp -lt 10 ]; then
        for ((i = 0; i < $will_delete_packages_count_tmp; ++i)); do
            package_path_tmp=${will_delete_packages_tmp[$i]}
            echo "delete path=$package_path_tmp"

            #带空格的路径shell无法直接识别，必须要加上双引号
            rm -rf "$package_path_tmp"
            echo "DeleteNotCurrentPackages delete path=$package_path_tmp"
        done
    else
        echo "For security reasons, automatic deletion is allowed only when the deleted file is less than 10"
    fi
else
    echo "nothing need delete"
fi

echo "\n<<<<<<delete not current packages end>>>>>>\n"
