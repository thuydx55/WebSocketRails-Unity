using System;
using System.Collections.Generic;

namespace WebSocketRails
{
    public class WebSocketRailsDispatcher
    {
	    private Uri url;
	    private Dictionary<String, WebSocketRailsChannel> channels;
	    private String connectionId;
        private Dictionary<long, WebSocketRailsEvent> queue;
        private Dictionary<String, List<EventHandler<WebSocketRailsDataEventArgs>>> callbacks;
	    private WebSocketRailsConnection connection;
	
	    public WebSocketRailsDispatcher(Uri url) 
        {
            this.url = url;
            State = "connecting";
            channels = new Dictionary<String, WebSocketRailsChannel>();
            queue = new Dictionary<long, WebSocketRailsEvent>();
            callbacks = new Dictionary<String, List<EventHandler<WebSocketRailsDataEventArgs>>>();
        
            connection = new WebSocketRailsConnection(url, this);
            connectionId = "";
	    }

	    public void NewMessage(List<JSONObject> data)
        {
	        foreach (JSONObject socket_message in data)
	        {
	            WebSocketRailsEvent _event = new WebSocketRailsEvent(socket_message);
	        
	            if (_event.IsResult)
	            {
	                if (queue.ContainsKey(_event.Id))
	                {
	                    queue[_event.Id].RunCallbacks(_event.IsSuccess, _event.Data);
	                    queue.Remove(_event.Id);
	                }
	            } else if (_event.IsChannel) {
	                DispatchChannel(_event);
	            } else if (_event.IsPing()) {
	                Pong();
	            } else {
	                Dispatch(_event);
	            }
	        
                if (State == "connecting" && _event.Name == "client_connected")
	                ConnectionEstablished(_event.Data);
	        }		
	    }
	
	    public void ConnectionEstablished(JSONObject dataJSON) 
        {
            State = "connected";
            ConnectionId = dataJSON["connection_id"].str;
            connection.FlushQueue(ConnectionId);

            JSONObject frame = new JSONObject();
            frame.Add("connection_opened");
            frame.Add(new JSONObject());

            WebSocketRailsEvent openedEvent = new WebSocketRailsEvent(frame);
            Dispatch(openedEvent);   
	    }

        public void Bind(String eventName, EventHandler<WebSocketRailsDataEventArgs> callback)
        {
	        if (! callbacks.ContainsKey(eventName))
                callbacks[eventName] = new List<EventHandler<WebSocketRailsDataEventArgs>>();
	    
	        callbacks[eventName].Add(callback);		
	    }

        public void Trigger(String eventName, JSONObject data, EventHandler<WebSocketRailsDataEventArgs> success, EventHandler<WebSocketRailsDataEventArgs> failure)
        {
            JSONObject frame = new JSONObject();
		    frame.Add(eventName);

//            if (data.IsObject)
//            {
//                frame.Add(data);
//            }
//            else
//            {
                JSONObject payload = new JSONObject();
                payload.AddField("data", data);
                frame.Add(payload);
//            }

		    frame.Add(connectionId);
	        WebSocketRailsEvent _event = new WebSocketRailsEvent(frame, success, failure);
	        queue[_event.Id] = _event;
	        connection.Trigger(_event);
	    }

        public void Trigger(String eventName, JSONObject data)
        {
            Trigger(eventName, data, null, null);
        }
	
	    public void TriggerEvent(WebSocketRailsEvent _event) 
        {
            if (queue.ContainsKey(_event.Id) && queue[_event.Id] == _event)
                return;

            queue[_event.Id] = _event;
            connection.Trigger(_event);		
	    }
	
	    public void Dispatch(WebSocketRailsEvent _event) 
        {
	        if (! callbacks.ContainsKey(_event.Name))
	            return;

            foreach (EventHandler<WebSocketRailsDataEventArgs> callback in callbacks[_event.Name])
	        {
	            callback(this, new WebSocketRailsDataEventArgs(_event.Data));
	        }		
	    }

        public bool IsSubscribed(String channelName)
        {
            return channels.ContainsKey(channelName);
        }

	    public WebSocketRailsChannel Subscribe(String channelName) 
        {
	        if (channels.ContainsKey(channelName))
	            return channels[channelName];
	    
	        WebSocketRailsChannel channel = new WebSocketRailsChannel(channelName, this, false);
	    
	        channels[channelName] = channel;
	    
	        return channel;
	    }
	
	    public void Unsubscribe(String channelName) 
        {
            if (! channels.ContainsKey(channelName))
	            return;
	    
	        channels[channelName].Destroy();
	        channels.Remove(channelName);		
	    }

	    private void DispatchChannel(WebSocketRailsEvent _event)
	    {
	        if (! channels.ContainsKey(_event.Channel))
	            return;
	    
	        channels[_event.Channel].Dispatch(_event.Name, _event.Data);
	    }

	    private void Pong()
	    {
            JSONObject frame = new JSONObject(JSONObject.Type.ARRAY);
		    frame.Add("websocket_rails.pong");
		    frame.Add(new JSONObject());
		    frame.Add(ConnectionId);
		
	        WebSocketRailsEvent pong = new WebSocketRailsEvent(frame);
	        connection.Trigger(pong);
	    }

        public void Connect()
        {
            connection.Connect();
        }

	    public void Disconnect()
	    {
	        connection.Disconnect();
	    }
	
	    public String State { get; set; }
		    
	    public Uri Uri { get; set; }

        public String ConnectionId { get; set; }

    }
}
