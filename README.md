# WebApiPagamento

O WebApiPagamento é uma API desenvolvida em C# com ASP.NET Core, responsável pelo gerenciamento e processamento de pagamentos em uma arquitetura baseada em microsserviços.

O projeto utiliza RabbitMQ para comunicação assíncrona entre serviços, permitindo receber pedidos através de filas, processar pagamentos e publicar eventos para que outros microsserviços sejam notificados sobre o resultado do processamento.

A aplicação também disponibiliza endpoints REST para realizar operações de consulta, cadastro, atualização e exclusão de pagamentos.

# Tecnologia
C#
.NET ASP.NET Core
Entity Framework ou repositório de persistência 
RabbitMQ – mensageria e comunicação assíncrona.
System.Text.Json – serialização e desserialização de mensagens JSON.
Dependency Injection – injeção de dependências.
BackgroundService – execução de consumidores em segundo plano.
REST API – comunicação HTTP. 
Arquitetura e funcionamento

# Estrutura
O projeto trabalha com comunicação entre microsserviços utilizando RabbitMQ.

O fluxo de processamento funciona da seguinte maneira:

Um microsserviço de pedidos publica uma mensagem na fila order-placed.
O serviço de pagamentos consome essa mensagem de forma assíncrona.
A mensagem é desserializada para o objeto OrderPlacedEvent.
Os dados do pedido são utilizados para criar um registro de pagamento.
O pagamento recebe um status de aprovado ou rejeitado.
Após o processamento, é publicado um evento PaymentProcessedEvent.
Outros microsserviços recebem o evento através de suas respectivas filas.
Comunicação com RabbitMQ

O projeto utiliza a seguinte fila:

# Fila e Responsabilidade
order-placed	Receber pedidos para processamento de pagamento.
notifications-payment-processed	Receber eventos de pagamentos processados para notificações.
catalog-payment-processed	Receber eventos de pagamentos processados pelo serviço de catálogo.

Também é utilizado o exchange:

pagamento.eventos

O exchange utiliza o tipo Fanout, distribuindo os eventos publicados para todas as filas vinculadas.

# Regra de negócio

Atualmente, a aplicação possui uma regra de negócio demonstrativa para determinar o resultado do pagamento:

Quando o valor do pedido é igual a 1000, o pagamento é rejeitado.
Para os demais valores, o pagamento é aprovado.

Após a validação, o sistema registra o pagamento e publica um evento contendo o resultado do processamento.

# API


| Método | Rota | Descrição |
|---|---|---|---|
| `GET` | `/Pagamento` | Lista todos os pagamentos. |
| `GET` | `/Pagamento/{id}` | Busca um pagamento pelo ID. |
| `POST` | `/Pagamento` | Adiciona um novo pagamento. |
| `PUT` | `/Pagamento` | Atualiza um pagamento existente. |
| `DELETE` | `/Pagamento?id={id}` | Exclui um pagamento. |
| `GET` | `/Pagamento/addpagamentofila` | Método de exemplo, somente para testar e não tem ligação com o fluxo principal e adiciona um pagamento à fila. |
| `GET` | `/Pagamento/addpagamentofila` | Método de exemplo para teste e recebe o pagamento através da fila. |


GET	/Pagamento	Lista todos os pagamentos.
GET	/Pagamento/{id}	Busca um pagamento pelo ID.
POST	/Pagamento	Adiciona um novo pagamento.
PUT	/Pagamento	Atualiza um pagamento existente.
DELETE	/Pagamento?id={id}	Exclui um pagamento.
GET	/Pagamento/receberpagamentofila	Recebe pagamentos através da fila.
GET	/Pagamento/addpagamentofila	Adiciona um pagamento à fila.

Exemplos de utilização

Cadastrar pagamento

POST /Pagamento

Exemplo de corpo da requisição:

json
{
  "valor": 150.00,
  "idJogo": 1,
  "idUsuario": 10
}

Os campos devem corresponder às propriedades definidas em PagamentoRequisicao.

Consultar pagamentos

GET /Pagamento

Retorna a lista de pagamentos cadastrados.

Consultar pagamento por ID

GET /Pagamento/1

Retorna os dados do pagamento correspondente ao identificador informado.

Atualizar pagamento

PUT /Pagamento

Recebe os dados de atualização através do objeto PagamentoAtualizacao.

Excluir pagamento

DELETE /Pagamento?id=1

Remove o pagamento correspondente ao ID informado.

# Migration

A migration inicia criando a tabela Pagamentos

# RabbitMq
O consumo das mensagens é realizado pela classe RabbitmqService, que herda de BackgroundService.

Essa implementação permite que a aplicação fique continuamente aguardando novas mensagens na fila order-placed, sem depender de uma requisição HTTP para iniciar o processamento.

A comunicação entre os microsserviços ocorre através de eventos:

OrderPlacedEvent: representa um pedido realizado.
PaymentProcessedEvent: representa o resultado do processamento do pagamento.
PaymentStatus: identifica se o pagamento foi aprovado ou rejeitado.

# Executar localmente


