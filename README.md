# Pré-requisitos
* Ter instalado o .NET versão 9.0.100 com o pacote de desenvolvimento web;
* Ter o Microsoft Visual Studio Code instalado;
* Estar com a porta 5013 livre;

# Configuração do banco de dados
O projeto já está configurado para usar um banco existente na Vercel. 
Para alterar o banco de dados para o de sua preferência é necessário seguir os seguintes passos:

* Na pasta server, abra o arquivo appsettings.json;
* Altere o campo DefaultConnection com as informações do banco que você pretende usar;
* Abra o Console do Gerenciador de Pacotes do Microsoft Visual Studio Code;
* Selecione o ServerLibrary no campo de projeto padrão do console;
* Execute o comando Add-Migration <nome_de_sua_preferencia>;
* Execute o comando Update-Database;

## Observação:
O banco de dados a ser configurado deve ser PostgreSql.

# Instruções
* Abra o Microsoft Visual Studio Code;
* Vá em abrir solução;
* Selecione o arquivo AtendimentosClinicos.sln na pasta raíz desse projeto;
* Após carregar o projeto selecione Server como projeto padrão;
* Selecione a opção de excução http e execute o projeto

## Observação:
O frontend do projeto (https://github.com/AmintasNeto/AtendimentosClinicosFrontend) deve estar sendo executado na porta 5180.
