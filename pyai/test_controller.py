import unittest
from unittest.mock import patch

from Controller.ordinary_chat import app


class TestController(unittest.TestCase):
    def test_post_ai_message(self):
        payload = {
            "小明": [
                {"role": "assistant", "content": ["你好", "我叫KeKe 我喜欢你"]},
                {"role": "user", "content": ["你好呀", "真的吗", "我同意"]},
            ]
        }

        with patch("controller.service.get_ai_reply", return_value="测试返回") as mock_get_ai_reply:
            response = app.test_client().post("/api/ai", json=payload)

        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.get_json(), "测试返回")
        mock_get_ai_reply.assert_called_once_with(payload["小明"])


if __name__ == "__main__":
    unittest.main()
