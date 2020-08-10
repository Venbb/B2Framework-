# coding: utf-8

import sys
import os
import platform
import commands
import util

# proto文件默认路径
proto_path = util.PROTO_PATH

# pb文件输出默认路径
pb_path = util.PB_PATH

# 外部指定proto和pb的目录
paramsLen = len(sys.argv)
if paramsLen > 2:
    proto_path = sys.argv[1]
    pb_path = sys.argv[2]

# pb目录不存在则创建
util.exist_dir(pb_path)

# protoc 的路径
protoc_path = util.PROTOC_PATH

# Lua协议文件路径
LUA_PROTOCOL_PATH = util.LUA_PROTOCOL_PATH + "/Net.lua"

msgCount = 1
notifyCount = 100001
enumStr = "\n"
def genMsgCode(filename, msgFileHandler, subfilename):
	f = open(filename, "r")
	global msgCount
	global notifyCount
	global enumStr
	line = f.readline()
	packageName = ""
	try:
		while line:
			if line.find("enum") == 0:
				enumName = line.split()[1]
				enumStr += '\nNet.'+packageName+'.'+enumName+' = {}\n'
				line = f.readline()
				while line:
					enuml = line.split("=")
					if len(enuml) >= 2 and not enuml[0].strip().startswith("//"):
						# enumStr += 'Net.'+packageName+'.'+enumName+'.'+enuml[0].strip()+' = "'+enuml[0].strip()+'"\n'
						enumStr += 'Net.'+packageName+'.'+enumName+'.'+enuml[0].strip()+' = '+enuml[1].strip().replace("//", "--")+'\n'
					if line.find("}") == 0:
						break
					line = f.readline()
			
			if line.find("package") == 0:
				packageName = line.split()[1].strip().replace(';', '') 
				msgFileHandler.write('if not Net.'+packageName + ' then Net.'+packageName + ' = {} end \n')

			if line.find("message") == 0:
				msgName = line.split()[1]
				index = msgCount
				if msgName.endswith('_notify'):
					index = notifyCount
					notifyCount += 1
				else:
					index = msgCount
					msgCount += 1

				msgFileHandler.write('\nNet.'+packageName+'.'+msgName+' = {')
				msgFileHandler.write('cmd = ' + str(index) + ',')
				msgFileHandler.write(' name = "'+packageName+'.'+msgName+'",')
				msgFileHandler.write('}\n')
				msgFileHandler.write('Net['+str(index)+'] = Net.' + packageName + '.' + msgName +'\n' )

			line = f.readline()
		f.close() 
	except Exception as e:
		print("python Error:"+str(e) + ", line : " +line)

# 删除原文件
util.del_file(pb_path)

# 把协议写到lua文件
luaProtocolFile = open(LUA_PROTOCOL_PATH, "w")
luaProtocolFile.write('Net = {}\n\n')

# 获取所有的proto文件
files = os.listdir(proto_path)

# 将所有的proto文件转换成pb文件
# 因为os.system(cmd)无法捕获异常，这里使用了commands.getstatusoutput(cmd)
# 但是commands.getstatusoutput(cmd)在window环境用有问题，所以在Unity Editor 没办法显示错误日志了
for f in files:
    name, ext = os.path.splitext(f)
    if ext == '.proto':
        # cmd = protoc_path + ' -I ' + proto_path + ' --descriptor_set_out ' + \
        #     pb_path + '/' + name + '.pb ' + proto_path + '/' + f
        cmd = protoc_path + ' -I ' + proto_path + ' -o ' + \
            pb_path + '/' + name + '.pb.bytes ' + proto_path + '/' + f
        os.system(cmd)
        # status, output = commands.getstatusoutput(cmd)
        # if status != 0:
        #     print("python Error:" + output)
        print("parse file " + f)
        genMsgCode(proto_path+'/'+f, luaProtocolFile, name)

luaProtocolFile.write(enumStr)
luaProtocolFile.close()

print("proto2pb Done")