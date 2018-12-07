
import socket
import time
UDP_IP = "192.168.0.19"
UDP_PORT = 65501
MESSAGE = 'Bonjour, le monde!'

print( "UDP target IP:", UDP_IP)
print( "UDP target port:", UDP_PORT)
print( "message:", MESSAGE)
count = 0
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
sock.connect((UDP_IP, UDP_PORT))
print("Beginning to send")
while True:
	sock.send(MESSAGE.encode('utf-8'))
	print(count)
	count =  count + 1
	time.sleep(0.5)
