# **A Camada do Payload**
O que você vê abaixo é o payload decriptado do nosso packet (agora com o preenchimento removido).

> ![](https://i.imgur.com/xhOXPeG.png)

Como dito anteriormente, ele é o portador das informações mais relevantes na comunicação cliente-servidor em todo o packet. Assim como o a camada de segurança, a camada do payload tem seus próprios segmentos: o **cabeçalho**, o **conteúdo** e o **preenchimento nulo**. Vamos explicá-los um a um. 

> Nota: diferentemente da camada de segurança, os dados na camada do payload são, em sua grande maioria, escritos na ordenação [big-endian](https://pt.wikipedia.org/wiki/Extremidade_(ordena%C3%A7%C3%A3o)).

## Cabeçalho
> ![](https://i.imgur.com/C19kDWK.png)

O **cabeçalho** do payload contém três informações essenciais: **_opcode_** do packet, **tamanho do conteúdo** e **indicador de compressão**. A seguir, daremos uma olhada mais de perto nesses valores.

### Opcode (código de operação)
> ![](https://i.imgur.com/JJfLbND.png)

O opcode, é o identificador do packet — ele indica qual o propósito do packet, o que ele é.

Por exemplo, o packet de opcode 1 é o packet em que as chaves da sessão são transmitidas. O de opcode 12 ou `00 1C`, mostrado acima, é o packet SHA_FILENAME_LIST_ACK, que serve para informar o cliente sobre os arquivos que serão verificados através de checksum SHA.

### Tamanho do Conteúdo
> ![](https://i.imgur.com/pTkORlB.png)

Esse é o **tamanho** em bytes do conteúdo do payload. 

No nosso caso, ele é `00 00 00 40` em valores hexadecimais, ou seja, 64 em decimal. Veja por si mesmo: conte cada byte do 1º após o cabeçalho ao 5º último. Sua contagem deverá alcançar 64.

### Indicador de Compressão
> ![](https://i.imgur.com/OZSqBEU.png)

O **indicador de compressão** é um valor booleano que indica se o conteúdo está comprimido ou não. 

Quando o indicador é _verdadeiro_ ( `01` ), significa que os dados do conteúdo estão comprimidos. Caso contrário, ele é _falso_ ( `00` ) e indica que os dados não estão comprimidos, que é o caso do nosso packet. A compressão em si será discutida em breve.

## Conteúdo
> ![](https://i.imgur.com/EbaO45Q.png)

Basicamente, o **conteúdo** é a mensagem em sua forma mais pura. Na verdade, é a informação que o packet realmente está transmitindo.

O conteúdo varia de packet para packet como resultado da [serialização](https://pt.wikipedia.org/wiki/Serializa%C3%A7%C3%A3o) de diversas estruturas de dados. Tomemos como exemplo a serialização de uma _string_.

> ![](https://image.prntscr.com/image/276d51bc2b4e4b2e820c1abefad4ab21.png)

A porção marcada em roxo é uma string [unicode](https://pt.wikipedia.org/wiki/Unicode) que representa o nome do arquivo _main.exe_, o que pode ser facilmente observado na imagem. Mas e quanto aos bytes em vermelho ( `00 00 00 10` )? Eles são um inteiro de 4 bytes que representa o tamanho da string que os sucede. Em decimal, `00 00 00 10` é 16, que é exatamente o tamanho da nossa string em bytes.

Strings são uma dos várias estruturas de dados serializáveis nos packets do Grand Chase, como _arrays_, _vectors_, _lists_, _sets_, _maps_ e _pairs_ (além dos tipos primitivos, como _int_, _char_, _float_, etc.). Os modos de serialização (bastante semelhantes) dessas estruturas podem ser entendidos, de forma simplificada, pelo pseudocódigo abaixo:

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

## Preenchimento Nulo
> ![](https://i.imgur.com/9ICryEF.png)

É apenas um preenchimento composto por 4 bytes `00` ao fim de cada payload. 

> Nota: há alguns packets cujos payloads contêm apenas **opcode**, **tamanho** (com valor zero) e **preenchimento nulo**. Em geral, são packets cujo opcode é a única informação que o destinatário precisa receber.

---
## **Compressão**
Alguns dos packets do Grand Chase tem o conteúdo de seu payload comprimido.

Para comprimir dados, o Grand Chase usa a [zlib](https://pt.wikipedia.org/wiki/Zlib). Podemos dizer isso por conta da presença de um dos cabeçalhos da zlib ( `78 01` ) em todos os payloads comprimidos.

Vamos dar uma olhada em um.

> Nota: para fins de demonstração, será mostrado o packet ENU_CLIENT_CONTENTS_FIRST_INIT_INFO_ACK

> ![](https://i.imgur.com/3t1MGKn.png)

(Como você pode ver nos bytes em vermelho, o indicador de compressão é _verdadeiro_ e o cabeçalho da zlib está presente)

Apenas a parte destacada em roxo é comprimida: o cabeçalho do payload e os 4 primeiros bytes do conteúdo permanecem normais, bem como o preenchimento `00 00 00 00` no final. 

Os 4 primeiros bytes do conteúdo dos payloads comprimidos (no nosso caso, marcados de azul) são um número inteiro que representa o **tamanho da parte marcada de roxo depois de descomprimida**. Note que, diferentemente dos outros dados do payload, esse tamanho é escrito como little-endian, sendo 764 em decimal.

Depois que os dados são descomprimidos, o conteúdo pode ser lido normalmente como qualquer um sem compressão.

**Continue lendo**: [A Configuração do Protocolo de Segurança](./A%20Configuração%20do%20Protocolo%20de%20Segurança.md#a-configuração-do-protocolo-de-segurança)