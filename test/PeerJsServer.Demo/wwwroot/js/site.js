"use strict";

const peerVideo = document.querySelector('#peerVideo');
const myVideo = document.querySelector('#myVideo');
const myId = document.querySelector('#myId');
const peerId = document.querySelector('#peerId');
const peerInput = document.getElementById("peerInput");
const callBtn = document.getElementById("callButton");

let randomId = Math.random().toString(36).substring(3);

const peer = new Peer(randomId, {
    host: 'localhost',
    port: 44329,
    //host: 'peerjsserverdemo20200405055319.azurewebsites.net',
    //port: 443,
    path: '/'
});

peer.on('open', function (id) {

    console.log('My peer ID is: ' + id);

    myId.innerText = id;

});

peer.on('call', function (mediaConnection) {

    navigator.mediaDevices
        .getUserMedia({
            audio: true,
            video: true
        })
        .then(function (mediaStream) {

            myVideo.srcObject = mediaStream;

            // Answer the call, providing our mediaStream
            mediaConnection.answer(mediaStream);

            mediaConnection.on('stream', function (stream) {
                // `stream` is the MediaStream of the remote peer.
                // Here you'd add it to an HTML video/canvas element.
                peerVideo.srcObject = stream;
            });
        });
});


callBtn.addEventListener("click", function (event) {
    var user = peerInput.value;

    navigator.mediaDevices
        .getUserMedia({
            audio: true,
            video: true
        })
        .then(function (mediaStream) {

            myVideo.srcObject = mediaStream;

            // Call a peer, providing our mediaStream
            var call = peer.call(user, mediaStream);

            call.on('stream', function (stream) {
                // `stream` is the MediaStream of the remote peer.
                // Here you'd add it to an HTML video/canvas element.
                peerVideo.srcObject = stream;
            });

        });

    event.preventDefault();
});