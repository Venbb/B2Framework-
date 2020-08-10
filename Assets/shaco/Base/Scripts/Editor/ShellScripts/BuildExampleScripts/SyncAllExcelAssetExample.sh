# !/bin/sh

#同步项目中设置过的excel导出csv目录下所有csv文件为asset序列化文件
#建议在真机都使用asset文件，这会比读取csv更加高效和节省内存

rootPathTmp=$(cd `dirname $0`;pwd)

UNITY_PATH="/Applications/Unity2018.4.5f1/Unity2018.4.5f1.app/Contents/MacOS/Unity"
export UNITY_PATH

REBOT_TOKEN="1b80e0a0314947cca483bf6d9d30f4ca"
export REBOT_TOKEN

bash "${rootPathTmp}/../SyncAllExcelAsset.sh"