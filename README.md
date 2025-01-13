[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/Nx9InY5f)
# Trabalho Prático II
Integração de Sistemas de Informação 

Licenciatura em Engenharia de Sistemas Informáticos (regime *laboral*) 2024-25

## Constituição do grupo (individual)
| 23502    | Manuel Fernandes | a23502@alunos.ipca.pt

## Problema a resolver 
  
Título
: 
Loja Online  
<br>
Breve descrição
:
API de uma Loja Online (Ecommerce) com Login, Encomendas, Pagamentos Simulados, Carrinhos, Carrinhos partilhados. Para ser futuramente implementados com Kotlin Jetpack Compose.
<br>

## Organização do repositório

[doc/](./doc/)  documentação com o relatório


[src/](./src/)  código da solução desenvolvida  <br>
    -> [RESTful-API/](./src/RESTful-API/) API em .NET Core

[other/](./other/) outros recursos necessários  <br>
    -> [SQL-Server/](./other/SQL-Server/) Ficheiros relacionados com SQL

## 1ª Entrega

### Tema e Breve Descrição

**Tema:** API para Loja Online de E-commerce

**Descrição:**
Este projeto tem como objetivo desenvolver uma API robusta e escalável para uma plataforma de e-commerce. A API fornece funcionalidades para autenticação de usuários, gestão de produtos, processamento de pedidos, simulação de pagamentos e partilha de carrinhos entre utilizadores. Suporta integração com serviços externos e foi projetada para trabalhar com uma futura aplicação frontend em Kotlin.


### Modelo ER

O diagrama seguinte representa o modelo Entidade-Relacionamento da base de dados:


![Alt text](./doc//Modelo%20ER.png)

### Principais Rotas (*endpoints*) <br>
#### Endpoints RESTful

- **Users**
  - `GET /api/User/getAllUser` - Retorna todos os users
  - `GET /api/User/getUserById/{userId}` - Retorna o user correspondente ao parametro userId
  - `POST /api/User/AddUser` - Adiciona um utilizador 
  - `DELETE /api/User/DeleteUser/{userId}` - Apaga o user correspondente ao parametro userId
  - `PUT /api/User/UpdateUser/{userId}` - Atualiza os detelhes do user correspondente ao parametro userId 

- **Address**
  - `GET /api/Address/getAllAddress` - Retorna todos os endereços
  - `GET /api/Address/getAddressById/{userId}` - Retorna o endereco do userId do parametro
  - `POST /api/Address/AddAddressToUser/{userId}` - Adiciona/Cria um endereco para um user 
  - `DELETE /api/Address/DeleteAddressFromUser/{userId}/{addressId}` - Apaga um endereco baseado num user
  - `PUT /api/Address/UpdateAddressFromUser/{userId}/{addressId}` - Atualiza o endereco de um user

- **Logins/SignUps**
  - `POST /api/users/register` - Registar um novo utilizador.
  - `POST /api/users/login` - Login do utilizador e geração de token.

- **Adicionar/Remover/Atualizar Endereço:**
  - `POST /api/address` - Adicionar um endereço.
  - `PUT /api/address/{id}` - Atualizar um endereço.
  - `DELETE /api/address/{id}` - Remover um endereço.

- **Navegar e ver os produtos:**
  - `GET /api/products` - Obter lista de produtos.
  - `GET /api/products/{id}` - Obter detalhes de um produto específico.

- **Adicionar/Remover itens ao Carrinho:**
  - `GET /api/carts/{userId}` - Obter o carrinho de um utilizador.
  - `POST /api/carts` - Adicionar um produto ao carrinho do utilizador.
  - `DELETE /api/carts/{cartItemId}` - Remover um item do carrinho.

- **Efetuar Encomendas:**
  - `POST /api/orders` - Fazer um pedido.

#### Endpoints SOAP

- **Payments:**
  - `ProcessPayment(OrderID, Amount, PaymentMethod)` - Processar pagamentos de forma segura.
  - `GetPaymentStatus(PaymentID)` - Obter o estado de um pagamento.

### Arquitetura da solução:

#### Operações Implementadas Com SOAP XML
Apenas interações mais críticas em termos de segurança. Por sua vez, neste projeto, essa será as interações dos **Pagamentos**.<br>

#### Operações Implementadas Com RESTFUL
Todas as outras operações para além dos Pagamentos serão feitas em **Restful**. Tais como:<br>

##### Cliente:
- Logins/SignUps
- Adicionar/Remover/Atualizar Endereço
- Navegar e ver os produtos
- Adicionar/Remover itens ao Carrinho
- Efetuar Encomendas <br>

##### Admin:
- Adicionar/Remover/Atualizar Utilizadores
- Adicionar/Remover/Atualizar Produtos
- Adicionar/Remover/Atualizar Categorias
- Adicionar/Remover/Atualizar Stock
- Adicionar/Remover/Atualizar Encomendas

#### SQL Server
Base de dados *hosted* no Microsoft Azure <br>
Tabelas SQL existentes neste projeto:
- Address
- Carts
- Category
- OrderDetails
- Order
- Payments
- ProductDetail
- Products
- Stock
- User
- UserAddress (*Bridge/Junction table*)

#### Serviços Externos 

1. **Gateway de Pagamentos:**
   - Integração baseada em SOAP para simulação de pagamentos segura e fiável.
   - 
2. **API de Geolocalização:**
   - Integração para sugerir locais de recolha próximos com base na morada do utilizador.
