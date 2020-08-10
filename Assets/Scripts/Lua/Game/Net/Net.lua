Net = {}

if not Net.common then Net.common = {} end 

Net.common.test4 = {cmd = 1, name = "common.test4",}
Net[1] = Net.common.test4

Net.common.test3 = {cmd = 2, name = "common.test3",}
Net[2] = Net.common.test3

Net.common.client_info = {cmd = 3, name = "common.client_info",}
Net[3] = Net.common.client_info

Net.common.match_player = {cmd = 4, name = "common.match_player",}
Net[4] = Net.common.match_player
if not Net.login then Net.login = {} end 

Net.login.test2 = {cmd = 5, name = "login.test2",}
Net[5] = Net.login.test2

Net.login.test1 = {cmd = 6, name = "login.test1",}
Net[6] = Net.login.test1

Net.login.req_login = {cmd = 7, name = "login.req_login",}
Net[7] = Net.login.req_login

Net.login.rsp_login = {cmd = 8, name = "login.rsp_login",}
Net[8] = Net.login.rsp_login

Net.login.ntf_logout = {cmd = 9, name = "login.ntf_logout",}
Net[9] = Net.login.ntf_logout
if not Net.room then Net.room = {} end 

Net.room.req_join_match = {cmd = 10, name = "room.req_join_match",}
Net[10] = Net.room.req_join_match

Net.room.rsp_join_match = {cmd = 11, name = "room.rsp_join_match",}
Net[11] = Net.room.rsp_join_match

Net.room.ntf_join_match_mateinfo = {cmd = 12, name = "room.ntf_join_match_mateinfo",}
Net[12] = Net.room.ntf_join_match_mateinfo

Net.room.ntf_join_match_result = {cmd = 13, name = "room.ntf_join_match_result",}
Net[13] = Net.room.ntf_join_match_result

Net.room.req_cancel_join_match = {cmd = 14, name = "room.req_cancel_join_match",}
Net[14] = Net.room.req_cancel_join_match

Net.room.rsp_cancel_join_match = {cmd = 15, name = "room.rsp_cancel_join_match",}
Net[15] = Net.room.rsp_cancel_join_match
if not Net.user then Net.user = {} end 

Net.user.req_create_user = {cmd = 16, name = "user.req_create_user",}
Net[16] = Net.user.req_create_user

Net.user.rsp_create_user = {cmd = 17, name = "user.rsp_create_user",}
Net[17] = Net.user.rsp_create_user

Net.user.req_change_name = {cmd = 18, name = "user.req_change_name",}
Net[18] = Net.user.req_change_name

Net.user.rsp_change_name = {cmd = 19, name = "user.rsp_change_name",}
Net[19] = Net.user.rsp_change_name

