# **The Cryptography**

> Note: to have a better understanding, it's highly recommended that you read the content of the links provided in this section.

The payload of the Grand Chase Packets are encrypted using the [DES algorithm] through the [CBC mode](https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation#Cipher_Block_Chaining_(CBC)) (**C**ipher **B**lock **C**haining).

The encryption process uses an **IV**, an 8-byte **encryption key** and a **plaintext** to produce the encrypted output (ciphertext).
* As mentioned before, an IV is generated for each packet and is sent along with it in the security layer;
* The plaintext is the unencrypted payload.

Through the CBC mode, the data is processed with the DES cryptography in blocks of 8 bytes each. What if the payload size isn't divisible by 8? We'll talk about this below.

## Padding
> ![](https://i.imgur.com/85Nc0vm.png)

Above, we can see the decrypted payload of our packet, but we'll limit ourselves only to the highlighted section.

This section in red is the **padding**. Its function is to pad the data until it reaches a determined size divisible by the encryption block length (in our case, 8).

Let's take our payload as an example. Without padding, its size would be 75, which is not divisible by 8. The next value divisible by 8 after 75 is 80, so our padding's size should be 5 (75 + 5 = 80). Thus, we start to count: `01`, `02`, `03`, `04` and `04` again. The last byte of the padding is always equal to the penultimate one.

After that, we have `01 02 03 04 04`: a 5-byte long padding

## The Padding Algorithm

As you may have thought, it's impossible that the padding have its last byte equal to the penultimate one if, for example, its size is only 1. Let's explain the algorithm a bit better now.

Actually, it's very simple: when the gap between the payload size and the next number divisible by 8 is greater or equal to 2, the padding length will be equal to this gap. When it's smaller, it'll be the encryption block length (8) plus the gap. After that, the only remaining step is to write the padding bytes

The code snippet (in C#) below should be more clarifying:
```cs
byte[] CalculatePadding(int payloadLength) 
{
    // calculates the gap between the payload size and the next value divisible by 8
    int paddingLength = (8 - (payloadLength % 8)) % 8;

    if (paddingLength <= 1)
        paddingLength += 8;

    var padding = new byte[paddingLength];

    for (byte i = 1; i < paddingLength; i++)
    {
        padding[i - 1] = i;
    }
    padding[paddingLength - 1] = padding[paddingLength - 2];

    return padding;
}
```

And to kill any remaining questions, here's a table with all the 8 possible paddings for the payloads:

| Gap                | Padding Length     | Padding Bytes                      |
| ------------------ | ------------------ | -----------------------------------|
| 0                  | 8                  | **01 02 03 04 05 06 07 07**        |
| 1                  | 9                  | **01 02 03 04 05 06 07 08 08**     |
| 2                  | 2                  | **01 01**                          |
| 3                  | 3                  | **01 02 02**                       |
| 4                  | 4                  | **01 02 03 03**                    |
| 5                  | 5                  | **01 02 03 04 04**                 |
| 6                  | 6                  | **01 02 03 04 05 05**              |
| 7                  | 7                  | **01 02 03 04 05 06 06**           |

**Continue reading**: [The Payload Layer](./The%20Payload%20Layer.md#the-payload-layer)
