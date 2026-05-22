# NetworkScanner

Scanner de rede local em .NET com interface de terminal baseada em Spectre.Console. O projeto identifica hosts online em uma faixa de IP, tenta resolver hostname, consulta o endereço MAC por ARP e faz o mapeamento do fabricante a partir da base OUI embutida em `Data/manuf.txt`.

## Visão geral

O aplicativo foi pensado para inspeção rápida de redes locais diretamente no terminal. A execução é interativa: você informa a base do IP, define a faixa de hosts e o scanner retorna uma tabela com os dispositivos ativos.

### Recursos

- Varredura concorrente por ping em uma faixa de IPs.
- Resolução de hostname por DNS reverso quando disponível.
- Descoberta de MAC address via ARP para hosts alcançáveis.
- Identificação de fabricante por OUI usando base local embarcada.
- Saída em tabela com Spectre.Console.
- Publicação simples como executável portátil.

## Tecnologias

| Componente | Uso |
| --- | --- |
| .NET 10 | Runtime e SDK da aplicação |
| Spectre.Console | Interface interativa e renderização da tabela |
| ArpLookup | Consulta do endereço MAC via ARP |
| IPAddressRange | Dependência já referenciada no projeto |

## Requisitos

- Para executar a versão publicada `self-contained`, basta Windows x64.
- Para desenvolver ou rodar via `dotnet run`, é necessário ter o .NET 10 SDK instalado.
- Acesso à rede local que será escaneada.
- Permissões suficientes para operações de rede locais.

## Como executar

### Restaurar e compilar

```bash
dotnet restore
dotnet build
```

### Rodar em modo desenvolvimento

```bash
dotnet run
```

Durante a execução, o programa solicita:

1. Base do IP, por exemplo `192.168.1`
2. Início da faixa, por exemplo `1`
3. Fim da faixa, por exemplo `254`

## Publicação

Para gerar uma versão portátil single-file para Windows x64:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\portable
```

O executável gerado em `portable/` inclui o runtime e não depende de instalação prévia do .NET na máquina de destino.

## Exemplo de uso

Fluxo típico:

```text
Base IP: 192.168.1
Start range: 1
End range: 254
```

Saída esperada:

| IP Address | Host Name | MAC Address | Vendor | Status | Latency |
| --- | --- | --- | --- | --- | --- |
| 192.168.1.1 | router | AA:BB:CC:DD:EE:FF | Example Vendor | Online | 2 ms |

## Estrutura do projeto

```text
Core/
  HostResult.cs        # Modelo do resultado de cada host encontrado
  ScanOptions.cs       # Opções de entrada da varredura
Services/
  PingScannerService.cs  # Orquestra ping, hostname, MAC e vendor
  MacAddressService.cs   # Consulta de MAC via ARP
  VendorLookupService.cs # Resolução de fabricante via OUI
UI/
  ConsoleRenderer.cs   # Interface interativa e renderização da tabela
Data/
  manuf.txt            # Base de fabricantes embarcada como recurso
Program.cs             # Ponto de entrada da aplicação
```

## Limitações conhecidas

- A descoberta de MAC address normalmente funciona apenas na mesma sub-rede local.
- Hostnames dependem de DNS reverso ou resolução disponível no ambiente.
- A varredura considera como resultado final apenas hosts que responderam ao ping.
- O projeto está configurado para `net10.0`, então exige SDK compatível.

## Desenvolvimento

Comandos úteis:

```bash
dotnet build
dotnet run
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .\portable
```

## Contribuição

Contribuições são bem-vindas. Para manter o histórico limpo:

1. Abra uma issue descrevendo a melhoria ou problema.
2. Crie uma branch focada em uma única mudança.
3. Valide a compilação antes de abrir o pull request.

## Licença

Distribuído sob a licença MIT. Veja [LICENSE](LICENSE) para os termos completos.