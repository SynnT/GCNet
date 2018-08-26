# **The Security Layer**
In Grand Chase, the packets are built, basically, in two layers: the **security layer** and the **payload layer**. Let's start by the security layer.

This layer is based on a **variation** of the security protocol called [_**E**ncapsulating **S**ecurity **P**ayload_](https://en.wikipedia.org/wiki/IPsec#Encapsulating_Security_Payload) (ESP), which provides confidentiality to the transmitted data, authenticity and integrity checking, in addition to replay attacks protection. Nevertheless, despite the layer being very similar to the original one, the modifications made by the Grand Chase developers changed its purposes considerably, as we'll see forward.

> For demostration purposes, we'll use the packet called SHA_FILENAME_LIST_ACK.

If we had sniffed this packet, its security layer's buffer would look like this:

> ![](https://i.imgur.com/GiER0Di.png)

Let's analyze its fields:

> Note: every field in the security layer is written as [little-endian](https://en.wikipedia.org/wiki/Endianness#Little-endian).

## Size
> ![](https://i.imgur.com/juPifVT.png)

As the name suggests, this field contains the total **size** in bytes of the received packet's security layer. The value is written as little-endian, so it's 106 in decimal. If you count each byte in our buffer, you'll notice that it has exactly 106 bytes. :smiley:

## Security Parameters Index (SPI)
> ![](https://i.imgur.com/9gVzt3M.png)

Now, we see the **security parameters index** or **SPI**.

These two bytes represent an arbitrary number whose purpose is, originally, to identify the _security association_ (**SA**) to be used by the packet receiver. However, **this value is not used for this purpose in Grand Chase**. A simple test shows that, even if we send random SPI values (from the client or server side), the client-server communication stays unaffected. A more in-depth analysis of the client's and server's executable files shows that neither party uses the SPI value received in the security layer to identify an SA. 

Note that both client and server have different SAs and that an SPI is defined for each one of them. Thus, the packets sent by the client and the ones sent by the server will contain different SPIs in their security layers.

> Note: an SA is a set of security parameters that, in Grand Chase, is composed by an authentication key, an encryption key, sequence numbers and a _replay window mask_. By now, don't worry about these details — they'll be explained in another section, along with more information about the SPI.

## Sequence Number
> ![](https://i.imgur.com/B9v5VDh.png)

The **sequence number** is a counter (associated to an SA) whose value is incremented by 1 for each packet sent and has the purpose of protecting the connection, with the help of a [_bitmask_](https://en.wikipedia.org/wiki/Mask_(computing)), from [replay attacks](https://en.wikipedia.org/wiki/Replay_attack). The bitmask is used to check whether a packet has already been received before or not, according to the following algorithm, _based_ on the original one (in C++):

```cpp
#define REPLAY_WINDOW_SIZE 32

bool security_association::is_valid_sequence_num(unsigned int sequence_num) {
    if (sequence_num == 0)
        return false;

    if (sequence_num <= this->m_last_sequence_num) {
        unsigned int gap = this->m_last_sequence_num - sequence_num;
        if (gap >= REPLAY_WINDOW_SIZE)
            return false;
        else
            // checks if the bit corresponding to the packet hasn't been marked
            return ((1 << gap) & this->m_replay_window_mask) == 0; 
    }
    else
        return true;
}
```

Nonetheless, the counter in Grand Chase doesn't perform this function. Try to stop incrementing the value of the sequence number, sending always the same one: the communication is completely unaffected, as opposed to what we'd expect. This happens because both the last sequence number ( `m_last_sequence_num` ) and the replay window mask ( `m_replay_window_mask` ) **are not updated**, staying, usually, `0` during all the lifetime of the SA that contains them, which causes the function to return `true` regardless of the values of `sequence_num`.

> Note: as observable in the algorithm, the sequence number cannot be zero. Otherwise, the packet is labeled as invalid and the communication terminated.

## IV (initialization vector)
> ![](https://i.imgur.com/pUd7n8j.png)

It's the **IV** used in the packet payload's encryption process. For each packet sent, a random IV that consists of 8 equal bytes is generated. The encryption process of the packets are described with more details in the [cryptography section](./The%20Cryptography.md#the-cryptography).

## Payload (encrypted)
> ![](https://i.imgur.com/PEtA9jj.png)

Located between the 16 first bytes and the 10 last ones, the **payload** is where the actual information to be transmitted is stored.

At first glance, the payload is encrypted and doesn't reveal much. However, when decrypted, it represents the real data, the one that tells us something really relevant such as the login entered by a user or the information of the players inside a game room.

Due to its importance, the payload layer itself will be discussed in its [own section](./The%20Payload%20Layer.md#the-payload-layer), as well as its [cryptography](./The%20Cryptography.md#the-cryptography).

## Integrity Check Value (ICV)
> ![](https://i.imgur.com/iyWTNuP.png)

The **ICV** occupies the last 10 bytes of the security layer and is the portion of the packet that's destinated to to ensure the authenticity and integrity of the transmitted data. In Grand Chase, it consists of an [MD5](https://en.wikipedia.org/wiki/MD5)-[HMAC](https://en.wikipedia.org/wiki/HMAC). (**H**ash-based **M**essage **A**uthentication **C**ode).

> ![](https://i.imgur.com/G7wV9BW.png)

The ICV calculation is done using the above highlighted portion of the packet (from the first byte after the size to the last byte of the encrypted payload) and an 8-byte authentication key that's defined in the beginning of the communication (this will be better detailed in the [last section](./The%20Security%20Protocol%20Setup.md#the-security-protocol-setup)).

Normally, an MD5-HMAC would be 16 bytes long. However, if we look at our packet, we'll notice that the ICV contains only 10 bytes. This is because, in Grand Chase, the HMAC is truncated to the size of 10 bytes.

> ![](https://i.imgur.com/uTFcywp.png)

Highlighted, you can see the portion of the HMAC present in the packet compared to the whole calculated for that section of the packet data.

**Continue reading**: [The Cryptography](./The%20Cryptography.md#the-cryptography)