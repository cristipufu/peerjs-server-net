# peerjs-server.net

ASP.NET Core Server for the [PeerJS library](https://peerjs.com/) which simplifies peer-to-peer data, video, and audio calls.

To broker connections, PeerJS connects to a PeerServer. Note that no peer-to-peer data goes through the server; The server acts only as a connection broker.

This an ASP.NET Core port of the [Node.js implementation](https://github.com/peers/peerjs-server).

## Server setup

```C#
public void ConfigureServices(IServiceCollection services)
{
    ...
    // register PeerJs Server dependencies
    services.AddPeerJsServer();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // enable PeerJs Server middleware
    app.UsePeerJsServer();        
}
```

That's it, we're good to go.

## Client library setup

The repository also contains a demo project.

However, we can read more about the library usage on the [official website](https://peerjs.com/docs.html). Here's a quick sample:

Add the PeerJS client library to your webpage:
```html
<script src="https://cdn.jsdelivr.net/npm/peerjs@0.3.20/dist/peer.min.js"></script>
```

Create the Peer object:
```js
const peer = new Peer(randomId, {
    host: 'localhost',
    port: 44329,
    path: '/'
});
```

Start a call:
```js
// Call a peer, providing our mediaStream
navigator.mediaDevices
    .getUserMedia({
        audio: true,
        video: true
    })
    .then(function (mediaStream) {
        myVideo.srcObject = mediaStream;
        // Call a peer, providing our mediaStream
        var call = peer.call('dest-peer-id', mediaStream);
    });
```

Answer call:
```js
peer.on('call', function(call) {
  // Answer the call, providing our mediaStream
  call.answer(mediaStream);
});
```

Use the stream:
```js
call.on('stream', function(stream) {
  // `stream` is the MediaStream of the remote peer.
  // Here you'd add it to an HTML video/canvas element.
});
```
