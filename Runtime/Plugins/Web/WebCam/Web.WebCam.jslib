//const WebWebCamPlugin = {
//	$WebWebCam: {
//        WebCamStartCustom: function(cameraName) {
//            var deviceID = -1;
//            if (!navigator.mediaDevices?.enumerateDevices) {
//                console.log("enumerateDevices() not supported.");
//                return;
//            } else {
//                // List cameras and microphones.
//                navigator.mediaDevices.enumerateDevices().then(
//                    (devices) => {
//                        devices.forEach(
//                            (device) => {
//                                if (device.kind === 'video' && device.name === cameraName) {
//                                    deviceID = device.deviceID;
//                                }
//                            }
//                        );
//                    }
//                )
//            }
//            if (deviceID === -1){
//                console.error("Cannot start video input with name "+cameraName+". No such name exists! Existing video inputs are:");
//                console.dir(videoInputDevices);
//                return;
//            }
//            if (activeWebCams[deviceId]) {
//                ++activeWebCams[deviceId].refCount;
//                return;
//            }
//            if (!videoInputDevices[deviceId]) {
//                console.error("Cannot start video input with ID "+deviceId+". No such ID exists! Existing video inputs are:");
//                console.dir(videoInputDevices);
//                return;
//            }
//            navigator.mediaDevices.getUserMedia(
//                {
//                    audio:false,
//                    video:videoInputDevices[deviceId].deviceId? {
//                        deviceId: {
//                            exact: videoInputDevices[deviceId].deviceId
//                        },
//                        width:  { min: 640,     ideal: 1280,    max: 1920 },
//                        height: { min: 480,     ideal: 720,     max: 1080 },
//                    }:true
//                }
//            ).then(
//                function(stream) {
//                    var video = document.createElement("video");
//                    video.srcObject=stream;
//                    if (/(iPhone|iPad|iPod)/.test(navigator.userAgent)) {
//                        warnOnce("Applying iOS Safari specific workaround to video playback: https://bugs.webkit.org/show_bug.cgi?id=217578");
//                        video.setAttribute("playsinline","")
//                    }
//                    video.play();
//                    var canvas = document.createElement("canvas");
//                    activeWebCams[deviceId] = {
//                        video:video,
//                        canvas:document.createElement("canvas"),
//                        stream:stream,
//                        frameLengthInMsecs:1e3/stream.getVideoTracks()[0].getSettings().frameRate,
//                        nextFrameAvailableTime:0,
//                        refCount:1
//                    }
//                }
//            ).catch(
//                function(e){
//                    console.error("Unable to start video input! "+e);
//                }
//            )
//        }
//	},

//	///////////////////////////////////////////////////////////////////////////
//    WebCamStartCustom: function(cameraName) {
//        WebWebCam.WebCamStartCustom(cameraName);
//    }
//};

//autoAddDeps(WebWebCamPlugin, '$WebWebCam');
//mergeInto(LibraryManager.library, WebWebCamPlugin);
