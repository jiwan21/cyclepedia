#!/usr/bin/env
 
import sys
import cv2
import numpy as np
import math

import socket
import time

UDP_IP = "127.0.0.1"
UDP_PORT = 5065

print "UDP target IP:", UDP_IP
print "UDP target port:", UDP_PORT
#print "message:", MESSAGE

sock = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP
 
def main():
    sys.stdout.write("asdfasdf\n")

    cap = cv2.VideoCapture(0)

    def areaRatio(roi2):
            hsv2 = cv2.cvtColor(roi2, cv2.COLOR_BGR2HSV)
            mask2 = cv2.inRange(hsv2, lower_skin, upper_skin)
            mask2 = cv2.dilate(mask2,kernel,iterations = 1)
            contours2,hierarchy2= cv2.findContours(mask2,cv2.RETR_TREE,cv2.CHAIN_APPROX_SIMPLE)
            cnt2 = max(contours2, key = lambda x: cv2.contourArea(x))
            hull2 = cv2.convexHull(cnt2)
            areahull2 = cv2.contourArea(hull2)
            areacnt2 = cv2.contourArea(cnt2)
            arearatio2=((areahull2-areacnt2)/areacnt2)*100
            return arearatio2

    roi1Val = 1
    roi2Val = 1
    roi3Val = 1
    roi4Val = 1

    prevroi1Val = 1
    prevroi2Val = 1
    prevroi3Val = 1
    prevroi4Val = 1
    count = 0
    maxCount = 4
    threshold = 500
    signal = "none"
    #take the average of 4 and then check
         
    while(1):
            
        try:  #an error comes if it does not find anything in window as it cannot find contour of max area
              #therefore this try error statement
              
            ret, frame = cap.read()
            frame=cv2.flip(frame,1)
            kernel = np.ones((3,3),np.uint8)
            

            #           roi2
            #
            #   roi1    roi3 
            #
            #           roi4 
        
            #left
            roi1=frame[300:325,200:225]
            # (400, 100) to (500, 200)
            #top
            roi2=frame[100:125, 400:425]
            # (400, 300) to (500, 400)
            #middle
            roi3=frame[300:325, 400:425]
            #bottom
            roi4=frame[500:525, 400:425]

            # define range of skin color in HSV
            lower_skin = np.array([15,0,0], dtype=np.uint8)
            upper_skin = np.array([36,255,255], dtype=np.uint8)
                    #roi1
            cv2.rectangle(frame,(200,300),(225,325),(0,255,255),0)
            #roi2
            cv2.rectangle(frame,(400,100),(425,125),(0,255,255),0)
            #roi3
            cv2.rectangle(frame,(400,300),(425,325),(0,255,255),0)
            #roi4
            cv2.rectangle(frame,(400,500),(425,525),(0,255,255),0)
        
            font = cv2.FONT_HERSHEY_SIMPLEX
         
            if(count < maxCount):
                roi1Val += areaRatio(roi1)
                roi2Val += areaRatio(roi2)
                roi3Val += areaRatio(roi3)
                roi4Val += areaRatio(roi4)

                #print areaRatio(roi3)

                

                count += 1

                # left side
                if prevroi1Val<threshold:
                    cv2.putText(frame,'off',(0,150), font, 2, (0,0,255), 3, cv2.LINE_AA)  
                else:
                    cv2.putText(frame,'on',(0,150), font, 2, (0,0,255), 3, cv2.LINE_AA)
                # top 
                if prevroi2Val<450:
                    cv2.putText(frame,'off',(100,100), font, 2, (0,0,255), 3, cv2.LINE_AA)  
                else:
                    cv2.putText(frame,'on',(100,100), font, 2, (0,0,255), 3, cv2.LINE_AA)
                #middle
                
                if prevroi3Val<threshold:
                    cv2.putText(frame,'off',(100,150), font, 2, (0,0,255), 3, cv2.LINE_AA)  
                else:
                    cv2.putText(frame,'on',(100,150), font, 2, (0,0,255), 3, cv2.LINE_AA)
                #bottom
                if prevroi4Val<520:
                    cv2.putText(frame,'off',(100,200), font, 2, (0,0,255), 3, cv2.LINE_AA)  
                else:
                    cv2.putText(frame,'on',(100,200), font, 2, (0,0,255), 3, cv2.LINE_AA)



            elif(count == maxCount):
                count = 0
                roi1Val = roi1Val/maxCount
                roi2Val = roi2Val/maxCount
                roi3Val = roi3Val/maxCount
                roi4Val = roi4Val/maxCount

                prevroi1Val = roi1Val
                prevroi2Val = roi2Val
                prevroi3Val = roi3Val
                prevroi4Val = roi4Val

                # left side
                if roi1Val<threshold:
                    cv2.putText(frame,'off',(0,150), font, 2, (0,0,255), 3, cv2.LINE_AA)  
                else:
                    cv2.putText(frame,'on',(0,150), font, 2, (0,0,255), 3, cv2.LINE_AA)
                # top 
                if roi2Val<450:
                    cv2.putText(frame,'off',(100,100), font, 2, (0,0,255), 3, cv2.LINE_AA)  
                else:
                    cv2.putText(frame,'on',(100,100), font, 2, (0,0,255), 3, cv2.LINE_AA)
                #middle
                
                if roi3Val<threshold:
                    cv2.putText(frame,'off',(100,150), font, 2, (0,0,255), 3, cv2.LINE_AA)  
                else:
                    cv2.putText(frame,'on',(100,150), font, 2, (0,0,255), 3, cv2.LINE_AA)
                #bottom
                if roi4Val<520:
                    cv2.putText(frame,'off',(100,200), font, 2, (0,0,255), 3, cv2.LINE_AA)  
                else:
                    cv2.putText(frame,'on',(100,200), font, 2, (0,0,255), 3, cv2.LINE_AA)

                roi1Val = 1
                roi2Val = 1
                roi3Val = 1
                roi4Val = 1
                if(left):
                    signal="left"
                elif(top and middle):
                    signal="right"
                elif(bottom):
                    signal="stop"
                else:
                    signal="none"
                #print(signal)
                
            left = True if(prevroi1Val>threshold) else False
            top = True if(prevroi2Val>threshold) else False
            middle = True if(prevroi3Val>threshold) else False
            bottom = True if(prevroi4Val>threshold) else False

            cv2.putText(frame,signal,(300,300), font, 2, (0,0,255), 3, cv2.LINE_AA)
            
            if(signal != "none"):
                sock.sendto(str(signal) , (UDP_IP, UDP_PORT))
                
            #show the windows
            #cv2.imshow('pink',mask3)
            #cv2.imshow('yellow',mask2)
            cv2.imshow('frame',frame)
        except:
            pass
            
        
        k = cv2.waitKey(5) & 0xFF
        if k == 27:
            break
        
    cv2.destroyAllWindows()
    cap.release()    


if __name__=='__main__':
    main()
