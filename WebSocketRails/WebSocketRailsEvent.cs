using System;
using System.Collections.Generic;

namespace WebSocketRails
{
    public class WebSocketRailsEvent
    {
        private bool result = false;

        private EventHandler<WebSocketRailsDataEventArgs> onEventSuccess;
        private EventHandler<WebSocketRailsDataEventArgs> onEventFailure;

        public WebSocketRailsEvent(JSONObject data, EventHandler<WebSocketRailsDataEventArgs> onEventSuccess, EventHandler<WebSocketRailsDataEventArgs> onEventFailure)
	    {
		    if(data.IsArray) 
            {
                Name = data[0].str;

                Attr = data[1];

	            if (Attr != null)
	            {
	                if (Attr.HasField("id"))
	                    Id = Attr["id"] == null ? 0L : (long)Attr["id"].f;
	                else
                        Id = (long)new Random().Next();
	            
                    if (Attr.HasField("channel"))
	                    Channel = Attr["channel"].str;
	            
                    if (Attr.HasField("data"))
	                    Data = Attr["data"];
	            
                    if (Attr.HasField("token"))
	                    Token = Attr["token"].str;
	            
	                if (data.Count > 2 && data[2] != null)
	                    ConnectionId = data[2].str;
	                else
	                    ConnectionId = "";
	                
                    if (Attr.HasField("success") && Attr["success"].str != null)
	                {
	                    result = true;
	                    IsSuccess = Attr["success"].b;
	                }
	            }

                this.onEventSuccess = onEventSuccess;
                this.onEventFailure = onEventFailure;
		    }
    	}

        public WebSocketRailsEvent(JSONObject data)
            : this(data, null, null)
        {

        }

        public bool IsPing()
        {
            return Name == "websocket_rails.ping";
        }

        public String Serialize()
        {
            JSONObject arrayJSON = new JSONObject(JSONObject.Type.ARRAY);
            arrayJSON.Add(Name);
            arrayJSON.Add(Attributes());
            arrayJSON.Add(ConnectionId);

            return arrayJSON.ToString();
        }

        public JSONObject Attributes()
        {
            JSONObject attributes = new JSONObject();
//            Dictionary<String, Object> attributes = new Dictionary<String, Object>();

            attributes.AddField("id", Id);
            if (!string.IsNullOrEmpty(Channel))
                attributes.AddField("channel", Channel);
            attributes.AddField("data", Data);
            if (!string.IsNullOrEmpty(Token))
                attributes.AddField("token", Token);

            return attributes;
        }

        public void RunCallbacks(bool success, JSONObject eventData)
        {
            if (success && onEventSuccess != null)
                FireEventSuccess(eventData);
            else
            {
                FireEventFailure(eventData);
            }
        }

        public String Name { get; set;}

        public JSONObject Attr { get; set; }

        public long Id { get; set; }

        public String Channel{ get; set;}

        public JSONObject Data{ get; set;}

        public String Token { get; set; }

        public String ConnectionId { get; set; }

        public bool IsSuccess{ get; set;}

        public bool IsChannel
        {
            get
            {
                return ! String.IsNullOrEmpty(Channel);
            }
        }

        public bool IsResult
        {
            get
            {
                return result;
            }
        }

        public event EventHandler<WebSocketRailsDataEventArgs> EventSuccess
        {
            add { onEventSuccess += value; }
            remove { onEventSuccess -= value; }
        }

        internal void FireEventSuccess(JSONObject data)
        {
            if (onEventSuccess == null)
                return;

            onEventSuccess(this, new WebSocketRailsDataEventArgs(data));
        }

        public event EventHandler<WebSocketRailsDataEventArgs> EventFailure
        {
            add { onEventFailure += value; }
            remove { onEventFailure -= value; }
        }

        internal void FireEventFailure(JSONObject data)
        {
            if (onEventFailure == null)
                return;

            onEventFailure(this, new WebSocketRailsDataEventArgs(data));
        }
    }
}
