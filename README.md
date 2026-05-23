# Network Scanner

Scanner de rede local para terminal em **.NET 10** com interface rica baseada em **Spectre.Console**. Informe a faixa de IPs, aguarde a varredura e receba uma tabela com hosts ativos, hostnames, endereços MAC e fabricantes — tudo direto no terminal.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Windows x64](https://img.shields.io/badge/Windows-x64-0078D4?logo=windows)
![License MIT](https://img.shields.io/badge/License-MIT-yellow)

---

## Funcionalidades

- Varredura **concorrente** de uma faixa de IPs via ping
- Resolução de **hostname** por DNS reverso quando disponível
- Descoberta de **MAC address** via ARP para hosts alcançáveis
- Identificação de **fabricante** por OUI usando base local embarcada (`Data/manuf.txt`)
- Saída em **tabela interativa** com bordas e cores via Spectre.Console
- Suporte a **re-varredura** sem reiniciar o programa
- Publicação como executável **portátil single-file** sem dependência de runtime

### Exemplo de saída

| IP Address | Host Name | MAC Address | Vendor | Status | Latency |
|---|---|---|---|---|---|
| 192.168.1.1 | router | AA:BB:CC:DD:EE:FF | Tp-link Technologies | Online | 2 ms |
| 192.168.1.10 | desktop | 11:22:33:44:55:66 | Intel Corporate | Online | 5 ms |

---

## Requisitos

**Para rodar o executável publicado:**
- Windows x64

**Para desenvolver ou rodar via `dotnet`:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Acesso à rede local e permissões suficientes para operações de rede

---

## Como usar

### Desenvolvimento

```powershell
git clone https://github.com/c-Morette/NetworkScanner.git
cd NetworkScanner
dotnet run
```

O programa solicita três informações:

1. **Base do IP** — por exemplo `192.168.1`
2. **Início da faixa** — por exemplo `1`
3. **Fim da faixa** — por exemplo `254`

Ao final da varredura, é possível escolher entre **Scan again** ou **Exit**.

### Executável portátil

Baixe o executável na página de [Releases](../../releases/latest) e execute diretamente — não requer .NET instalado.

---

## Publicação

Para gerar um executável portátil single-file para Windows x64:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\portable
```

O binário gerado em `portable\` inclui o runtime e pode ser distribuído sem dependências externas.

---

## Estrutura do projeto

```
NetworkScanner/
├── Core/
│   ├── HostResult.cs           # Modelo de resultado de cada host
│   └── ScanOptions.cs          # Parâmetros de entrada da varredura
├── Services/
│   ├── PingScannerService.cs   # Orquestra ping, hostname, MAC e vendor
│   ├── MacAddressService.cs    # Consulta de MAC address via ARP
│   └── VendorLookupService.cs  # Identificação de fabricante por OUI
├── UI/
│   └── ConsoleRenderer.cs      # Interface interativa e renderização da tabela
├── Data/
│   └── manuf.txt               # Base OUI embarcada como recurso do assembly
└── Program.cs                  # Ponto de entrada
```

---

## Dependências

| Pacote | Versão | Uso |
|---|---|---|
| [Spectre.Console](https://spectreconsole.net/) | 0.55.2 | Interface interativa e renderização da tabela |
| [ArpLookup](https://github.com/nikeee/dotnet-arp) | 2.1.88 | Consulta de MAC address via ARP |
| [IPAddressRange](https://github.com/jsakamoto/ipaddressrange) | 6.3.0 | Manipulação de faixas de IP |

---

## Licença

Distribuído sob a licença **MIT** — veja o arquivo [LICENSE](LICENSE).

---

## Contribuindo

Contribuições são bem-vindas. Para reportar bugs ou sugerir melhorias, abra uma [issue](../../issues). Para enviar código, crie um branch focado em uma única mudança e abra um Pull Request.
