import socket

UDP_IP = "127.0.0.1"
UDP_PORT = 65501
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
print("Listening...")
while True:
    data, addr = sock.recvfrom(1024)
    print("Received message: ", data)
