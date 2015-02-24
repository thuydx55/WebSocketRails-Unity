using System;
using System.Collections.Generic;

namespace WebSocketRails
{
    public class WebSocketRailsDataEventArgs : EventArgs
    {
        public WebSocketRailsDataEventArgs(JSONObject data)
        {
            Data = data;
        }

        public JSONObject Data { get; private set; }
    }
}
