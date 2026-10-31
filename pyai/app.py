import os

from Controller.ordinary_chat import ordinary_chat_bp
from Controller.pre_person import pre_person_bp
from flask import Flask


def create_app() -> Flask:
    app = Flask(__name__)
    app.register_blueprint(ordinary_chat_bp)
    app.register_blueprint(pre_person_bp)
    return app


app = create_app()

# 端口：桌面壳用 PORT 环境变量传进来（它动态挑的空闲端口），没传就用开发默认 10086
port = int(os.environ.get("PORT", 10086))

if __name__ == "__main__":
    app.run(host="127.0.0.1", port=port, debug=False)
