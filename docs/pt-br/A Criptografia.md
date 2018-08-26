# **A Criptografia**
> Nota: para um melhor entendimento, é fortemente recomendado que você leia os conteúdos dos links fornecidos nesta seção.

Os payloads dos packets do Grand Chase são criptografados usando o [algoritmo DES](https://pt.wikipedia.org/wiki/Data_Encryption_Standard) através do [modo CBC](https://pt.wikipedia.org/wiki/Modo_de_opera%C3%A7%C3%A3o_(criptografia)#Modo_CBC_.28Cipher-block_chaining.29) (_**C**ipher **B**lock **C**haining_).

O processo de criptografia usa um **IV**, uma **chave de criptografia** de 8 bytes e um **_plaintext_** (texto claro) para produzir a saída encriptada (_ciphertext_).
* Como dito anteriormente, um IV é gerado para cada pacote e é enviado junto com ele na camada de segurança;
* O _plaintext_ corresponde ao payload não criptografado.

Pelo modo CBC, os dados são processados com a criptografia DES em blocos de 8 bytes cada. E se o tamanho do payload não for divisível por 8? É o que veremos abaixo.

## Preenchimento (padding)
> ![](https://i.imgur.com/85Nc0vm.png)

Acima, podemos ver o payload decriptado do nosso pacote, mas, por ora, vamos nos limitar apenas à parte destacada. 

Essa parte em vermelho é o **prenchimento**. Sua função é preencher os dados até que eles chegem a um determinado tamanho divisível pelo comprimento do bloco de criptografia (no nosso caso, 8 bytes).

Tomemos nosso payload aqui como exemplo. Sem preenchimento, seu tamanho seria 75, que não é divisível por 8. O próximo número divisível por 8 depois de 75 é 80, então o tamanho do nosso preenchimento deve ser 5 (75 + 5 = 80). Assim, nós começamos a contar: `01`, `02`, `03`, `04` e `04` de novo. O último byte do preenchimento é sempre igual ao penúltimo. 

Depois disso, nós temos `01 02 03 04 04`: um preenchimento de 5 bytes.

## O Algoritmo de Preenchimento

Como você pode ter imaginado, é impossível que o preenchimento tenha seu último byte igual ao penúltimo se, por exemplo, seu tamanho fosse apenas 1. Vamos explicar o algoritmo um pouco melhor agora.

Na verdade é bem simples: quando a distância do tamanho do payload ao próximo número divisível por 8 for maior ou igual a 2, o comprimento do preenchimento será essa distância. Quando for menor, será o comprimento do bloco (8) mais a distância. Depois disso, o único passo restante é escrever os bytes.

O trecho de código (em C#) abaixo deve ser mais esclarecedor.
```cs
byte[] CalculatePadding(int payloadLength) 
{
    // calcula a distância entre o tamanho do payload e o próximo valor divisível por 8
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
E aqui está uma tabela com todos os 8 preenchimentos possíveis para os payloads do jogo para acabar com qualquer dúvida restante:

| Distância          | Tamanho do Padding | Bytes do Padding                   |
| ------------------ | ------------------ | -----------------------------------|
| 0                  | 8                  | **01 02 03 04 05 06 07 07**        |
| 1                  | 9                  | **01 02 03 04 05 06 07 08 08**     |
| 2                  | 2                  | **01 01**                          |
| 3                  | 3                  | **01 02 02**                       |
| 4                  | 4                  | **01 02 03 03**                    |
| 5                  | 5                  | **01 02 03 04 04**                 |
| 6                  | 6                  | **01 02 03 04 05 05**              |
| 7                  | 7                  | **01 02 03 04 05 06 06**           |

**Continue lendo**: [A Camada do Payload](./A%20Camada%20do%20Payload.md#a-camada-do-payload)
