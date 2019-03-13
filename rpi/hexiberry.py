#!/usr/bin/python3
# Using Hexiwear with Python
import pexpect
import time
import socket
import string
import sys
import time
import math
import IMU
import datetime
import os

#------------------------------------------IMU------------------------------------------
#---------------------------------------------------------------------------------------

# If the IMU is upside down (Skull logo facing up), change this value to 1
IMU_UPSIDE_DOWN = 1

ACC_LPF_FACTOR = 0.4 	# Low pass filter constant for accelerometer
ACC_MEDIANTABLESIZE = 9    	# Median filter table size for accelerometer. Higher = smoother but a longer delay

oldXAccRawValue = 0
oldYAccRawValue = 0
oldZAccRawValue = 0

a = datetime.datetime.now()

#Setup the tables for the mdeian filter. Fill them all with '1' soe we dont get devide by zero error 
acc_medianTable1X = [1] * ACC_MEDIANTABLESIZE
acc_medianTable1Y = [1] * ACC_MEDIANTABLESIZE
acc_medianTable1Z = [1] * ACC_MEDIANTABLESIZE
acc_medianTable2X = [1] * ACC_MEDIANTABLESIZE
acc_medianTable2Y = [1] * ACC_MEDIANTABLESIZE
acc_medianTable2Z = [1] * ACC_MEDIANTABLESIZE


IMU.detectIMU()     #Detect if BerryIMUv1 or BerryIMUv2 is connected.
IMU.initIMU()       #Initialise the accelerometer, gyroscope and compass

def readBerry():

    #Read the accelerometer,gyroscope and magnetometer values
    ACCx = IMU.readACCx()
    ACCy = IMU.readACCy()
    ACCz = IMU.readACCz()

    #Normalize accelerometer raw values.
    if not IMU_UPSIDE_DOWN:
        #Use these two lines when the IMU is up the right way. Skull logo is facing down
        accXnorm = ACCx/math.sqrt(ACCx * ACCx + ACCy * ACCy + ACCz * ACCz)
        accYnorm = ACCy/math.sqrt(ACCx * ACCx + ACCy * ACCy + ACCz * ACCz)
    else:
        #Us these four lines when the IMU is upside down. Skull logo is facing up
        accXnorm = -ACCx/math.sqrt(ACCx * ACCx + ACCy * ACCy + ACCz * ACCz)
        accYnorm = ACCy/math.sqrt(ACCx * ACCx + ACCy * ACCy + ACCz * ACCz)

    #Calculate pitch and roll

    pitch = math.asin(accXnorm)
    roll = -math.asin(accYnorm/math.cos(pitch))
    return roll

#----------------------------------------Hexiwear---------------------------------------
#---------------------------------------------------------------------------------------

DEVICE = "00:2E:40:08:00:31"
UDP_IP = "192.168.43.142"
UDP_PORT = 12346

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

#------------------------------------------UDP------------------------------------------
#---------------------------------------------------------------------------------------

# UDP connect to the laptop
print( "UDP target IP:", UDP_IP)
print( "UDP target port:", UDP_PORT)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
error = sock.connect_ex((UDP_IP, UDP_PORT))
print("Error: ", error) 

#---------------------------------------------------------------------------------------
#---------------------------------------------------------------------------------------

# function to transform hex string like "0a cd" into signed integer
def hexStrToInt(hexstr):
    lowval = int(hexstr[0:2],16)*256
    highval = int(hexstr[3:5],16)
    val = lowval + highval
    if ((val & 0x8000) == 0x8000):  # treat signed 16bits
        val = -((val ^ 0xffff)+1)
    return val

def getAngle():
	#Accelerometer
	child.sendline("char-read-uuid 0x2001")
	child.expect("handle: ", timeout=10)
	child.expect("\r\n", timeout=10)
	input = str(child.before)
	#print(input)
	index  = input.find(':')
	trimmedString = input[index+2:]
	#print(trimmedString)
	# scales back to m/s^2
	x = float(hexStrToInt(trimmedString[0:5]))*0.00098
	y = float(hexStrToInt(trimmedString[6:11]))*0.00098
	z = float(hexStrToInt(trimmedString[12:17]))*0.00098
	#print("Accel: %.3f, %.3f, %.3f" % (x,y,z))
	if x == 0:
		x = 0.000001
	angle = math.atan(y/x)/6.28318*360 + 90
	if x<0:
		angle = angle + 180
	return angle

def getGyro():
	#Gyroscope
	child.sendline("char-read-uuid 0x2002")
	child.expect("handle: ", timeout=10)
	child.expect("\r\n", timeout=10)
	input = str(child.before)
	#print(input)
	index  = input.find(':')
	trimmedString = input[index+2:]
	#print(trimmedString)
	x = int(hexStrToInt(trimmedString[0:5]))
	y = int(hexStrToInt(trimmedString[6:11]))
	z = int(hexStrToInt(trimmedString[12:17]))
	#print("Gyro: %d" % z)
	return z

prev_time = time.time()
curr_time = 0
speed = 0
prevAngle = 0
while True:
	angularZ = getGyro()
	if angularZ > 25:
		#angle = getAngle()
		#delta = (angle - prevAngle)%360
		#curr_time = time.time()
		#speed = delta/360/(curr_time - prev_time)
		#prev_time = curr_time
		#prevAngle = angle
		speed = angularZ
	else:
		speed = 0
	#print("Angle: %.3f Next Angle: %.3f" % (angle,next_angle))
	print(readBerry())
	speed = int(speed)
	#print("RPS: %.3f" % speed)
	#sock.send(str(speed).encode('utf-8'))
	time.sleep(0.3)

#print("RPS: %.3f" % speed)

