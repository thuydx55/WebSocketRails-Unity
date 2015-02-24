# WebSocketRails-Unity

Port of JavaScript client provided by https://github.com/websocket-rails/websocket-rails

## Misc

Refer to https://github.com/websocket-rails/websocket-rails to learn more about WebSocketRails

Refer to https://github.com/KLab/websocket-unitymobile to learn more about WebSocket-Sharp

Refer to http://wiki.unity3d.com/index.php?title=JSONObject to learn more about JSONObject

## Example

```
private WebSocketRailsDispatcher Dispatcher;

void Awake () {
	Dispatcher = new WebSocketRailsDispatcher(new Uri(Constants.SOCKET_URL));
	Dispatcher.Connect();
	Dispatcher.Bind(Constants.SOCKET_EVENT_CONNECTED, clientConnected);
}

private void clientConnected(object sender, WebSocketRailsDataEventArgs e)
{
	...
}

public void Trigger(string eventName, JSONObject data)
{
    Dispatcher.Trigger(eventName, data);
}

```