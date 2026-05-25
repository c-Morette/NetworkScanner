# Network Scanner

Scanner de rede local para terminal em **.NET 10** com interface rica baseada em **Spectre.Console**. Informe a faixa de IPs, aguarde a varredura e receba uma tabela com hosts ativos, hostnames, endereços MAC e fabricantes — tudo direto no terminal.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Windows x64](https://img.shields.io/badge/Windows-x64-0078D4?logo=windows)
![License MIT](https://img.shields.io/badge/License-MIT-yellow)

---

## Funcionalidades

- Varredura **concorrente controlada** (até 32 hosts simultâneos) para descobrir dispositivos sem saturar o canal WiFi
- **Ping com 3 tentativas espaçadas** por destino — pega celulares em modo de economia de energia (Doze) que ignoram o primeiro pacote
- Descoberta dupla: **ICMP ping** + leitura direta da **tabela ARP do Windows** (via `iphlpapi.dll`) como rede de segurança
- Resolução de **hostname** por DNS reverso quando disponível
- Identificação de **fabricante** por OUI usando base local embarcada (`Data/manuf.txt`)
- Saída em **tabela interativa** com bordas e cores via Spectre.Console
- Suporte a **re-varredura** sem reiniciar o programa
- Publicação como executável **portátil single-file** sem dependência de runtime

### Exemplo de saída

| IP Address | Host Name | MAC Address | Vendor | Status | Latency |
|---|---|---|---|---|---|
| 192.168.0.1 | router | AABBCCDDEEFF | Tp-link Technologies | Online | 2 ms |
| 192.168.0.10 | desktop | 112233445566 | Intel Corporate | Online | 5 ms |
| 192.168.0.42 | - | 9C8E991A2B3C | Samsung Electronics | Online | 87 ms |
| 192.168.0.55 | - | F0998C112233 | Apple, Inc. | Online | ARP |

Na coluna **Latency**, valor em milissegundos significa que o host respondeu ao ping; a palavra **`ARP`** indica que o host só foi visto pela tabela ARP do sistema (sem resposta ICMP).

---

## Requisitos

**Para rodar o executável publicado:**
- Windows x64

**Para desenvolver ou rodar via `dotnet`:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Acesso à rede local e permissões suficientes para operações de rede

---

## Como usar

### Executável portátil

O executável já compilado está versionado em [`portable/NetworkScanner.exe`](portable/NetworkScanner.exe) — basta baixar e executar, **não requer .NET instalado**.

### Desenvolvimento

```powershell
git clone https://github.com/c-Morette/NetworkScanner.git
cd NetworkScanner
dotnet run
```

O programa solicita três informações:

1. **Base do IP** — por exemplo `192.168.0`
2. **Início da faixa** — por exemplo `1`
3. **Fim da faixa** — por exemplo `254`

Ao final da varredura, é possível escolher entre **Scan again** ou **Exit**.

> Uma varredura de `/24` (254 IPs) leva aproximadamente **50 segundos** no pior caso — o tempo é gasto, principalmente, em IPs vazios que aguardam 3 tentativas de ping. Hosts vivos respondem na primeira tentativa e liberam o slot rapidamente.

---

## Publicação

Para regerar o executável portátil single-file para Windows x64:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\portable
```

O binário gerado em `portable\` inclui o runtime e pode ser distribuído sem dependências externas.

---

## Como a descoberta funciona

A combinação de técnicas existe porque dispositivos WiFi modernos (especialmente celulares Android/iOS em modo de economia) frequentemente **descartam o primeiro pacote ICMP** para não acordar a CPU à toa.

1. **Concorrência limitada** a 32 sondagens simultâneas — evita que rajadas de centenas de pings sejam vistas como flood pelo chip WiFi do dispositivo, que descartaria os pacotes em lote.
2. **Até 3 pings por host**, com 1 segundo de espaçamento entre tentativas — espelha o ritmo do `ping.exe` do Windows e dá tempo do chip acordar a CPU para responder na 2ª ou 3ª tentativa.
3. **Leitura da tabela ARP do Windows** ao final do scan — captura hosts que, mesmo sem responder ao ping, deixaram entrada válida na tabela após qualquer tráfego anterior.

---

## Estrutura do projeto

```
NetworkScanner/
├── Core/
│   ├── HostResult.cs           # Modelo de resultado de cada host
│   └── ScanOptions.cs          # Parâmetros de entrada da varredura
├── Services/
│   ├── PingScannerService.cs   # Orquestra ping com retries, throttling, ARP e DNS
│   ├── ArpTableService.cs      # Lê a tabela ARP do Windows via iphlpapi.dll
│   ├── MacAddressService.cs    # Consulta de MAC address via ARP (fallback)
│   └── VendorLookupService.cs  # Identificação de fabricante por OUI
├── UI/
│   └── ConsoleRenderer.cs      # Interface interativa e renderização da tabela
├── Data/
│   └── manuf.txt               # Base OUI embarcada como recurso do assembly
├── portable/
│   └── NetworkScanner.exe      # Executável single-file pré-publicado
└── Program.cs                  # Ponto de entrada
```

---

## Dependências

| Pacote | Versão | Uso |
|---|---|---|
| [Spectre.Console](https://spectreconsole.net/) | 0.55.2 | Interface interativa e renderização da tabela |
| [ArpLookup](https://github.com/nikeee/dotnet-arp) | 2.1.88 | Consulta de MAC address via ARP (fallback) |
| [IPAddressRange](https://github.com/jsakamoto/ipaddressrange) | 6.3.0 | Manipulação de faixas de IP |

---

## Licença

Distribuído sob a licença **MIT** — veja o arquivo [LICENSE](LICENSE).

---

## Contribuindo

Contribuições são bem-vindas. Para reportar bugs ou sugerir melhorias, abra uma [issue](../../issues). Para enviar código, crie um branch focado em uma única mudança e abra um Pull Request.
