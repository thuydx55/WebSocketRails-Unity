using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketRails
{
    public class WebSocketRailsChannel
    {
        public bool isSubscribed = false;
	    private String eventName;
        private Dictionary<String, List<EventHandler<WebSocketRailsDataEventArgs>>> callbacks;
	    private String channelName;
	    private String token;
	    private WebSocketRailsDispatcher dispatcher;
	
	    public WebSocketRailsChannel(String channelName, WebSocketRailsDispatcher dispatcher, bool isPrivate)
	    {
            String eventName = null;
            if (isPrivate)
                eventName = "websocket_rails.subscribe_private";
            else
                eventName = "websocket_rails.subscribe";
        
            this.channelName = channelName;
            this.dispatcher = dispatcher;

            JSONObject frame = new JSONObject(JSONObject.Type.ARRAY);
            frame.Add(eventName);

            JSONObject dataJSON = new JSONObject();
            JSONObject infoJSON = new JSONObject();

            infoJSON.AddField("channel", channelName);
            dataJSON.AddField("data", infoJSON);

            frame.Add(dataJSON);
            frame.Add(dispatcher.ConnectionId);
//            UnityEngine.Debug.LogWarning(frame);
            WebSocketRailsEvent _event = new WebSocketRailsEvent(frame, null, null);

            callbacks = new Dictionary<String, List<EventHandler<WebSocketRailsDataEventArgs>>>();
    
            dispatcher.TriggerEvent(_event);
	    }

        public void Bind(String eventName, EventHandler<WebSocketRailsDataEventArgs> callback)
        {
		
	        if (! callbacks.ContainsKey(eventName))
                callbacks[eventName] = new List<EventHandler<WebSocketRailsDataEventArgs>>();
	    
	        callbacks[eventName].Add(callback);
	    }

	    public void Trigger(String eventName, JSONObject message) 
        {
            JSONObject frame = new JSONObject();
            frame.Add(eventName);

            JSONObject infoJSON = new JSONObject();
            infoJSON.AddField("channel", channelName);
            infoJSON.AddField("data", message);
            infoJSON.AddField("token", token);

            frame.Add(infoJSON);
            frame.Add(dispatcher.ConnectionId);

//            UnityEngine.Debug.LogError(frame);
            WebSocketRailsEvent _event = new WebSocketRailsEvent(frame, null, null);
		
	        dispatcher.TriggerEvent(_event);		
	    }
	
	    public void Dispatch(String eventName, JSONObject message) 
        {
	        if(eventName == "websocket_rails.channel_token") {
                this.token = message["token"].str;
                isSubscribed = true;
	        }
	        else {
	            if (! callbacks.ContainsKey(eventName))
	                return;

                foreach (EventHandler<WebSocketRailsDataEventArgs> callback in callbacks[eventName])
	            {
	                callback(this, new WebSocketRailsDataEventArgs(message));
	            }
	        }		
	    }
	
	    public void Destroy() 
        {
	        String eventName = "websocket_rails.unsubscribe";
        
            JSONObject frame = new JSONObject();
            frame.Add(eventName);

            JSONObject dataJSON = new JSONObject();
            JSONObject infoJSON = new JSONObject();
            infoJSON.AddField("channel", channelName);
            dataJSON.AddField("data", infoJSON);

            frame.Add(dataJSON);
            frame.Add(dispatcher.ConnectionId);
        
            WebSocketRailsEvent _event = new WebSocketRailsEvent(frame, null, null);
	    
	        dispatcher.TriggerEvent(_event);
	        callbacks.Clear();		
            isSubscribed = false;
	    }
    }
}
