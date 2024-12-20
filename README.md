# **CompCredi API**

Bem-vindo à documentação oficial da **CompCredi API**. Este projeto foi desenvolvido como parte de uma trilha de aprendizado oferecida pela **Credicitrus**, uma cooperativa de crédito amplamente reconhecida na minha cidade pela sua excelência e inovação. O desafio foi criar uma aplicação inspirada no **Twitter**, com funcionalidades de gerenciamento de postagens, interações, notificações e conexões entre usuários, utilizando tecnologias modernas e aplicando as melhores práticas de desenvolvimento.

Este projeto foi uma oportunidade para demonstrar as habilidades técnicas adquiridas durante a trilha disponibilizada na plataforma **Udemy**, que cobriu **C#**, **REST com [ASP.NET](http://asp.net/) Core WebAPI**, **HTML, CSS e JavaScript**.

---

## **Índice**

1. [Visão Geral](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
2. [Objetivos do Projeto](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
3. [Principais Funcionalidades](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
4. [Requisitos Técnicos](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
5. [Instalação e Configuração](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
6. [Autenticação](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
7. [Estrutura de Endpoints](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
8. [Filtros e Paginação](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
9. [Considerações de Segurança](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
10. [Boas Práticas](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
11. [Conclusão](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)
12. [Contribuição](https://www.notion.so/Documenta-o-13f653128b9d8031b931eede9da1da34?pvs=21)

---

## **Visão Geral**

A **CompCredi API** foi construída utilizando **.NET 8** e **Entity Framework Core** para garantir alta performance, segurança e flexibilidade. A estrutura foi projetada para suportar grandes volumes de dados e permitir integração eficiente com outros sistemas ou interfaces de usuário.

### **Objetivos do Projeto**

- **Demonstrar Conhecimento Técnico**: Mostrar minha capacidade de desenvolver uma aplicação completa e escalável, baseada no desafio proposto pela **Credicitrus**.
- **Aplicar Boas Práticas de Desenvolvimento**: Desenvolver um sistema seguro e bem estruturado, com atenção especial à autenticação e ao gerenciamento eficiente de dados.
- **Criar uma Base Sólida e Flexível**: Projetar uma aplicação com uma arquitetura que permita escalabilidade e extensibilidade, adequada a diferentes contextos.

### **Principais Funcionalidades**

- **Gerenciamento de Postagens**: Criar, editar, excluir e exibir posts com suporte a mídias.
- **Autenticação e Registro**: Sistema de autenticação baseado em **JWT (JSON Web Token)**, garantindo que apenas usuários autenticados tenham acesso a certas funcionalidades.
- **Interações em Postagens**: Usuários podem curtir e comentar nas postagens, promovendo engajamento.
- **Notificações Personalizadas**: Informações sobre novas interações ou seguidores para os usuários.
- **Conexões Entre Usuários**: Funcionalidades de seguir e deixar de seguir outros usuários.
- **Filtros e Paginação**: Gerenciamento otimizado de grandes volumes de dados para melhorar o desempenho e a experiência do usuário.

---

## **Requisitos Técnicos**

Para executar este projeto, é necessário:

1. **.NET SDK 8.0 ou superior**.
2. Um banco de dados configurado com **Entity Framework Core** (SQL Server é recomendado).
3. Configuração da chave de autenticação **JWT** no arquivo `appsettings.json`.

---

## **Instalação e Configuração**

### Pré-requisitos

- **.NET SDK 8.0** ou superior.
- **SQL Server** (ou outro banco de dados compatível).

### Passo a Passo

1. Clone o repositório:
    
    ```bash
    git clone <https://github.com/kellmanny/ConstCredi.git>
    
    ```
    
2. Acesse o diretório do projeto:
    
    ```bash
    cd CompCredi-API
    
    ```
    
3. Configure o arquivo `appsettings.json` com suas credenciais e conexão de banco de dados:
    
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=projetocredi;Trusted_Connection=True;TrustServerCertificate=True;"
      },
      "Jwt": {
        "Key": "12345678910111213141516171819202122232425"
      }
    }
    
    ```
    
4. Execute as migrações para configurar o banco de dados:
    
    ```bash
    dotnet ef database update
    
    ```
    
5. Inicie a aplicação:
    
    ```bash
    dotnet run
    
    ```
    

---

## **Autenticação**

A API utiliza **JWT (JSON Web Token)** para autenticar usuários e proteger endpoints sensíveis, assegurando que apenas usuários autorizados possam acessar certas funcionalidades.

### **Como Funciona**

1. O usuário realiza login enviando suas credenciais (username e password) para o endpoint `/api/User/login`.
2. A API retorna um **token JWT**, que deve ser incluído em todas as requisições protegidas no cabeçalho `Authorization`:
    
    ```
    Authorization: Bearer <seu-token>
    
    ```
    
3. Tokens têm um tempo de validade configurado (2 horas por padrão) e devem ser renovados após expirar.

> Nota Importante: Tokens nunca devem ser armazenados de forma insegura. Use armazenamento seguro, como Secure Storage.
> 

---

## **Estrutura de Endpoints**

Abaixo está a estrutura detalhada dos principais endpoints da API **CompCredi**, organizada por áreas funcionais. Cada endpoint é descrito com seu método HTTP, URL, uma breve descrição, exemplos de requisição e resposta, além dos possíveis erros que podem ocorrer.

---

### **1. Post Controller**

### Criar um Post

**POST** `/api/Post/create`

- **Descrição**: Permite que usuários autenticados criem uma nova postagem.
- **Autenticação**: Sim (requer token JWT).
- **Requisição**:
    
    ```json
    {
      "content": "Texto do post",
      "mediaUrl": "<http://url-da-midia.com/imagem.jpg>"
    }
    
    ```
    
- **Resposta**:
    
    ```json
    {
      "message": "Post criado com sucesso",
      "postId": 42
    }
    
    ```
    
- **Erros Possíveis**:
    - **400 Bad Request**: Parâmetros obrigatórios ausentes ou inválidos.
    - **401 Unauthorized**: Token ausente ou inválido.

---

### Listar Posts da Timeline

**GET** `/api/Post/timeline-with-interactions`

- **Descrição**: Retorna uma lista de posts recentes e suas interações (curtidas e comentários).
- **Parâmetros de URL**:
    - `pageNumber` (opcional): Número da página (padrão: 1).
    - `pageSize` (opcional): Quantidade de posts por página (padrão: 10).
- **Resposta**:
    
    ```json
    [
      {
        "id": 1,
        "content": "Primeiro post",
        "createdAt": "2024-11-15T08:00:00Z",
        "username": "usuario1",
        "interactions": [
          {
            "id": 101,
            "type": "like",
            "username": "usuario2",
            "createdAt": "2024-11-15T08:05:00Z"
          }
        ]
      }
    ]
    
    ```
    
- **Erros Possíveis**:
    - **400 Bad Request**: Parâmetros inválidos.
    - **404 Not Found**: Nenhum post encontrado.

---

### Registrar Interação (Curtir/Comentar)

**POST** `/api/Post/interact`

- **Descrição**: Permite registrar interações (curtidas ou comentários) em um post.
- **Requisição**:
    
    ```json
    {
      "postId": 1,
      "type": "like",
      "content": "Comentário opcional"
    }
    
    ```
    
- **Resposta**:
    
    ```json
    {
      "message": "Interação registrada com sucesso"
    }
    
    ```
    
- **Erros Possíveis**:
    - **400 Bad Request**: Parâmetros ausentes ou inválidos.
    - **404 Not Found**: Post não encontrado.

---

### Atualizar Post

**PUT** `/api/Post/update/{postId}`

- **Descrição**: Permite que o autor de um post atualize seu conteúdo ou mídia.
- **Parâmetros**:
    - `postId`: ID do post a ser atualizado.
- **Requisição**:
    
    ```json
    {
      "content": "Novo conteúdo",
      "mediaUrl": "<http://exemplo.com/nova-imagem.jpg>"
    }
    
    ```
    
- **Resposta**:
    
    ```json
    {
      "message": "Post atualizado com sucesso"
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.
    - **404 Not Found**: Post não encontrado ou acesso negado.

---

### Deletar Post

**DELETE** `/api/Post/delete/{postId}`

- **Descrição**: Permite que o autor exclua seu post.
- **Parâmetros**:
    - `postId`: ID do post.
- **Resposta**:
    
    ```json
    {
      "message": "Post deletado com sucesso"
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.
    - **404 Not Found**: Post não encontrado ou acesso negado.

---

### Curtir ou Descurtir Post

**POST** `/api/Post/like/{postId}`

- **Descrição**: Permite que usuários curtam ou removam a curtida de um post.
- **Parâmetros**:
    - `postId`: ID do post.
- **Resposta**:
    
    ```json
    {
      "message": "Curtida registrada ou removida com sucesso"
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.
    - **404 Not Found**: Post não encontrado.

---

### Buscar Posts por Usuário

**GET** `/api/Post/user/{userId}`

- **Descrição**: Lista posts de um usuário específico, com suas interações.
- **Parâmetros**:
    - `pageNumber`: Número da página.
    - `pageSize`: Número de posts por página.
- **Resposta**:
    
    ```json
    [
      {
        "id": 1,
        "content": "Post de exemplo",
        "createdAt": "2024-11-15T10:00:00Z",
        "username": "usuario1",
        "interactions": [...]
      }
    ]
    
    ```
    
- **Erros Possíveis**:
    - **404 Not Found**: Nenhum post encontrado para o usuário.

---

### Pesquisar Posts

**GET** `/api/Post/search`

- **Descrição**: Pesquisa posts com base em palavras-chave fornecidas pelo usuário.
- **Parâmetros**:
    - `query`: Palavra-chave para a pesquisa.
- **Resposta**:
    
    ```json
    [
      {
        "id": 1,
        "content": "Post contendo a palavra-chave",
        "createdAt": "2024-11-15T10:00:00Z",
        "username": "usuario1"
      }
    ]
    
    ```
    
- **Erros Possíveis**:
    - **400 Bad Request**: Palavra-chave ausente ou inválida.

---

### **2. User Controller**

### Registrar um Novo Usuário

**POST** `/api/User/register`

- **Descrição**: Permite registrar novos usuários na plataforma.
- **Requisição**:
    
    ```json
    {
      "email": "email@dominio.com",
      "username": "novoUsuario",
      "password": "senhaForte123"
    }
    
    ```
    
- **Resposta**:
    
    ```json
    {
      "message": "Usuário registrado com sucesso"
    }
    
    ```
    
- **Erros Possíveis**:
    - **409 Conflict**: O e-mail ou nome de usuário já está em uso.
    - **400 Bad Request**: Dados inválidos.

---

### Realizar Login

**POST** `/api/User/login`

- **Descrição**: Autentica o usuário e retorna um token JWT.
- **Requisição**:
    
    ```json
    {
      "username": "usuarioExistente",
      "password": "senhaCorreta"
    }
    
    ```
    
- **Resposta**:
    
    ```json
    {
      "token": "eyJhbGciOiJIUzI1NiIsInR..."
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Credenciais inválidas.

---

### Obter Notificações do Usuário

**GET** `/api/User/notifications`

- **Descrição**: Retorna as notificações do usuário autenticado.
- **Parâmetros**:
    - `pageNumber` (opcional): Número da página.
    - `pageSize` (opcional): Quantidade por página.
- **Resposta**:
    
    ```json
    [
      {
        "id": 1,
        "message": "Você recebeu uma nova curtida!",
        "createdAt": "2024-11-15T10:00:00Z",
        "isRead": false
      }
    ]
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.

---

### Marcar Notificação Como Lida

**PUT** `/api/User/notifications/{notificationId}/mark-as-read`

- **Descrição**: Marca uma notificação específica como lida.
- **Parâmetros**:
    - `notificationId`: ID da notificação a ser marcada como lida.
- **Resposta**:
    
    ```json
    {
      "message": "Notificação marcada como lida"
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.
    - **404 Not Found**: Notificação não encontrada.

---

### Marcar Todas as Notificações Como Lidas

**PUT** `/api/User/notifications/mark-all-as-read`

- **Descrição**: Marca todas as notificações do usuário autenticado como lidas.
- **Resposta**:
    
    ```json
    {
      "message": "Todas as notificações foram marcadas como lidas"
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.

---

### Seguir um Usuário

**POST** `/api/User/follow/{userId}`

- **Descrição**: Permite que o usuário autenticado siga outro usuário.
- **Parâmetros**:
    - `userId`: ID do usuário a ser seguido.
- **Resposta**:
    
    ```json
    {
      "message": "Usuário seguido com sucesso"
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.
    - **404 Not Found**: Usuário a ser seguido não encontrado.
    - **400 Bad Request**: Usuário já está seguindo.

---

### Deixar de Seguir um Usuário

**POST** `/api/User/unfollow/{userId}`

- **Descrição**: Permite que o usuário autenticado deixe de seguir outro usuário.
- **Parâmetros**:
    - `userId`: ID do usuário a ser deixado de seguir.
- **Resposta**:
    
    ```json
    {
      "message": "Você deixou de seguir o usuário com sucesso"
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.
    - **404 Not Found**: Usuário não encontrado ou não está sendo seguido.

---

### Obter Lista de Seguidores

**GET** `/api/User/{userId}/followers`

- **Descrição**: Retorna a lista de seguidores de um usuário específico.
- **Parâmetros**:
    - `userId`: ID do usuário cujos seguidores serão listados.
- **Resposta**:
    
    ```json
    [
      {
        "id": 1,
        "username": "seguidor1",
        "email": "seguidor1@dominio.com"
      },
      {
        "id": 2,
        "username": "seguidor2",
        "email": "seguidor2@dominio.com"
      }
    ]
    
    ```
    
- **Erros Possíveis**:
    - **404 Not Found**: Usuário não encontrado.

---

### Obter Lista de Seguindo

**GET** `/api/User/{userId}/following`

- **Descrição**: Retorna a lista de usuários que um usuário específico está seguindo.
- **Parâmetros**:
    - `userId`: ID do usuário cujas conexões de "seguindo" serão listadas.
- **Resposta**:
    
    ```json
    [
      {
        "id": 3,
        "username": "seguido1",
        "email": "seguido1@dominio.com"
      },
      {
        "id": 4,
        "username": "seguido2",
        "email": "seguido2@dominio.com"
      }
    ]
    
    ```
    
- **Erros Possíveis**:
    - **404 Not Found**: Usuário não encontrado.

---

### Atualizar Senha do Usuário

**PUT** `/api/User/update-password`

- **Descrição**: Permite que o usuário autenticado atualize sua senha.
- **Requisição**:
    
    ```json
    {
      "currentPassword": "senhaAtual",
      "newPassword": "novaSenhaForte123"
    }
    
    ```
    
- **Resposta**:
    
    ```json
    {
      "message": "Senha atualizada com sucesso"
    }
    
    ```
    
- **Erros Possíveis**:
    - **401 Unauthorized**: Token ausente ou inválido.
    - **400 Bad Request**: Senha atual incorreta ou nova senha não atende aos requisitos.

---

## **Filtros e Paginação**

A API oferece suporte para filtros e paginação em vários endpoints, especialmente naqueles que retornam listas de dados, como posts e notificações. Abaixo estão os detalhes sobre como utilizar esses recursos.

### Parâmetros de Paginação

- **pageNumber**: Define o número da página a ser exibida (padrão é 1).
- **pageSize**: Define o número de itens por página (padrão é 10).

### Exemplo de Paginação

Abaixo, um exemplo de como passar os parâmetros de paginação na URL:

```
GET /api/Post/timeline-with-interactions?pageNumber=2&pageSize=5

```

> Dica: A paginação é recomendada para grandes volumes de dados, pois melhora a experiência do usuário e reduz o consumo de recursos.
> 

---

## **Considerações de Segurança**

A segurança é uma prioridade na **CompCredi API**, e algumas práticas foram implementadas para garantir a integridade dos dados e proteger o sistema contra acessos indevidos.

- **Autenticação JWT**: Todos os endpoints sensíveis estão protegidos com autenticação JWT, garantindo que apenas usuários autenticados possam acessá-los.
- **HTTPS**: Recomendamos que a API seja executada sobre HTTPS para proteger as comunicações contra interceptação.
- **Rate Limiting**: A API limita o número de requisições por IP a 10 por minuto, retornando o status **429 Too Many Requests** em caso de excesso, prevenindo abusos.

---

## **Boas Práticas**

Para um uso eficaz e seguro da API, seguem algumas boas práticas recomendadas:

1. **Armazenamento Seguro de Tokens**:
    - Armazene tokens JWT de forma segura, evitando expô-los em locais públicos ou inseguros.
2. **Teste de Endpoints**:
    - Utilize ferramentas como **Postman** ou **Insomnia** para testar os endpoints durante o desenvolvimento e integração.
3. **Configuração para Produção**:
    - Verifique que as strings de conexão e chaves de autenticação estejam protegidas em ambientes de produção.
4. **Validação de Dados**:
    - Implemente validações no cliente para garantir que apenas dados corretos sejam enviados para a API, reduzindo o risco de falhas e erros.

---

## **Conclusão**

Desenvolver a **CompCredi API** como parte da trilha oferecida pela **Credicitrus** foi uma experiência enriquecedora e desafiadora. Este projeto reflete meu compromisso com boas práticas de desenvolvimento e inovação. A API foi estruturada para ser uma base sólida e escalável, com funcionalidades essenciais que podem ser expandidas para aplicações mais complexas.

Além de ser uma demonstração do meu conhecimento técnico, esta API representa o aprendizado aplicado e a valorização das oportunidades que a **Credicitrus** me proporcionou. A **CompCredi API** é um exemplo prático de como transformar teoria em soluções reais e aplicáveis ao mercado.

---

## **Contribuição**

Contribuições são bem-vindas! Se você deseja colaborar com melhorias nesta API, siga as etapas abaixo:

1. Faça um fork do repositório.
2. Crie uma nova branch (`git checkout -b feature/MinhaFeature`).
3. Commit suas alterações (`git commit -m 'Adiciona nova funcionalidade'`).
4. Faça push para a branch (`git push origin feature/MinhaFeature`).
5. Abra um pull request para revisão.

---
