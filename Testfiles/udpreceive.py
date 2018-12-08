import socket

UDP_IP = ""
UDP_PORT = 65300
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
print("Listening...")
while True:
    data, addr = sock.recvfrom(1024)
    print("Received message: ", data)
