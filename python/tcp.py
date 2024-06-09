import socket
import json
import torch
from torch.distributions import Categorical
import numpy as np

from minigane.ppo import PPO


# 클라이언트로부터 받은 데이터를 처리하고 응답을 반환하는 함수
class TCP():
    def __init__(self):
        super(TCP, self).__init__()
        self.model = PPO()
        self.state = np.array([])
        self.action = 0
        self.prob = None
        self.reward = 0

    def handle_request(self, data):
        # 처음은 그냥 가만히
        # JSON 데이터를 디코딩
        request = json.loads(data.decode('ascii'))

        state = np.array(request["state"])

        if self.state.shape != (0,):
            state_prime = state
            self.reward = request["reward"]
            end = request["end"]

            self.model.put_data((self.state, self.action, self.reward / 100.0, state_prime, self.prob[self.action].item(), end))

        self.prob = self.model.pi(torch.from_numpy(state).float())
        m = Categorical(self.prob)
        self.action = m.sample().item()

        self.state = state
        # a = 2
        # reward = request["reward"]
        # end = request["end"]
        #
        #
        #
        # model.put_data((s, a, r / 100.0, state_prime, prob[a].item(), done))


        return self.action.to_bytes(4, byteorder='little')

    # 서버를 시작하는 함수
    def start_server(self, epi):
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind(('localhost', 8080))
        # 클라이언트의 연결 요청을 대기
        server_socket.listen(1)

        # 클라이언트의 연결을 수락
        conn, addr = server_socket.accept()

        cnt = 0
        t_horizon = 20
        while True:
            cnt += 1
            data = conn.recv(1024)
            if not data:
                break
            # 요청을 처리하고 응답을 생성
            response = self.handle_request(data)
            # 클라이언트에 응답을 전송
            conn.sendall(response)
            if cnt % t_horizon == 0:
                self.model.train()

        self.model.train()
        if epi % 100 == 0:
            self.model.new_save_model(epi, self.reward)

        conn.close()
        # start_server()


if __name__ == "__main__":
    epi = 0 #
    tcp = TCP()

    while True:
        epi += 1
        tcp.start_server(epi)
        print(epi)
