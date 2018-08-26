# **A Camada de Segurança**
No Grand Chase, os packets são construídos, resumidamente, em duas camadas: a **camada de segurança** e a **camada do payload**. Vamos começar pela camada de segurança. 

Essa camada é construída em torno de uma **variação** do protocolo de segurança chamado [_**E**ncapsulating **S**ecurity **P**ayload_](https://pt.wikipedia.org/wiki/IPsec#Encapsulating_Security_Payload_(ESP)) (ESP), que promove confidencialidade às informações transimitidas, checagem de autenticidade e de integridade de dados, além de proteger a comunicação de ataques de replay. Contudo, apesar de extremamente semelhante, as alterações feitas a esse protocolo pelos desenvolvedores do Grand Chase mudaram consideravelmente os seus propósitos, conforme veremos adiante.

> Para fins de demonstração, usaremos o packet chamado SHA_FILENAME_LIST_ACK.

Se tivéssemos _sniffado_ esse packet, o buffer de sua camada de segurança seria como este:

> ![](https://i.imgur.com/GiER0Di.png)

Analisemos suas partes:
> Nota: todos os dados na camada de segurança são escritos na ordenação [little-endian](https://pt.wikipedia.org/wiki/Extremidade_(ordenação)).

## Tamanho
> ![](https://i.imgur.com/juPifVT.png)

Como o próprio nome sugere, esses dois bytes representam o **tamanho** total dos dados da camada de segurança do packet recebido. O valor está na ordenação little-endian e, portanto, é 106 em decimal. Se você contar cada byte de nosso buffer, vai perceber que ele contém exatamente 106 bytes. :smiley:

## Índice dos Parâmetros de Segurança (SPI)
> ![](https://i.imgur.com/9gVzt3M.png)

Agora, deparamo-nos com o **índice dos parâmetros de segurança**, ou, pela sigla em inglês, **SPI**.

Esses dois bytes representam um valor arbitrário que serve, originalmente, para identificar a _associação de segurança_ (**SA**) a ser utilizada pelo destinatário do packet.
Entretanto, **esse valor não é utilizado para esse propósito no Grand Chase**. Um simples teste mostra que, mesmo com o envio de packets com valores aleatórios de SPI (por qualquer uma das partes), a comunicação cliente-servidor continua fluindo normalmente. Uma análise aprofundada dos executáveis do cliente (_main.exe_) e do servidor mostra que nenhuma das partes utiliza o valor do SPI recebido na camada de segurança para identificar uma SA.

Note que o servidor e o cliente têm SAs diferentes e que é determinado um SPI para cada uma delas. Portanto, os packets enviados pelo cliente e pelo servidor conterão SPIs diferentes nas suas camadas de segurança.

> Nota: uma SA é um conjunto de parâmetros de segurança que, no caso do Grand Chase, é constituído de chave de autenticação, chave de criptografia, números de sequência e _replay window mask_. Por ora, não se preocupe com esses detalhes — eles serão explicados em outra seção, juntamente a mais detalhes sobre o SPI no Grand Chase.

## Número de Sequência
> ![](https://i.imgur.com/B9v5VDh.png)

O **número de sequência** é um contador (associado a uma SA) cujo valor é incrementado em 1 a cada packet enviado que tem como objetivo proteger a conexão, com o auxílio de uma [_bitmask_](https://pt.wikipedia.org/wiki/M%C3%A1scara_(computa%C3%A7%C3%A3o)), de [ataques de replay](https://en.wikipedia.org/wiki/Replay_attack). A máscara de bits é utilizada para verificar se o packet já foi recebido anteriormente, conforme o algoritmo a seguir _baseado_ no original do Grand Chase (em C++):

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
            // checa se o bit correspondente ao packet não foi marcado
            return ((1 << gap) & this->m_replay_window_mask) == 0; 
    }
    else
        return true;
}
```

Porém, o contador no Grand Chase não exerce essa função. Experimente parar de incrementar os valores do número de sequência (seja nos packets do servidor ou do cliente), enviando sempre o mesmo: a comunicação é completamente inalterada, ao contrário do que se esperaria. Isso ocorre pelo fato de que ambos último número de sequência ( `m_last_sequence_num` ) e _replay window mask_ ( `m_replay_window_mask` ) **não são atualizados**, permanecendo, em geral, `0` durante todo o tempo de vida da SA que os contém, o que faz com que a função mostrada retorne `true` independentemente dos valores de `sequence_num`.

> Nota: como pode ser observado no algoritmo, o número de sequência não pode ser zero. Caso contrário, o packet é considerado inválido e a comunicação cessada.

## IV (vetor de inicialização)
> ![](https://i.imgur.com/pUd7n8j.png)

É o **IV** usado no processo de criptografia do payload do packet. Para cada packet enviado, é gerado um IV aleatório que consiste em 8 bytes iguais. O processo de criptografia dos packets do jogo é descrito com maior detalhamento na [seção de criptografia](./A%20Criptografia.md#a-criptografia).

## Payload (criptografado)
> ![](https://i.imgur.com/PEtA9jj.png)

Localizado entre os 16 primeiros e os 10 últimos bytes, o **payload** é onde estão armazenadas as informações a serem comunicadas à outra parte.

À primeira vista, o payload está criptografado não informa muito. Porém, quando decriptado, ele passa a representar os dados efetivos, aqueles que nos dizem algo realmente relevante como, por exemplo, o login introduzido por um usuário ou a informação dos jogadores dentro de uma sala de missão. 

Devido à sua importância, a camada do payload em si será discutida em sua [própria seção](./A%20Camada%20do%20Payload.md#a-camada-do-payload), do mesmo modo que sua [criptografia](./A%20Criptografia.md#a-criptografia).

## Valor de Checagem de Integridade (ICV)
> ![](https://i.imgur.com/iyWTNuP.png)

Ocupando os 10 últimos bytes dos dados da camada de segurança, o **ICV** é a porção do packet que é destinada a assegurar a autenticidade e integridade dos dados transmitidos. No Grand Chase, ele consiste em uma [MD5](https://pt.wikipedia.org/wiki/MD5)-[HMAC](https://pt.wikipedia.org/wiki/HMAC) (**H**ash-based **M**essage **A**uthentication **C**ode).

> ![](https://i.imgur.com/G7wV9BW.png)

O cálculo do ICV é feito sobre a parcela do packet destacada acima (do primeiro byte após o tamanho até o último byte do payload criptografado) e de uma chave de autenticação de 8 bytes que é definida no início da sessão de comunicação (isso será mais detalhado na [última seção](./A%20Configuração%20do%20Protocolo%20de%20Segurança.md#a-configuração-do-protocolo-de-segurança)).

Normalmente, um MD5-HMAC teria o tamanho de 16 bytes. Entretanto, se olharmos o nosso packet, perceberemos que o ICV tem apenas 10. Isso porque, no Grand Chase, o HMAC é truncado, sendo deixado com o tamanho de 10 bytes.
> ![](https://i.imgur.com/uTFcywp.png)

Em destaque, você pode ver a parcela do HMAC presente no packet em relação ao HMAC inteiro calculado para aquele trecho dos dados do packet.

**Continue lendo**: [A Criptografia](./A%20Criptografia.md#a-criptografia)
