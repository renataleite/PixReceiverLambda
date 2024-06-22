# PixReceiverLambda

PixReceiverLambda é uma função AWS Lambda escrita em C# usando .NET 8.0 que processa transações Pix e integra com a API Plaid para executar pagamentos via Open Finance.

## Funcionalidades

- Recebe detalhes de transações Pix
- Cria um destinatário de pagamento usando a API Plaid
- Cria um consentimento de pagamento usando a API Plaid
- Executa o pagamento usando o consentimento de pagamento fornecido

## Requisitos

- SDK .NET 8.0
- AWS Lambda
- Conta na API Plaid com client ID e secret
- Papéis e políticas IAM da AWS para acessar Lambda e SQS

## Configuração

1. Clone o repositório:
    ```sh
    git clone https://github.com/renataleite/pix-receiver-lambda.git
    cd PixReceiverLambda/src/PixReceiverLambda
    ```

2. Instale as dependências:
    ```sh
    dotnet restore
    ```

3. Configure as credenciais e papéis da AWS para acesso ao Lambda e SQS.

4. Configure as variáveis de ambiente para a API Plaid na configuração do AWS Lambda:
    - PLAID_CLIENT_ID
    - PLAID_SECRET
    - PLAID_ENVIRONMENT (sandbox, development ou production)

## Construção e Implantação

1. Construa o projeto:
    ```sh
    dotnet build
    ```

2. Implante a função Lambda usando o AWS CLI ou o Console de Gerenciamento da AWS.

## Uso

Envie uma transação Pix para a função Lambda. A função processará a transação, criará um destinatário, criará um consentimento de pagamento e executará o pagamento.

### Exemplo de Payload

```json
{
    "TransactionId": "trans-123",
    "Amount": 100.50,
    "Sender": "Alice",
    "Receiver": "Bob",
    "Timestamp": "2024-06-22T15:00:00Z",
    "FinanceData": "Dados financeiros de exemplo"
}
```

## Logs e Monitoramento

A função registra etapas importantes e erros no AWS CloudWatch. Você pode monitorar os logs para solucionar qualquer problema.

## Testes

Você pode testar a função usando o console AWS Lambda ou o AWS CLI, fornecendo um payload JSON similar ao exemplo acima.

## Contribuição

Contribuições são bem-vindas! Por favor, abra uma issue ou envie um pull request para quaisquer melhorias ou correções de bugs.

## Agradecimentos

- [Plaid API](https://plaid.com/docs/)
- [AWS Lambda](https://aws.amazon.com/lambda/)
- [AWS SQS](https://aws.amazon.com/sqs/)
