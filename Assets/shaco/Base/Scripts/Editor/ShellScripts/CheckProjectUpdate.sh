# !/bin/sh

rootPathTmp=$(cd `dirname $0`;pwd)
shaco_path_flag="GitHub/shaco"

if [[ $rootPathTmp =~ $shaco_path_flag ]]; then
	projectPath=${rootPathTmp%/shaco/Base/*}/shaco/Base
else
	projectPath=${rootPathTmp%/Assets/*}
fi

#进入工程目录
cd $projectPath

echo "rootPathTmp="$rootPathTmp
echo "projectPath="$projectPath
echo "param1="$1" param2="$s2

#回滚配置
if [[ "$2" = "DiscardAllLocalChanges" ]]; then

	echo "project will discard all changes"

	if [[ $1 = "Svn" ]]; then
		svn revert . -R
	else
		git checkout .
		git clean -df
		# git stash save --include-untracked
		# git stash drop
	fi

	echo "project discard all changes end"
fi

#更新工程
echo "project will update"

if [[ $1 = "Svn" ]]; then
	svn update --username $SVN_USER_NAME --password $SVN_PASSWORD --no-auth-cache
else
	git pull --no-edit
fi
#检查git是否pull成功
if [[ $1 != "Svn" ]]; then

	result_tmp=$(git status)

	#git还提示需要使用git pull去更新项目说明之前的git pull失败了
	if [[ $result_tmp =~ 'use "git pull" to update your local branch' ]]; then
		error_log_tmp="<<<<<<[Project Update Error]>>>>>>"
		error_log_tmp=$error_log_tmp"\ngit pull has error, maybe use other system to execute shell, like 'MacOS crontab'"
		error_log_tmp=$error_log_tmp"\nTodo fix that, you can modify some config, path to $projectPath/.git/config"
		error_log_tmp=$error_log_tmp"\nJust like:\n"
		error_log_tmp=$error_log_tmp"\n[remote "origin"]\n\thttp://127.0.0.1/file.git"
		error_log_tmp=$error_log_tmp"\n\n↓↓↓↓↓↓ Modified to ↓↓↓↓↓↓\n"
		error_log_tmp=$error_log_tmp"\n[remote "origin"]\n\thttp://{username}:{password}@127.0.0.1/file.git"
		echo -e $error_log_tmp

		#发送错误日志
		bash "$rootPathTmp/SendMessage.sh" "$error_log_tmp"
		exit
	fi
fi

#检查工程是否有修改内容
if [[ $1 = "Svn" ]]; then
	diff_tmp=$(svn status)
else
	diff_tmp=$(git diff)
fi

echo $diff_tmp>$rootPathTmp"/diff.tmp"
echo "project update end"

#更新项目成功，记录更新成功标记
echo "1" > $rootPathTmp"/is_update_project_success_tmp.txt"