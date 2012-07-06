/* See the copyright information at https://github.com/srfoster/June/blob/master/COPYRIGHT */
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

using UnityEngine;
using System.Collections;


public class Server
{
    private TcpListener tcpListener;
    private Thread listenThread;
	public String last_message = "";
	private CallResponseQueue queue;
	public bool applicationRunning = true;
	
    public Server(CallResponseQueue queue)
    {

	  this.queue = queue;
      this.tcpListener = new TcpListener(IPAddress.Loopback, 3000);
      this.listenThread = new Thread(new ThreadStart(ListenForClients));
	  this.listenThread.IsBackground = true;
      this.listenThread.Start();
    }
	
	
	private void ListenForClients()
    {
      this.tcpListener.Start();

	  while (true)
	  {
		if(!this.tcpListener.Pending())
		{
			Thread.Sleep (100);		
		} else {

		    //blocks until a client has connected to the server
		    TcpClient client = this.tcpListener.AcceptTcpClient();
		
		    //create a thread to handle communication 
		    //with connected client
		    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
		    clientThread.Start(client);
		}	

	  }
	}
	
	
	private void HandleClientComm(object client)
	{
	  TcpClient tcpClient = (TcpClient)client;
	  NetworkStream clientStream = tcpClient.GetStream();
	
	  byte[] message = new byte[4096];
	  int bytesRead;
	
	  while (applicationRunning)
	  {
		using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"/Codespells/Codespells/ConsoleOutput.txt", true))
		{
			file.WriteLine("Top of handling client communication loop.");
		}

	    bytesRead = 0;
	
	    try
	    {
	      //blocks until a client sends a message
	      bytesRead = clientStream.Read(message, 0, 4096);
	    }
	    catch
	    {
	      //a socket error has occured
	      break;
	    }
			

	
	    if (bytesRead == 0)
	    {
	      //the client has disconnected from the server
	      break;
	    }
	
	    //message has successfully been received
	    ASCIIEncoding encoder = new ASCIIEncoding();
		String message_string = encoder.GetString(message, 0, bytesRead);
			//using (System.IO.StreamWriter file = System.IO.StreamWriter(@"C:/Codespells/Codespells/ConsoleOutput.txt"))
			//{
				//file.WriteLine("message_string: "+message_string+"\n");
			
		using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"/Codespells/Codespells/ConsoleOutput.txt", true))
		{
			file.WriteLine("Unity gets from Java: "+message_string);
		}
			
				//UnityEngine.Debug.Log ("message_string: "+message_string);
			//}
		CallResponse call_response = new CallResponse(message_string, clientStream);
		queue.add(call_response);
		
		using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"/Codespells/Codespells/ConsoleOutput.txt", true))
		{
			file.WriteLine("Bottom of handling client communication loop.");
		}
	  }
		
	  using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"/Codespells/Codespells/ConsoleOutput.txt", true))
	  {
	    file.WriteLine("Unity communication thread exiting.");
	  }
	
	  tcpClient.Close();
	}
	
}


