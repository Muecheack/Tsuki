from pprint import pprint

from flask import Blueprint, Response, request, jsonify
from Core import parse
from Shell import file_IO
import ai_service

ordinary_chat_bp = Blueprint("ordinary_chat", __name__)
"""
常规聊天接收:
{
    'sessionId': 4,
    'messages': {
            '测试用户1': [
                {'role': 'user', 'content': '你好'}
            ]
    }
}

回:
{
	aiName: xxx, 
	sessionId(可选): 000,
	message: xxx,
	type: xxx
}
"""


@ordinary_chat_bp.route("/api/ai", methods=["POST"])
def ordinary_chat() -> Response:
    data = request.get_json()
    info_from_chat = parse.get_info_from_chat(data)
    sessionId, name, session_list = info_from_chat

    print("收到的完整json:")
    pprint(data, indent=4, sort_dicts=False)

    # 接收到的sessionId: sessionId, name: messages, session_list: {'测试用户1': [{'role': 'user', 'content': '你好'}]}
    prmp: str = file_IO.load_prompt_from_file("prmp/prmp_chat.txt")
    history = parse.insert_prmp_to_session(session_list, prmp)
    # 未来把类型换成list
    response: str = ai_service.get_ai_reply(history)

    print(f"完整历史消息, 正常聊天:{history}")

    print(f"回复:{response}")

    # TODO: 未来把回复列表返回
    return jsonify(
        {
            "aiName": name,
            "sessionId": sessionId,
            "message": response,
            "type": "text",
        }
    )
