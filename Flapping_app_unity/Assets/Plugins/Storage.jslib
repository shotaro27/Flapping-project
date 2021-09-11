var StoragePlugin = {
  GetLocalStorage: function(key) {
    var value_str = localStorage.getItem(Pointer_stringify(key));
    if (!value_str) return "";
    var bufferSize = lengthBytesUTF8(value_str) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(value_str, buffer, bufferSize);
    return buffer;
  },
  SetLocalStorage: function(key, value) {
    localStorage.setItem(Pointer_stringify(key), Pointer_stringify(value));
  }
}
mergeInto(LibraryManager.library, StoragePlugin);