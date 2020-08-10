#!/bin/sh

# REBOT_TOKEN="a06a97dc8b91481893626435ba7c3a2b"
if [[ $REBOT_TOKEN != "" ]]; then

     #发送钉钉消息
     # $(curl "https://oapi.dingtalk.com/robot/send?access_token=$REBOT_TOKEN" \
     #           -H "Content-Type: application/json" \
     #           -d "{\"msgtype\": \"text\", 
     #                \"text\": {
     #                     \"content\": \".$1\"
     #                }
     #           }")

     #发送飞书消息
     result=$(curl "https://open.feishu.cn/open-apis/bot/hook/$REBOT_TOKEN" -X "POST" -H "Content-Type: application/json" -d "{\"title\":\"\", \"text\":\".$1\"}")
     echo "send result="$result
fi