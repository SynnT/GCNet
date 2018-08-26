# **A Configuração do Protocolo de Segurança**

Nesta seção, estaremos discutindo algo importante: a configuração do protocolo de segurança. Abaixo, vamos entender como ocorre o estabelecimento do protocolo de segurança no começo da comunicação cliente-servidor.

O packet de opcode 1, que define as propriedades da SA a ser usada pelo cliente e pelo servidor (essencialmente, as chaves de criptografia e autenticação a serem usadas ao longo do restante da sessão), é o primeiro packet com dados a ser enviado pela conexão, sendo o envio feito pelo servidor.

> Nota: o início de uma sessão ocorre toda vez que um cliente se conecta com sucesso a um Center Server ou Game Server. Note que podem ser estabelecidas várias conexões ao longo do gameplay.

Talvez você esteja se perguntando quais chaves são utilizadas na camada de segurança do primeiro packet se as chaves estão dentro de seu payload. Isso ficará claro em breve, prossigamos. 

Os dados da camada de segurança desse packet seriam como os seguintes:

> ![](https://i.imgur.com/jD40Gtt.png)

Observemos o valor de SPI. No "packet inicial", o valor de SPI é zero. Isso indica que _o protocolo de segurança, nesse packet, utiliza uma associação de segurança pré-compartilhada pelas partes_. Isto é, são utilizadas chaves padrão, ambas já conhecidas pelo servidor e pelo cliente.

* **Chave de Criptografia Padrão:** `C7 D8 C4 BF B5 E9 C0 FD`
* **Chave de Autenticação Padrão:** `C0 D3 BD C3 B7 CE B8 B8`

> Nota: o número de sequência da SA padrão é incrementado a cada packet enviado usando os parâmetros dessa SA. Logo, como esse packet é enviado uma vez para cada conexão bem-sucedida, o número de sequência nesse packet equivale ao número total de conexões a um servidor desde seu início. Sendo assim, não se assuste com os valores (comumente grandes) desse campo nesse packet.

---

Tendo isso explicado, agora vamos analisar o payload do packet.
> ![](https://i.imgur.com/RRBIfbO.png)

Ele é como o de qualquer outro: tem cabeçalho, conteúdo, e preenchimento de bytes nulos ao final. Foquemos no conteúdo.

Os valores destacados compõe o estado inicial da associação de segurança a ser utilizada pelo cliente. Seus campos, marcados na imagem acima, são, respectivamente:

* **(Verde) Novo SPI:** `8E E7`
* **(Roxo) Chave de Autenticação:** `62 88 F3 A7 D3 2C 87 C5`
* **(Vermelho) Chave de Criptografia:** `A4 29 1C 74 B2 CE 4A 34`
* **(Azul) Número de Sequência:** `00 00 00 01`
* **(Amarelo) Último Número de Sequência:** `00 00 00 00`
* **(Marrom) _Replay Window Mask_:** `00 00 00 00`

> Nota: os três últimos valores são sempre esses mostrados no exemplo acima. Os três primeiros variam para cada sessão criada.

Note que, se voltarmos ao packet que observamos na primeira seção (_sniffado_ na mesma sessão que o packet mostrado nesta seção), podemos ver que [seu SPI](./A%20Camada%20de%20Segurança.md#Índice-dos-parâmetros-de-segurança-spi) ( `E7 8E` ) corresponde ao SPI definido no "packet inicial" ( `8E E7` ). O detalhe é que o SPI está em little-endian na camada de segurança, enquanto que, no conteúdo do "packet inicial", ele está em big-endian.

E, por enquanto, isso é tudo :smile:
