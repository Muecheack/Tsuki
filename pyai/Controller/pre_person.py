from typing import Any

from flask import Blueprint, Response, jsonify, request
from Shell import file_IO
import ai_service
import Core.parse as parse
from Shell.file_IO import to_promp_file_normal, to_promp_file_pre

pre_person_bp = Blueprint("pre_person", __name__)

"""
发来:
{
    "sessionId": 123,
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
    data: dict[str, Any] = request.get_json() or {}
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
    user_name, ai_name, session, tags, avatars, gender, call_name = info_from_pre
    # 生成预人设の提示词
    to_promp_file_pre(tags, avatars, ai_name, user_name, gender, call_name)
    prmp: str = file_IO.load_prompt_from_file("prmp/prmp_pre.txt")
    pre_history: list[dict[str, Any]] = [{"role": "user", "content": "打个招呼吧~"}]
    pre_all_history: list[dict[str, Any]] = parse.insert_prmp_to_session(
        pre_history, prmp
    )

    print(f"完整历史消息, 预人设:{pre_all_history}")

    ai_reply: str = ai_service.get_ai_reply(pre_all_history)
    ai_reply_dict: dict[str, Any] = parse.parse_reply_to_dict(ai_reply)
    reply: str
    tags_list: list[str]
    avatar_id: int
    reply, tags_list, avatar_id = parse.get_reply_selected(ai_reply_dict)
    description: str = parse.get_description_by_id(avatar_id, avatars)
    to_promp_file_normal(ai_name, user_name, gender, call_name, description, tags_list)
    return jsonify(
        {"reply": reply, "selected": {"tags": tags_list, "avatarId": avatar_id}}
    )
