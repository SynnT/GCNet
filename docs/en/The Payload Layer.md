# **The Payload Layer**
What you see below is the decrypted payload of our packet (now with the padding removed).

> ![](https://i.imgur.com/xhOXPeG.png)

As mentioned earlier, it's the container of the most relevant information in the client-server communication among all the packet data. Like the security layer, the payload layer has its own segments: the **header**, the **content** and the **null padding**. Let's explain them one by one.

> Note: as opposed to the security layer, the data in the payload layer are mostly written as [big-endian](https://en.wikipedia.org/wiki/Endianness#Big-endian).

## Header
> ![](https://i.imgur.com/C19kDWK.png)

The payload **header** contains three essential fields: the packet **opcode**, the **content size** and the **compression flag**. Next, we'll take a closer look at these values.

### Opcode (operation code)
> ![](https://i.imgur.com/JJfLbND.png)

The opcode is the packet identifier — it indicates what is the purpose of the packet, what it is.

For example, the packet whose opcode is 1 is the one in which the session keys are transmitted. The one whose opcode is 12 or `00 1C`, shown above, is called SHA_FILENAME_LIST_ACK and its purpose is to inform the client about the files that will be verified through SHA checksum.

### Content Size
> ![](https://i.imgur.com/pTkORlB.png)

This is the **size** in bytes of the payload content. 

In our case, it is `00 00 00 40` in hexadecimal, that is, 10 in decimal. See it by yourself: count each byte from the 1st one after the header to the 5th last. Your count should reach 64.

### Compression Flag
> ![](https://i.imgur.com/OZSqBEU.png)

The **compression flag** is a boolean value that indicates whether the content is compressed or not.

When the flag is _true_ ( `01` ), it means that the content data is compressed. Otherwise, it's _false_ ( `00` ) and means that the data is uncompressed, which is the case of our packet. The compression iself will be discussed soon.

## Content
> ![](https://i.imgur.com/EbaO45Q.png)

Basically, the **content** is the message in its purest form. Actually, it's the information that the packet is really transmitting.

The content varies from packet to packet as result of the [serialization](https://en.wikipedia.org/wiki/Serialization) of many data strucures. Let's take the serialization of a string as an example.

> ![](https://image.prntscr.com/image/276d51bc2b4e4b2e820c1abefad4ab21.png)

The portion highlighted in purple is a [unicode](https://en.wikipedia.org/wiki/Unicode) string that represents the name of the file _main.exe_, as can be easily noted in the picture. But what about the bytes in red? ( `00 00 00 10` )? They're a 4-byte integer that corresponds to the size of the string that follows it. In decimal, `00 00 00 10` is 16, which is exactly the size of our string in bytes.

Strings are one of the many serializable data structures in the Grand Chase packets, as _arrays_, _vectors_, _lists_, _sets_, _maps_ e _pairs_ (in addition to the primitive types as _int_, _char_, _float_, etc.). The serialization modes (very similar) of these structures can be understood in a simplified way through the pseudocode below:

```js
function SerializeString(string) {
    WriteData(string.sizeInBytes)
    foreach (character in string) {
        WriteData(character)
    }
}

function SerializeArray(array) {
    WriteData(array.size)
    foreach (element in array) {
        WriteData(element)
    }
}

function SerializeVector(vector) {
    WriteData(vector.size)
    foreach (element in vector) {
        WriteData(element)
    }
}

function SerializeList(list) {
    WriteData(list.size)
    foreach (element in list) {
        WriteData(element)
    }
}

function SerializeSet(set) {    
    WriteData(set.size)
    foreach (element in set) {
        WriteData(element)
    }
}

function SerializeMap(map) {
    WriteData(map.size)
    foreach (element in map) {
        WriteData(element.key)
        WriteData(element.value)
    }
}

function SerializePair(pair) {
    WriteData(pair.firstValue)
    WriteData(pair.secondValue)
}
```

## Null Padding
> ![](https://i.imgur.com/9ICryEF.png)

It's only a padding composed by 4 bytes `00` at the end of each payload. 

> Note: there are some packets whose payloads contain only **opcode**, **size** (with value zero) and **null padding**. They're mostly packets whose opcode is the only information that the receiver needs to know.

---
## **Compression**
Some of the Grand Chase packets contain its payload's content compressed.

To compress data, Grand Chase uses [zlib](https://en.wikipedia.org/wiki/Zlib). We can tell this because of the presence of one of the zlib headers ( `78 01` ) in all compressed payloads.

Let's take a look at one compressed content.

> Note: for demonstration purposes, the packet ENU_CLIENT_CONTENTS_FIRST_INIT_INFO_ACK will be shown.

> ![](https://i.imgur.com/3t1MGKn.png)

(As you can see in the bytes in red, the compression flag is _true_ and the zlib header ir present)

Only the section highlighted in purple is compressed: the payload header and the first 4 bytes of the content ramain uncompressed, as well as the `00 00 00 00` padding at the end.

The first 4 bytes of the content of the compressed payloads (in our case, highlighted in blue) are an integer number that represents the **size of the section in purple after it's decompressed**. Note that, differently from the other data in the payload, this size is written as little-endian, thus being 764 in decimal.

After the data is decompressed, the content can be read normally as any uncompressed one.

**Continue reading**: [The Security Protocol Setup](./The%20Security%20Protocol%20Setup.md#the-security-protocol-setup)