# **The Security Protocol Setup**

In this section, we'll be discussing something important: the security protocol setup. Below, we'll understand how the estabilishment of the security protocol occurs in the beginning of the client-server communication.

The packet whose opcode is 1, which defines the SA properties to be used by the client and by the server (essentialy, the encryption and authentication keys that will be used through the rest of the session), is the first packet with data to be sent over the connection and is sent by the server.

> Note: the beginning of a session takes place every time a client connects successfully to a Center Server or Game Server. Also note that many connections may be estabilished throughout the gameplay.

Maybe you're asking yourself what keys are used in the security layer of the first packet if the keys are inside its payload. It'll be clarified soon, let's move forward.

The data in the security layer of this packet would look like the following one:

> ![](https://i.imgur.com/jD40Gtt.png)

Observe the value of the SPI. In the "initial packet", the value of SPI is zero. This tells us that _the security protocol, in this packet, uses a pre-shared security association by the parties_. That is, default keys, both already known by the server and by the client, are used to build the security layer of this packet.

* **Default Encryption Key:** `C7 D8 C4 BF B5 E9 C0 FD`
* **Default Authentication Key:** `C0 D3 BD C3 B7 CE B8 B8`

> Note: the sequence number of the default SA is incremented for every packet sent using this SA's parameters. Therefore, as this packet is sent once for each successful connection, the sequence number of this packet is equal to the total number of connections to a server since its startup. Thus, don't be surprised with the (often high) values of this field in this packet.

---

That said, now let's analyse the payload of this packet.

> ![](https://i.imgur.com/RRBIfbO.png)

It's like any other: it has header, content and null padding at the end. Let's focus on the content.

The highlighted values compose the initial state of the security association that will be used by the client. Its fields, marked in the picture above, are, respectively:

* **(Green) New SPI:** `8E E7`
* **(Purple) Authentication Key:** `62 88 F3 A7 D3 2C 87 C5`
* **(Red) Encryption Key:** `A4 29 1C 74 B2 CE 4A 34`
* **(Blue) Sequence Number:** `00 00 00 01`
* **(Yellow) Last Sequence Number:** `00 00 00 00`
* **(Brown) Replay Window Mask:** `00 00 00 00`

> Note: the three last values are always these same values shown above. The three first ones vary for each created session.

Note that if we go back to the packet that we looked at in the first section (sniffed in the same session as the one shown in this section), we can see that [its SPI](./The%20Security%20Layer.md#security-parameters-index-spi) ( `E7 8E` ) corresponds to the SPI defined in the "initial packet" ( `8E E7` ). The detail is that the SPI was written as little-endian in the security layer, while big-endian in the "initial packet's" content.

And for now, that's all :smile: