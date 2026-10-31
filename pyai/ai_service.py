def get_ai_reply(session_list: list, api_key: str) -> str:
    from openai import OpenAI

    client = OpenAI(
        api_key=api_key,
        base_url="https://api.deepseek.com",
    )

    response = client.chat.completions.create(
        model="deepseek-v4-pro",
        messages=[
            # TODO: 未来在提示词文件里加上把逗号换成分割符号,使ai按照分隔符回复
            # TODO: 未来新增提示词文件用于自动化点赞(格式化输出模板)
            {"role": "system", "content": "You are a helpful assistant"},
            *session_list,
        ],
        stream=False,
        reasoning_effort="high",
        extra_body={"thinking": {"type": "enabled"}},
    )

    # TODO: 未来把回复的分隔符处理掉,拆成列表返回
    return response.choices[0].message.content
