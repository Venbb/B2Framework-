# -*- coding:utf-8 -*-

import os
import sys
import xlrd
import util

# 使可以解析中文字符
reload(sys)
sys.setdefaultencoding('utf8')

# xlsx源文件路径
XLSX_PATH = util.XLSX_PATH

# lua文件输出路径
LUA_CFG_PATH = util.LUA_CFG_PATH

# 外部指定xlsx和lua的目录
paramsLen = len(sys.argv)
if paramsLen > 2:
    XLSX_PATH = sys.argv[1]
    LUA_CFG_PATH = sys.argv[2]

# lua文件输出路径不存在则创建
util.exist_dir(LUA_CFG_PATH)

# load excel data
def open_excel(file):
    try:
        data = xlrd.open_workbook(file, encoding_override='utf-8')
        return data
    except Exception as e:
        print("python Error:" + str(e))


# 转换成整型
def toint(val):
    if isinstance(val, unicode):
        v = int(filter(str.isdigit, val.encode("utf-8")))
    elif isinstance(val, str):
        v = int(val)
    else:
        v = int(val)
    return v


def parseval(vt, cell):
    if vt == 'string':
        if cell.ctype == 0:
            v = '\'\''
        else:
            v = '\'%s\'' % (str(cell.value))
    elif vt == 'int':
        if cell.ctype == 0:
            v = 0
        else:
            v = toint(cell.value)
    elif vt == 'float':
        if cell.ctype == 0:
            v = 0
        else:
            v = toint(cell.value)
    elif vt == 'bool':
        if cell.ctype == 0:
            v = 'false'
        else:
            v = cell.value
    elif vt == 'table':
        if cell.ctype == 0:
            v = '{}'
        else:
            v = cell.value
    else:
        # 未指定类型，默认当字符串处理
        if cell.ctype == 0:
            v = '\'\''
        else:
            v = '\'%s\'' % (str(cell.value))
        # v = cell.value
    return v


def sheet2lua(xlsx_name, xlsx_sheet):

    print('[excel] parse sheet: %s (%d row, %d col)' %
          (xlsx_sheet.name, xlsx_sheet.nrows, xlsx_sheet.ncols))

    # 这个存放的是列名称
    col_name_list = []

    # 这个存放字段值类型
    col_val_type_list = []

    # 这个存放所有数据的字典
    xlsx_data_dict = {}

    # ctype: 0 empty, 1 string, 2 number, 3 date, 4 boolean, 5 error

    # 第一行是所有列的描述

    # 遍历第二行的所有列 保存字段名
    for col in range(0, xlsx_sheet.ncols):
        cell = xlsx_sheet.cell(1, col)
        col_name_list.append(str(cell.value))
        if (cell.ctype != 1):
            print("found a invalid col name in col [%d] !~" % (col))

    # 遍历第三行的所有列 保存数据类型
    for col in range(0, xlsx_sheet.ncols):
        cell = xlsx_sheet.cell(2, col)
        col_val_type_list.append(str(cell.value))
        if (cell.ctype != 1):
            print("found a invalid col value type in col [%d] !~" % (col))

    # 从第四行开始遍历 构造行数据
    for row in range(3, xlsx_sheet.nrows):
        # 读取第一列为key
        cell_key = xlsx_sheet.cell(row, 0)

        vt = col_val_type_list[0]
        kv = parseval(vt, cell_key)

        # 检查key的唯一性
        if kv in xlsx_data_dict:
            print('[warning] duplicated data id: "%s", all previous value will be ignored!~' % (kv))

        # row data list
        row_data_list = []

        # 保存每一行的所有数据
        for col in range(0, xlsx_sheet.ncols):
            cell = xlsx_sheet.cell(row, col)
            k = col_name_list[col]
            vt = col_val_type_list[col]

            # ignored the string that start with '_'
            if str(k).startswith('_'):
                continue

            # 根据字段类型去调整数值 如果为空值 依据字段类型 填上默认值
            v = parseval(vt, cell)

            # 加入列表
            row_data_list.append([k, v])

        # 保存key 和 row data
        xlsx_data_dict[kv] = row_data_list

        # 生成Lua配置表
        lua_table_name = xlsx_sheet.name
        if lua_table_name != xlsx_name:
            lua_table_name = xlsx_name + '_' + lua_table_name
        lua_path = LUA_CFG_PATH + '/' + lua_table_name + '.lua'

        # export to lua file
        lua_export_file = open(lua_path, 'w')
        lua_export_file.write('local %s = {\n' % xlsx_sheet.name)

        # 遍历excel数据字典 按格式写入
        for k, v in xlsx_data_dict.items():
            lua_export_file.write('    [%s] = {\n' % k)
            for row_data in v:
                lua_export_file.write(
                    '        {0} = {1},\n'.format(row_data[0], row_data[1]))
            lua_export_file.write('    },\n')

        lua_export_file.write('}\n')
        lua_export_file.write('return %s' % xlsx_sheet.name)

        lua_export_file.close()

        print('[excel] %d row data exported !~ %s' %
              (xlsx_sheet.nrows, os.path.basename(lua_path)))


gameCfg = "GameCfg"
# 将指定Excel文件转换成lua表


def excel2lua(xlsx_path):
    # Excel 文件名
    xlsx_name = os.path.splitext(os.path.basename(xlsx_path))[0]

    # load excel data
    xlsx_data = open_excel(xlsx_path)
    # get first Sheet
    # xlsx_sheet = xlsx_data.sheet_by_index(0)
    # sheet2lua(xlsx_name,xlsx_sheet)

    # 获取所有sheet
    nsheets = xlsx_data.nsheets
    for idx in range(nsheets):
        sheet = xlsx_data.sheets()[idx]
        if sheet.nrows > 0 or sheet.ncols > 0:
            sheet2lua(xlsx_name, sheet)
            requre = 'require \'Game.Data.Config.%s_%s\'' % (
                xlsx_name, sheet.name)
            requrefile.write(gameCfg + "." + xlsx_name +
                             "." + sheet.name + " = %s\n" % requre)


# 删除原文件
util.del_file(LUA_CFG_PATH)

# 读取文件夹里所有的Excel文件，转换成lua表，映射关系到GameCfg全局表
requrefile = open(LUA_CFG_PATH + '/' + gameCfg + '.lua', "w")
requrefile.write(gameCfg + " = {}\n")

files = os.listdir(XLSX_PATH)
for f in files:
    name, ext = os.path.splitext(f)
    if (ext == '.xls' or ext == '.xlsx'):
        if name.find("~") >= 0:
            print("ignore file ", f)
            continue
        requrefile.write('\n%s.%s = {}\n' % (gameCfg, name))
        print("export  " + f)
        excel2lua(XLSX_PATH + "/" + f)

requrefile.close()

print("xlsx2lua Done")