# -*- coding:utf-8 -*-

import os
import sys
import xlrd
import util

# 使可以解析中文字符
reload(sys)
sys.setdefaultencoding('utf8')

# xlsx源文件路径
XLSX_PATH = util.LOC_XLSX_PATH

# lua文件输出路径
LOC_CFG_PATH = util.LOC_CFG_PATH

# 指定的语言，为空则会生成所有多语言
LAN = ''

# 外部指定xlsx和lua的目录
paramsLen = len(sys.argv)
if paramsLen > 3:
    XLSX_PATH = sys.argv[1]
    LOC_CFG_PATH = sys.argv[2] 
    LAN = sys.argv[3]
    if LAN == 'all':
        LAN = ''   

# lua文件输出路径不存在则创建
util.exist_dir(LOC_CFG_PATH)

# load excel data
def open_excel(file):
    try:
        data = xlrd.open_workbook(file, encoding_override='utf-8')
        return data
    except Exception as e:
        print("python Error:" + str(e))


def sheet2json(xlsx_sheet):
    print('[excel] parse sheet: %s (%d row, %d col)' %
          (xlsx_sheet.name, xlsx_sheet.nrows, xlsx_sheet.ncols))

    # ctype: 0 empty, 1 string, 2 number, 3 date, 4 boolean, 5 error

    xlsx_data_dict = {}

    # for col in range(0, xlsx_sheet.ncols):
    #     cell = xlsx_sheet.cell(0, col)
    #     fileName = 'lc_%s_%s.json' % (str(cell.value), xlsx_sheet.name)
    #     xlsx_data_list.append([fileName, []])
    #     if (cell.ctype != 1):
    #         print("found a invalid col name in col [%d] !~" % (col))

    for row in range(1, xlsx_sheet.nrows):
        key_cell = xlsx_sheet.cell(row, 0)
        if key_cell.ctype != 1:
            continue
        for col in range(1, xlsx_sheet.ncols):
            val_cell = xlsx_sheet.cell(row, col)
            # 值为空则不生成
            if val_cell.ctype == 0:
                continue

            top_cell = xlsx_sheet.cell(0, col)
            lan = str(top_cell.value)
            # 生成指定语言
            if len(LAN) > 0:
                lans = LAN.split('|')
                if lan not in lans:
                    continue
            js = '\"{0}\": \"{1}\"'.format(
                str(key_cell.value), str(val_cell.value))

            # 使用要生成的文件名称为key
            fileName = 'lc_%s_%s.json' % (str(top_cell.value), xlsx_sheet.name)
            if fileName in xlsx_data_dict:
                data_list = xlsx_data_dict[fileName]
            else:
                data_list = xlsx_data_dict[fileName] = []
            data_list.append(js)

    # 遍历excel数据字典 按格式写入
    for k, v in xlsx_data_dict.items():
        lua_export_file = open(LOC_CFG_PATH + '/' + k, 'w')
        lua_export_file.write('{\n')
        m = len(v)
        for i in range(0, m):
            if(i < m - 1):
                lua_export_file.write(' %s,\n' % v[i])
            else:
                lua_export_file.write(' %s\n' % v[i])

        lua_export_file.write('}\n')

        lua_export_file.close()
        print("parse  " + k)


# 将指定Excel文件转换成json文本
def excel2json(xlsx_path):
    # load excel data
    xlsx_data = open_excel(xlsx_path)

    # 获取所有sheet
    nsheets = xlsx_data.nsheets
    for idx in range(nsheets):
        sheet = xlsx_data.sheets()[idx]
        if sheet.nrows > 0 or sheet.ncols > 0:
            sheet2json(sheet)


# 删除原文件
util.del_file(LOC_CFG_PATH)

files = os.listdir(XLSX_PATH)
for f in files:
    name, ext = os.path.splitext(f)
    if (ext == '.xls' or ext == '.xlsx'):
        if name.find("~") >= 0:
            print("ignore file ", f)
            continue
        print("export  " + f)
        excel2json(XLSX_PATH + "/" + f)

print("localization Done")
