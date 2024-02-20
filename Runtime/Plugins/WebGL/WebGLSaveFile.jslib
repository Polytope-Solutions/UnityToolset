// https://forum.unity.com/threads/user-image-download-from-in-webgl-app.474715/#post-3698248
// https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html

mergeInto(LibraryManager.library, {
    SaveFile : function(array, size, fileNamePtr) {
        var fileName = UTF8ToString(fileNamePtr);

        var bytes = new Uint8Array(size);
        for (var i = 0; i < size; i++)
        {
           bytes[i] = HEAPU8[array + i];
        }

        var blob = new Blob([bytes]);
        var link = document.getElementById("FileLoader");
        if (link == null){
            link = document.createElement('a');
            link.setAttribute("id", "FileLoader");
            link.style.visibility = "hidden";
        }
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName;

        var event = document.createEvent("MouseEvents");
        event.initMouseEvent("click");
        link.dispatchEvent(event);
        window.URL.revokeObjectURL(link.href);
        // link.parentNode.removeChild(link);
    }
});
