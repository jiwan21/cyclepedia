
import socket
import time
<<<<<<< Updated upstream
UDP_IP = "192.168.0.36"
UDP_PORT = 65300
=======
UDP_IP = "192.168.43.219"
UDP_PORT = 65400
>>>>>>> Stashed changes
MESSAGE = 'Bonjour, le monde!'

print( "UDP target IP:", UDP_IP)
print( "UDP target port:", UDP_PORT)
print( "message:", MESSAGE)
count = 0
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
error = sock.connect_ex((UDP_IP, UDP_PORT))
<<<<<<< Updated upstream
print "Error: ", error
=======
print "Error: ", error 
>>>>>>> Stashed changes
print("Beginning to send")
while True:
	sock.send(MESSAGE.encode('utf-8'))
	print(count)
	count =  count + 1
	time.sleep(0.5)
