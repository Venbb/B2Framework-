# coding: utf-8

import os
import sys

# os.getcwd() 获取terminal命令所在路径

# 获取当前文件所在路径
cur_dir = os.path.split(os.path.realpath(__file__))[0]

# 这里定义了要执行的Python命令，执行路径上的Python脚本
# cmd = 'python ' + cur_dir + '/proto2pb.py'
# 调用系统命令行，执行ptyhon命令
# os.system(cmd)

print("export pb-------------------------------")
cmd = 'python ' + cur_dir + '/proto2pb.py'
os.system(cmd)

print("export config-------------------------------")
cmd = 'python ' + cur_dir + '/xlsx2lua.py'
os.system(cmd)

print("export localization-------------------------------")
cmd = 'python ' + cur_dir + '/localization.py'
os.system(cmd)

print("build Done")