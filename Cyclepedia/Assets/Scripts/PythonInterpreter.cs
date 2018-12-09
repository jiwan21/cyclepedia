using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;



public class PythonInterpreter : MonoBehaviour
{

    void Start()
    {
        string path = "/Users/taco/Documents/Fall2018/ECE180DA/opencv/test2.py";
        char[] splitter = {'\n'};
       
        Process proc = new Process();
        proc.StartInfo.FileName = "/usr/bin/python2.7";
        proc.StartInfo.CreateNoWindow = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardInput = true;
        proc.StartInfo.RedirectStandardOutput = true;

        // call hello.py to concatenate passed parameters
        proc.StartInfo.Arguments = path;
        proc.Start();

        StreamReader sReader = proc.StandardOutput;
        string[] output = sReader.ReadToEnd().Split(splitter);

        foreach (string s in output)
            UnityEngine.Debug.Log(s);

        proc.WaitForExit();
        Console.ReadLine();
    }

}