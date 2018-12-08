# Using Hexiwear with Python
import pexpect
import time
import socket

DEVICE = "00:2E:40:08:00:31"
UDP_IP = "192.168.0.36"
UDP_PORT = 12345

print("Hexiwear address:"),
print(DEVICE)

# Run gatttool interactively.
print("Run gatttool...")
child = pexpect.spawn("gatttool -I")


# Bluetooth connect to the Hexiwear device.
print("Bluetooth connecting to "),
print(DEVICE)
con = "connect 00:2E:40:08:00:31"
child.sendline(con)
child.expect("Connection successful", timeout=5)
print("Bluetooth connected!")

# UDP connect to the laptop
print( "UDP target IP:", UDP_IP)
print( "UDP target port:", UDP_PORT)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
error = sock.connect_ex((UDP_IP, UDP_PORT))
print("Error: ", error) 
# function to transform hex string like "0a cd" into signed integer
def hexStrToInt(hexstr):
    lowval = int(hexstr[0:2],16)*256
    highval = int(hexstr[3:5],16)
    val = lowval + highval
    if ((val & 0x8000) == 0x8000):  # treat signed 16bits
        val = -((val ^ 0xffff)+1)
    return val

def computeDirection(x, y):
	if y<-4:
		return 'R'
	elif y>4:
		return 'L'
	elif x<-3:
		return 'F'
	elif x>3:
		return 'B'
	else:
		return 'N'

while True:
	#Accelerometer
	child.sendline("char-read-uuid 0x2001")
	child.expect("handle: ", timeout=10)
	child.expect("\r\n", timeout=10)
	input = str(child.before)
	print(input)
	index  = input.find(':')
	trimmedString = input[index+2:]
	print(trimmedString)
	x = float(hexStrToInt(trimmedString[0:5]))/1000
	y = float(hexStrToInt(trimmedString[6:11]))/1000
	z = float(hexStrToInt(trimmedString[12:17]))/1000
	message = "Accel: %.3f, %.3f, %.3f" % (x,y,z)
	dir = computeDirection(x,y)
	sock.send(dir.encode('utf-8'))
	print(message)
	print("Direction: %s" % dir)
