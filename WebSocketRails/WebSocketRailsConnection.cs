using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace WebSocketRails
{
    public class WebSocketRailsConnection
    {
        private Uri uri;
	    private WebSocketRailsDispatcher dispatcher;
	    private List<WebSocketRailsEvent> message_queue;
	    private WebSocket webSocket;
	
	    public WebSocketRailsConnection(Uri uri, WebSocketRailsDispatcher dispatcher) 
        {
            this.uri = uri;
            this.dispatcher = dispatcher;
            this.message_queue = new List<WebSocketRailsEvent>();

            webSocket = new WebSocket(uri.ToString());
            webSocket.OnClose += webSocket_Closed;
            webSocket.OnMessage += webSocket_MessageReceived;
	    }

        void webSocket_MessageReceived(object sender, MessageEventArgs e)
        {
            JSONObject responseJSON = new JSONObject(e.Data);
//            UnityEngine.Debug.Log(responseJSON);

            dispatcher.NewMessage(responseJSON.list);
        }

        void webSocket_Closed(object sender, EventArgs e)
        {
            JSONObject dataJSON = new JSONObject(JSONObject.Type.ARRAY);
            dataJSON.Add("connection_closed");
            dataJSON.Add(new JSONObject());

            WebSocketRailsEvent closeEvent = new WebSocketRailsEvent(dataJSON);
            dispatcher.State = "disconnected";
            dispatcher.Dispatch(closeEvent);            
        }

	    public void Trigger(WebSocketRailsEvent _event) 
        {
	        if (dispatcher.State != "connected")
	            message_queue.Add(_event);
	        else
            {
	            webSocket.Send(_event.Serialize());	
//                UnityEngine.Debug.LogError(_event.Serialize());
            }
	    }
	
	    public void FlushQueue(String id) 
        {
	        foreach (WebSocketRailsEvent _event in message_queue)
	        {
                _event.ConnectionId = id;
	            String serializedEvent = _event.Serialize();
	            webSocket.Send(serializedEvent);
//                UnityEngine.Debug.LogError(serializedEvent);
	        }		
	    }

        public void Connect()
        {
            webSocket.Connect();
        }

	    public void Disconnect() 
        {
		    webSocket.Close();
	    }
    }
}
