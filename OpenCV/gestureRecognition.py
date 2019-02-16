#!/usr/bin/env
import sys
import numpy as np
import cv2
import math
import time
import socket


UDP_IP = "127.0.0.1"
UDP_PORT = 5065
                                  
print "UDP target IP:", UDP_IP
print "UDP target port:", UDP_PORT

sock = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP

font = cv2.FONT_HERSHEY_SIMPLEX

def detect_faces(f_cascade, colored_img, scaleFactor = 1.1):
    img_copy = np.copy(colored_img)
    #convert the test image to gray image as opencv face detector expects gray images
    gray = cv2.cvtColor(img_copy, cv2.COLOR_BGR2GRAY)
    
    #let's detect multiscale (some images may be closer to camera than others) images
    faces = f_cascade.detectMultiScale(gray, scaleFactor=scaleFactor, minNeighbors=5);
    
    #go over list of faces and draw them as rectangles on original colored img
    for (x, y, w, h) in faces:
        cv2.rectangle(img_copy, (x, y), (x+w, y+h), (0, 255, 0), 2)
        
    return img_copy

def convertToRGB(img):
    return cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

def areaRatio(roi2):

   mask2 = cv2.dilate(roi2,kernel,iterations = 1)
   contours2,hierarchy2= cv2.findContours(mask2,cv2.RETR_TREE,cv2.CHAIN_APPROX_SIMPLE)

   if contours2:
     cnt2 = max(contours2, key = lambda x: cv2.contourArea(x))
     hull2 = cv2.convexHull(cnt2)
     areahull2 = cv2.contourArea(hull2)
     areacnt2 = cv2.contourArea(cnt2)
     arearatio2=((areahull2-areacnt2)/areacnt2)*100
     return arearatio2
   else:
      print("max() arg is an empty sequence")
      return 0

def generateGestureBoxes(x, y, area):
   vals = [0,0,0]
   vals[0] = math.sqrt(area) / 4

   # roi1 
   vals[1] = x - 6*vals[0]
   vals[2] = y
   i = 0
   while i < len(vals):
      if vals[i] < 0:
         vals[i] = 0
      i+=1

   return vals

roi1Val = 1
roi2Val = 1
roi3Val = 1
roi4Val = 1

threshold = 0
signal = "none"

#           roi2
#
#   roi1    roi3 
#
#           roi4 
cap = cv2.VideoCapture(0)
fgbg = cv2.createBackgroundSubtractorMOG2()

while(1):
   
   ret, frame = cap.read()
   frame=cv2.flip(frame,1)
   height, width, channels = frame.shape
   kernel = np.ones((3,3),np.uint8)
   
   # apply background subtraction to the frame
   fgmask = fgbg.apply(frame)


   # XML training files for Haar cascade are stored in `opencv/data/haarcascades/` folder. 
   # First we need to load the required XML classifier. Then load our input image in grayscale mode.
   #load cascade classifier training file for lbpcascade
   lbp_face_cascade = cv2.CascadeClassifier('data/lbpcascade_frontalface.xml')
   testlbp = frame

   #convert the test image to gray image as opencv face detector expects gray images
   gray_img = cv2.cvtColor(testlbp, cv2.COLOR_BGR2GRAY)
   faces = lbp_face_cascade.detectMultiScale(gray_img, scaleFactor=1.1, minNeighbors=5)
   # go over list of faces and draw them as rectangles on original colored img
   for (x, y, w, h) in faces:
        cv2.rectangle(testlbp, (x, y), (x+w, y+h), (0, 255, 0), 2)
   
   # draw expected boundary for head rectangle
   cv2.rectangle(frame,(1080/2,0),(1200, 720/2),(0,255,255),0)

   # only generate regions of interest for gesture recognition if there is one face in frame
   if len(faces) == 1:
      #ideal area is 120 x 120
      area = faces[0][2] * faces[0][3]
      x = faces[0][0]
      y = faces[0][1]

      #print("area: ", area)

      if area > 270*270:
        cv2.putText(frame,'move backwards',(0,150), font, 2, (0,0,255), 3, cv2.LINE_AA)

      #cv2.putText(frame, str(x),(100,100), font, 2, (0,0,255), 3, cv2.LINE_AA)
      #cv2.putText(frame, str(y),(300,100), font, 2, (0,0,255), 3, cv2.LINE_AA)
      

      if area < 270*270 and (x > 400 and y > 50 and y < 300):

        vals = generateGestureBoxes(x, y, area)
        gestureArea = int(vals[0])
        roi2x = int(vals[1])
        roi2y = int(vals[2])

        roi3x = roi2x
        roi3y = roi2y + gestureArea*6

        roi1y = roi3y
        roi1x = roi3x - gestureArea*6
        
        roi4x = roi2x
        roi4y = roi2y + gestureArea*12

        # need to make the top, left, bottom boxes wider to make room
        # for user error
        roi2x = roi2x - gestureArea
        roi1y = roi1y - gestureArea
        roi4x = roi4x - gestureArea

        #left
        roi1=fgmask[roi1y:(roi1y + gestureArea*3),roi1x:(roi1x + gestureArea)]
        #top
        roi2=fgmask[roi2y:(roi2y + gestureArea),roi2x:(roi2x + gestureArea*3)]
        #middle
        roi3=fgmask[roi3y:(roi3y + gestureArea),roi3x:(roi3x + gestureArea)]
        #bottom
        roi4=fgmask[roi4y:(roi4y + gestureArea),roi4x:(roi4x + gestureArea*3)]
      
        
        #roi1
        cv2.rectangle(frame,(roi1x,roi1y),(roi1x+gestureArea,roi1y+gestureArea*3),(0,255,255),0)
        #roi2
        cv2.rectangle(frame,(roi2x,roi2y),(roi2x+gestureArea*3,roi2y+gestureArea),(0,255,255),0)
        #roi3
        cv2.rectangle(frame,(roi3x,roi3y),(roi3x+gestureArea,roi3y+gestureArea),(0,255,255),0)
        #roi4
        cv2.rectangle(frame,(roi4x,roi4y),(roi4x+gestureArea*3,roi4y+gestureArea),(0,255,255),0)
        
        font = cv2.FONT_HERSHEY_SIMPLEX

        roi3Val = areaRatio(roi3)
        roi2Val = areaRatio(roi2)
        roi1Val = areaRatio(roi1)
        roi4Val = areaRatio(roi4)
       
        """
        # left side
        if roi1Val<threshold:
           cv2.putText(frame,'off',(0,150), font, 2, (0,0,255), 3, cv2.LINE_AA)  
        else:
           cv2.putText(frame,'on',(0,150), font, 2, (0,0,255), 3, cv2.LINE_AA)
      
        # top 
        if roi2Val<threshold:
           cv2.putText(frame,'off',(100,100), font, 2, (0,0,255), 3, cv2.LINE_AA)  
        else:
           cv2.putText(frame,'on',(100,100), font, 2, (0,0,255), 3, cv2.LINE_AA)
        #middle
      
        if roi3Val<threshold*2:
           cv2.putText(frame,'off',(100,150), font, 2, (0,0,255), 3, cv2.LINE_AA)  
        else:
           cv2.putText(frame,'on',(100,150), font, 2, (0,0,255), 3, cv2.LINE_AA)
        #bottom
        if roi4Val<threshold:
           cv2.putText(frame,'off',(100,200), font, 2, (0,0,255), 3, cv2.LINE_AA)  
        else:
           cv2.putText(frame,'on',(100,200), font, 2, (0,0,255), 3, cv2.LINE_AA)
        """
        
        cv2.putText(frame,str(int(roi1Val)),(0,150), font, 2, (0,0,255), 3, cv2.LINE_AA)  
        cv2.putText(frame,str(int(roi2Val)),(100,100), font, 2, (0,0,255), 3, cv2.LINE_AA)  
        cv2.putText(frame,str(int(roi3Val)),(100,150), font, 2, (0,0,255), 3, cv2.LINE_AA)  
        cv2.putText(frame,str(int(roi4Val)),(100,200), font, 2, (0,0,255), 3, cv2.LINE_AA)  
        
        # set arm pose decision
        left = True if(roi1Val>threshold) else False
        top = True if(roi2Val>threshold) else False
        middle = True if(roi3Val>threshold) else False
        bottom = True if(roi4Val>threshold) else False

        if(left):
           signal="left"
        elif(top):
           signal="right"
        elif(bottom):
           signal="stop"
        else:
           signal="none"
           
        cv2.putText(frame,signal,(300,300), font, 2, (0,0,255), 3, cv2.LINE_AA)
            
        """
        if(signal != "none"):
                sock.sendto(str(signal) , (UDP_IP, UDP_PORT))
                print signal
            else:
                if( firstNone ):
                    sock.sendto(str(signal) , (UDP_IP, UDP_PORT))
                    print signal
                    firstNone = 0
        """


   k = cv2.waitKey(5) & 0xFF
   if k == 27:
      break

   cv2.imshow('Test Image', testlbp)

cv2.destroyAllWindows()
cap.release()    
