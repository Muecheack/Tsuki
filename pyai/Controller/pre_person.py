import json
import logging
from typing import Any

import ai_service
import Core.parse as parse
from flask import Blueprint, Response, jsonify, request
from Shell import file_IO
from Shell.file_IO import to_promp_file_normal, to_promp_file_pre

# 兜底日志配置: 无 handler 时才有生效, 不会覆盖 Flask/宿主的日志配置
if not logging.getLogger().handlers:
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    )

logger = logging.getLogger(__name__)

pre_person_bp = Blueprint("pre_person", __name__)

"""
发来:
{
    "apiKey"; xxx,
    "sessionDict": {
        "张三": {
            "aiName": "...",
            "session": [],
            "tags": [...],
            "avatars": [{id: 000, description: xxx}],
            "gender": "...",
            "callName": "..."
        }
    }
}

回:
{
    "reply": str,
    "selected":{tags:[str], avatarId: int}
}
"""


@pre_person_bp.route("/api/pre", methods=["POST"])
def pre_person() -> Response:
    logger.info("收到预人设请求: %s %s", request.method, request.path)
    data: dict[str, Any] = request.get_json() or {}
    logger.debug(
        "收到的完整json:\n%s",
        json.dumps(data, ensure_ascii=False, indent=4, default=str),
    )
    # 数据->拆->生成prmp->发ai->拿回复->解析成字典->拆回复->解析回复->组合新prmp->返回
    info_from_pre: tuple[
        int | None,
        str,
        str,
        list[dict[str, Any]],
        list[str],
        list[dict[str, Any]],
        str,
        str,
    ] = parse.get_info_pre(data)
    user_name, ai_name, session, tags, avatars, gender, call_name, api_key = (
        info_from_pre
    )
    logger.info(
        "解析请求完成: user_name=%s, ai_name=%s, 标签数=%s, 头像数=%s, gender=%s, callName=%s",
        user_name,
        ai_name,
        len(tags or []),
        len(avatars or []),
        gender,
        call_name,
    )
    # 生成预人设の提示词
    to_promp_file_pre(tags, avatars, ai_name, user_name, gender, call_name)
    logger.info("已生成预人设提示词文件: prmp/prmp_pre.txt")
    prmp: str = file_IO.load_prompt_from_file("prmp/prmp_pre.txt")
    logger.info("已加载预人设提示词: 字符数=%s", len(prmp))
    pre_history: list[dict[str, Any]] = [{"role": "user", "content": "打个招呼吧~"}]
    pre_all_history: list[dict[str, Any]] = parse.insert_prmp_to_session(
        pre_history, prmp
    )

    ai_reply: str = ai_service.get_ai_reply(pre_all_history, api_key)
    logger.info("AI 回复成功: 字符数=%s", len(ai_reply or ""))
    logger.debug("AI 回复内容: %s", ai_reply)
    ai_reply_dict: dict[str, Any] = parse.parse_reply_to_dict(ai_reply)
    if not ai_reply_dict:
        logger.warning("AI 回复解析为空, 将按默认值兜底: ai_reply=%r", ai_reply)
    reply: str
    tags_list: list[str]
    avatar_id: int
    reply, tags_list, avatar_id = parse.get_reply_selected(ai_reply_dict)
    description: str = parse.get_description_by_id(avatar_id, avatars)
    logger.info("解析回复完成: tags=%s, avatarId=%s", tags_list, avatar_id)
    if description is None:
        logger.warning("avatarId=%s 未匹配到任何头像描述, 头像描述将为空", avatar_id)
    to_promp_file_normal(ai_name, user_name, gender, call_name, description, tags_list)
    logger.info("已生成常规聊天提示词文件: prmp/prmp_chat.txt")
    logger.info("返回预人设响应: 回复字符数=%s", len(reply or ""))

    return jsonify(
        {"reply": reply, "selected": {"tags": tags_list, "avatarId": avatar_id}}
    )
