from flask import Flask

from Controller.ordinary_chat import ordinary_chat_bp
from Controller.pre_person import pre_person_bp


def create_app() -> Flask:
    app = Flask(__name__)
    app.register_blueprint(ordinary_chat_bp)
    app.register_blueprint(pre_person_bp)
    return app


app = create_app()


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=10086, debug=True)
