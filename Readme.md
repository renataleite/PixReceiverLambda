# PixReceiverLambda

Função Lambda para processar transações Pix e enviar mensagens para uma fila SQS.

## Descrição

Este projeto implementa uma função AWS Lambda que recebe transações Pix, processa os dados e envia uma mensagem para uma fila Amazon SQS. O propósito é demonstrar como utilizar AWS Lambda e SQS para processar transações financeiras de forma eficiente e escalável.

## Estrutura do Projeto

- `Function.cs`: Contém a lógica principal da função Lambda.
- `PixReceiverLambda.csproj`: Arquivo de configuração do projeto .NET.
- `Readme.md`: Documentação do projeto.

## Pré-requisitos

- .NET SDK 8.0 ou superior
- AWS CLI configurado com as credenciais apropriadas
- Conta AWS com permissões para criar e gerenciar funções Lambda e filas SQS

## Configuração

1. **Clone o repositório**:

```bash
git clone https://github.com/seu-usuario/PixReceiverLambda.git
cd PixReceiverLambda/src/PixReceiverLambda
```

2. **Instale as dependências do projeto**:

```bash
dotnet restore
```

3. **Configure o AWS CLI**:

```bash
aws configure
```

## Implementação

### Código da Função Lambda

```csharp
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PixReceiverLambda;

public class Function
{
    // URL da fila SQS
    private static readonly string QueueUrl = "https://sqs.us-east-1.amazonaws.com/440724126630/PixQueue";

    private readonly IAmazonSQS _sqsClient;

    public Function() : this(new AmazonSQSClient()) { }

    public Function(IAmazonSQS sqsClient)
    {
        _sqsClient = sqsClient;
    }

    public async Task<string> FunctionHandler(PixTransaction pixTransaction, ILambdaContext context)
    {
        var message = JsonConvert.SerializeObject(pixTransaction);
        var request = new SendMessageRequest
        {
            QueueUrl = QueueUrl,
            MessageBody = message
        };
        await _sqsClient.SendMessageAsync(request);
        return "Transaction received and sent to SQS";
    }
}

public class PixTransaction
{
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Implantação da Função Lambda

1. **Compile o projeto**:

```bash
dotnet build
```

2. **Implante a função Lambda**:

```bash
dotnet lambda deploy-function PixReceiverLambda --region us-east-1
```

## Teste

1. **Crie um Evento de Teste no Console da AWS Lambda**:
    - Vá para a aba "Test" (Testar) no console da função Lambda.
    - Crie um novo evento de teste com o seguinte payload:

    ```json
    {
        "TransactionId": "12345",
        "Amount": 100.50,
        "Sender": "Alice",
        "Receiver": "Bob",
        "Timestamp": "2023-06-21T12:34:56Z"
    }
    ```

2. **Execute o teste e verifique os logs no CloudWatch** para garantir que a mensagem foi enviada para a fila SQS.
