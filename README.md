# Network Scanner

Scanner de rede local para terminal em C#/.NET para **descobrir dispositivos ativos na rede**, exibindo IP, hostname, MAC address, fabricante e latência direto no terminal.

![Windows 10/11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![License MIT](https://img.shields.io/badge/License-MIT-yellow)

---

## Funcionalidades

- Faz varredura **concorrente controlada** com até 32 hosts simultâneos
- Usa **3 tentativas de ping** por destino, com pausa entre tentativas
- Combina **ICMP ping** com leitura da **tabela ARP do Windows**
- Resolve **hostname** por DNS reverso quando disponível
- Identifica **fabricante** por OUI usando a base local `Data/manuf.txt`
- Exibe resultado em **tabela colorida** com Spectre.Console
- Permite **re-varredura** sem reiniciar o programa
- Pode ser publicado como executável **single-file** sem exigir runtime instalado

---

## Download

Baixe a versão mais recente em:

[Releases](https://github.com/c-Morette/NetworkScanner/releases/latest)

| Sistema | Arquivo |
|---------|---------|
| Windows 10 / 11 (x64) | `NetworkScanner.exe` |
| Windows 7 SP1 (x86) | `NetworkScanner-win7-x86.zip` |

O executável publicado já inclui o runtime necessário para execução.

### Windows 7

O build principal é **.NET 10**, que **não suporta Windows 7** (o .NET 7 em diante exige Windows 10 1607+). Por isso existe um build separado em **.NET 6** (último runtime que roda no Windows 7 SP1), publicado em **x86** — o mesmo executável serve para Windows 7 de **32 e 64 bits**.

Baixe o `NetworkScanner-win7-x86.zip`, extraia e execute **`Iniciar-Win7.cmd`** (não o `.exe` direto). O lançador confere os pré-requisitos do sistema antes de abrir e avisa o que instalar se faltar algo.

Pré-requisitos no Windows 7:

- **Windows 7 SP1** com Windows Update em dia
- **Visual C++ 2015-2019 Redistributable (x86)** — instala UCRT + `vcruntime140`/`msvcp140`
- **Atualização SHA-2 (KB4474419)** — necessária para o runtime .NET carregar

---

## Uso rápido

Execute o arquivo baixado:

```powershell
.\NetworkScanner.exe
```

O programa solicita três informações:

1. **Base do IP**: por exemplo `192.168.0`
2. **Início da faixa**: por exemplo `1`
3. **Fim da faixa**: por exemplo `254`

Ao final da varredura, é possível escolher entre **Scan again** ou **Exit**.

### Exemplo de saída

| IP Address | Host Name | MAC Address | Vendor | Status | Latency |
|---|---|---|---|---|---|
| 192.168.0.1 | router | AABBCCDDEEFF | Tp-link Technologies | Online | 2 ms |
| 192.168.0.10 | desktop | 112233445566 | Intel Corporate | Online | 5 ms |
| 192.168.0.42 | - | 9C8E991A2B3C | Samsung Electronics | Online | 87 ms |
| 192.168.0.55 | - | F0998C112233 | Apple, Inc. | Online | ARP |

Na coluna **Latency**, valor em milissegundos significa que o host respondeu ao ping. O valor `ARP` indica que o host foi encontrado pela tabela ARP do sistema, mesmo sem resposta ICMP.

---

## Desenvolvimento

```powershell
git clone https://github.com/c-Morette/NetworkScanner.git
cd NetworkScanner
dotnet run
```

### Requisitos de desenvolvimento

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10/11 x64
- Acesso à rede local

---

## Publicação

Para gerar o executável single-file para Windows x64:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\portable
```

Arquivo esperado para anexar na release:

```text
portable/NetworkScanner.exe
```

---

## Como a descoberta funciona

A combinação de técnicas existe porque dispositivos WiFi modernos, especialmente celulares Android/iOS em modo de economia de energia, podem descartar pacotes ICMP iniciais.

1. **Concorrência limitada** a 32 sondagens simultâneas evita rajadas grandes de ping.
2. **Até 3 pings por host**, com 1 segundo entre tentativas, aumenta a chance de resposta em dispositivos em economia de energia.
3. **Leitura da tabela ARP do Windows** captura hosts que deixaram entrada ARP válida mesmo sem responder ao ping.

Uma varredura de `/24` com 254 IPs pode levar aproximadamente **50 segundos** no pior caso. O tempo é gasto principalmente em IPs vazios que aguardam as tentativas de ping.

---

## Estrutura do projeto

```text
NetworkScanner/
├── Core/
│   ├── HostResult.cs           # Modelo de resultado de cada host
│   └── ScanOptions.cs          # Parâmetros de entrada da varredura
├── Services/
│   ├── PingScannerService.cs   # Orquestra ping com retries, throttling, ARP e DNS
│   ├── ArpTableService.cs      # Lê a tabela ARP do Windows via iphlpapi.dll
│   ├── MacAddressService.cs    # Consulta de MAC address via ARP como fallback
│   └── VendorLookupService.cs  # Identificação de fabricante por OUI
├── UI/
│   └── ConsoleRenderer.cs      # Interface interativa e renderização da tabela
├── Data/
│   └── manuf.txt               # Base OUI embarcada como recurso do assembly
├── portable/
│   └── NetworkScanner.exe      # Executável single-file publicado
├── Program.cs                  # Ponto de entrada
├── NetworkScanner.csproj
└── README.md
```

---

## Dependências

| Pacote | Versão | Uso |
|---|---|---|
| [Spectre.Console](https://spectreconsole.net/) | 0.55.2 | Interface interativa e renderização da tabela |
| [ArpLookup](https://github.com/nikeee/dotnet-arp) | 2.1.88 | Consulta de MAC address via ARP como fallback |
| [IPAddressRange](https://github.com/jsakamoto/ipaddressrange) | 6.3.0 | Manipulação de faixas de IP |

---

## Licença

Distribuído sob a licença **MIT**. Veja o arquivo [LICENSE](LICENSE).

---

## Contribuindo

Contribuições são bem-vindas. Para reportar bugs ou sugerir melhorias, abra uma [issue](../../issues). Para enviar código, crie um branch focado em uma única mudança e abra um Pull Request.
