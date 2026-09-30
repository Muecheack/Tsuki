import json

# {
#     "sessionId": 123,
#     "sessionDict": {
#         "张三": {
#             "aiName": "...",
#             "session": [],
#             "tags": [...],
#             "avatars": [{id: 000, description: xxx},]
#             "gender": "...",
#             "callName": "..."
#         }
#     }
# }

# 普通聊天
def get_info_from_chat(data: dict):
    sessionId: str  = data.get('sessionId')
    messages: dict = data.get('messages')
    name: str = list(messages.keys())[0]
    session_list: list = messages[name]
    return (sessionId, name, session_list)


# 预人设
def get_info_pre(prop_dict:dict) -> tuple:
    session_dict: dict = prop_dict.get("sessionDict") or {}
    user_name: str = list(session_dict.keys())[0]
    user_info: dict = session_dict[user_name]
    ai_name: str = user_info.get("aiName")
    session: list = user_info.get("session")
    tags: list = user_info.get("tags")
    avatars: list = user_info.get("avatars")
    gender: str = user_info.get("gender")
    callName: str = user_info.get("callName")
    return (user_name, ai_name, session, tags,avatars, gender, callName)


# 把prmp塞到历史列表的最前方
def insert_prmp_to_session(history: list, prmp: str) -> list:
    return [{"role": "system", "content": prmp}] + history


# 把回复解析成字典
def parse_reply_to_dict(reply: str) -> dict:
    try:
        return json.loads(reply)
    except Exception as e:
        print(f"解析回复为字典失败: {e}")
        return {}


# 提取reply和selected
def get_reply_selected(reply_dict: dict) -> tuple:
    reply: str = reply_dict.get("reply", "")
    selected: dict = reply_dict.get("selected") or {}
    tags: list = selected.get("tags") or []
    avatar_id: int = selected.get("avatarId") or 0
    return (reply, tags, avatar_id)


# 根据id拿description
def get_description_by_id(id: int, id_description: list) -> str:
    return next((i["description"] for i in id_description if i["id"] == id), None)