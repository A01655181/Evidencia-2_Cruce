from flask import Flask, render_template, request, jsonify
import json, logging, os, atexit
import simul

app = Flask(__name__, static_url_path='')
model = simul.Model(10, 14, 1, 1, 1, 0)

# On IBM Cloud Cloud Foundry, get the port number from the environment variable PORT
# When running this app on the local machine, default the port to 8000
port = int(os.getenv('PORT', 8000))

@app.route('/')
def root():
    response = model.step()
    return jsonify(response)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=port, debug=True, )