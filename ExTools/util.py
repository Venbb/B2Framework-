# coding: utf-8

import os
import platform

# 当前脚本所在目录
# ../../ExTools
CUR_DIR = os.path.split(os.path.realpath(__file__))[0]

# 工具所在目录
# ../../B2
TOOLS_DIR = os.path.abspath(os.path.join(CUR_DIR, ".."))

# AssetBundles 路径
BUNDLES_PATH = TOOLS_DIR + '/Assets/AssetBundles'

# ==================== protobuf =========================

# protobuf 目录
PROTOBUF_DIR = CUR_DIR + "/protobuf"

# proto文件默认路径
PROTO_PATH = PROTOBUF_DIR + '/proto'

# pb文件输出默认路径
PB_PATH = BUNDLES_PATH + '/Protos'

# Lua协议文件生成路径
LUA_PROTOCOL_PATH = TOOLS_DIR + '/Assets/Scripts/Lua/Game/Net'

# 根据当前系统选择protoc可执行文件
protoc = "protoc"
if platform.system() == "Windows":
    protoc = "protoc.exe"

# protoc路径
PROTOC_PATH = os.path.join(PROTOBUF_DIR, protoc)

# ==================== protobuf =========================


# ==================== config =========================

# Excel 配置表默认路径路径
XLSX_PATH = CUR_DIR + "/config"
# Lua 配置表的路径
LUA_CFG_PATH = TOOLS_DIR + '/Assets/Scripts/Lua/Game/Data/Config'

# ==================== config =========================

# ==================== Localization =========================

# Excel 配置表默认路径路径
LOC_XLSX_PATH = CUR_DIR + "/localization"

# Localization 配置路径
LOC_CFG_PATH = BUNDLES_PATH + '/Localization'

# ==================== Localization =========================

# 删除文件
def del_file(path_data):
    try:
        for i in os.listdir(path_data):# os.listdir(path_data)#返回一个列表，里面是当前目录下面的所有东西的相对路径
            file_data = path_data + "/" + i#当前文件夹的下面的所有东西的绝对路径
            if os.path.isfile(file_data) == True:#os.path.isfile判断是否为文件,如果是文件,就删除.如果是文件夹.递归给del_file.
                os.remove(file_data)
            else:
                del_file(file_data)
    except Exception as e:
        print(e)

# 目录不存在则创建
def exist_dir(dir):
	if not os.path.exists(dir):
		os.makedirs(dir)