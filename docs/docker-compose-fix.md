Objetivo
--------
Identificar e mitigar o erro de conflito de nome do container Docker:

✘ Error response from daemon: Conflict. The container name "/oficina_postgres" is already in use by container "<id>". You have to remove (or rename) that container to be able to reuse that name.

Causas comuns
-------------
- docker-compose define container_name fixo para o serviço (ex.: "oficina_postgres").
- Já existe um container antigo com esse nome (parado ou ativo).
- Recriar a stack com o mesmo nome causa conflito.

Soluções recomendadas
---------------------
1) Melhor prática: não definir container_name no docker-compose
   - Deixe o Docker Compose gerar nomes baseados no diretório/projeto (evita conflitos).

2) Parametrizar o container_name (se for necessário)
   - Use variável de ambiente COMPOSE_PROJECT_NAME ou variável própria.
   - Exemplo:

     container_name: "${COMPOSE_PROJECT_NAME:-oficina}_postgres"

   - Alternativa: usar uma variável específica no .env:

     container_name: "${PROJECT_PREFIX:-oficina}_postgres"

3) Alternativa quando precisar manter nome fixo
   - Antes de subir a stack, remover o container antigo:

     # PowerShell
     docker rm -f oficina_postgres

   - Ou parar e remover pelo id:
     docker rm -f 57645ff9aba47c7d72241b0a17a31653eac51467d589126a2b2b84d39a971cfa

4) Usar docker compose com flags para evitar orfãos
   - docker compose up -d --remove-orphans

Exemplo de docker-compose (recomendado, sem container_name fixo)
----------------------------------------------------------------
version: '3.8'
services:
  db:
    image: postgres:15
    environment:
      POSTGRES_USER: oficina
      POSTGRES_PASSWORD: oficina123
      POSTGRES_DB: oficina
    volumes:
      - db-data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  db-data:

Exemplo de docker-compose (parametrizando nome do container)
----------------------------------------------------------
version: '3.8'
services:
  db:
    image: postgres:15
    container_name: "${PROJECT_PREFIX:-oficina}_postgres"
    environment:
      POSTGRES_USER: oficina
      POSTGRES_PASSWORD: oficina123
      POSTGRES_DB: oficina
    volumes:
      - db-data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  db-data:

Como aplicar a correção no seu repositório
-----------------------------------------
- Se existir docker-compose.yml com container_name estático:
  - Remova a propriedade container_name ou substitua por variável.
  - Adicione um arquivo .env no repositório (não commit de segredos) com PROJECT_PREFIX único por ambiente:

    PROJECT_PREFIX=oficina_local

- Se preferir remover container existente:
  - Executar no PowerShell (na sua máquina):

    docker ps -a --filter "name=oficina_postgres"
    docker rm -f oficina_postgres

- Subir a stack com remoção de orfãos:

    docker compose up -d --remove-orphans

Notas finais
------------
- Evitar container_name fixo melhora portabilidade (múltiplos ambientes/devs simultâneos).
- Se quiser, posso procurar o docker-compose.yml no repositório e aplicar a alteração automaticamente. Confirme se deseja que eu modifique o arquivo e qual padrão prefere (remover container_name ou parametrizar com PROJECT_PREFIX/COMPOSE_PROJECT_NAME).
