import json
import logging

import ai_service
from Core import parse
from flask import Blueprint, Response, jsonify, request
from Shell import file_IO
from Shell.file_IO import to_promp_file_normal

# 兜底日志配置: 无 handler 时才有生效, 不会覆盖 Flask/宿主的日志配置
if not logging.getLogger().handlers:
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    )

logger = logging.getLogger(__name__)

ordinary_chat_bp = Blueprint("ordinary_chat", __name__)
"""
常规聊天接收:
{
    'apiKey': xxx,
    'sessionId': 4,
    'persona': {
        aiName: xxx,
        description: xxx,
        userCallName: xxx,
        userGender: xxx,
        tags: [string]
    }
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
    logger.info("收到常规聊天请求: %s %s", request.method, request.path)
    data = request.get_json()
    info_from_chat = parse.get_info_from_chat(data)
    (
        api_key,
        sessionId,
        name,
        session_list,
        ai_name,
        avatar_description,
        user_call_name,
        user_gender,
        ai_tags,
    ) = info_from_chat
    logger.info(
        "解析请求完成: sessionId=%s, name=%s, 历史消息条数=%s",
        sessionId,
        name,
        len(session_list or []),
    )
    logger.debug(
        "收到的完整json:\n%s",
        json.dumps(data, ensure_ascii=False, indent=4, default=str),
    )

    to_promp_file_normal(
        ai_name, name, user_gender, user_call_name, avatar_description, ai_tags
    )

    # 接收到的sessionId: sessionId, name: messages, session_list: {'测试用户1': [{'role': 'user', 'content': '你好'}]}
    prmp_path = "prmp/prmp_chat.txt"
    prmp: str = file_IO.load_prompt_from_file(prmp_path)
    logger.info("已加载人设提示词: path=%s, 字符数=%s", prmp_path, len(prmp))
    history = parse.insert_prmp_to_session(session_list, prmp)
    logger.info("已拼接提示词与历史, 待发送消息条数=%s", len(history))
    # 未来把类型换成list
    response: str = ai_service.get_ai_reply(history, api_key)
    logger.info("AI 回复成功: sessionId=%s, 字符数=%s", sessionId, len(response or ""))
    logger.debug("AI 回复内容: %s", response)

    # TODO: 未来把回复列表返回
    logger.info("返回常规聊天响应: sessionId=%s, aiName=%s", sessionId, name)
    return jsonify(
        {
            "aiName": ai_name,
            "sessionId": sessionId,
            "message": response,
            "type": "text",
        }
    )
