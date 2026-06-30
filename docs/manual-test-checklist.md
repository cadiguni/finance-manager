# FinTrack — Checklist de testes manuais

## 1. Objetivo

Validar os fluxos críticos do FinTrack em execução real, incluindo API ASP.NET Core, PostgreSQL, migrations, frontend servido pelo Nginx e Docker Compose.

Esta rodada foi executada em 29/06/2026 (`America/Sao_Paulo`) usando um projeto Docker Compose descartável. Nenhum banco local persistente do desenvolvedor foi alterado.

Status utilizados:

- `Pendente`: ainda precisa ser executado;
- `Passou`: resultado observado corresponde ao esperado;
- `Falhou`: resultado observado diverge do esperado;
- `Bloqueado`: o ambiente ou uma limitação impede a execução completa;
- `Melhoria futura`: comportamento conhecido que não será alterado nesta etapa.

## 2. Pré-requisitos

- Docker Engine e Docker Compose;
- `curl` e `jq` para reproduzir as chamadas HTTP;
- .NET SDK 10 para executar diretamente, ou imagem `mcr.microsoft.com/dotnet/sdk:10.0`;
- portas `3000`, `5000` e `5432` livres.

O host Fedora usado nesta rodada possui .NET 8. Os testes .NET 10 foram executados dentro do container oficial do SDK.

## 3. Como subir o ambiente

Ambiente normal:

```bash
docker compose -f infra/docker-compose.yml up --build
```

Ambiente descartável recomendado para testes manuais:

```bash
docker compose -p fintrack-manual -f infra/docker-compose.yml up --build -d
docker compose -p fintrack-manual -f infra/docker-compose.yml ps
```

Encerrar e apagar o banco descartável:

```bash
docker compose -p fintrack-manual -f infra/docker-compose.yml down -v
```

## 4. Comandos executados

```bash
docker compose -p fintrack-manual -f infra/docker-compose.yml build
docker compose -p fintrack-manual -f infra/docker-compose.yml up -d

docker run --rm \
  --user 1000:1000 \
  -e HOME=/tmp \
  -e NUGET_PACKAGES=/tmp/nuget \
  -v "$PWD:/src:Z" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet test --configuration Release
```

Resultado automatizado do backend após as correções desta rodada:

```text
Passed: 28
Failed: 0
Skipped: 0
```

## 5. Dashboard e saldos

| ID | Fluxo | Passos | Resultado esperado | Status | Observações |
| -- | ----- | ------ | ------------------ | ------ | ----------- |
| DASH-01 | Seed de junho/2026 | Consultar `GET /api/dashboard/monthly-summary?year=2026&month=6` | `balance=4193.75`, `initialBalance=2800.00`, `currentBalance=6993.75` | Passou | Valores confirmados pela API. |
| DASH-02 | Conta com saldo inicial | Criar conta com saldo inicial de R$ 1.000 | Conta criada e saldo inicial agregado | Passou | `initialBalance` total passou de R$ 2.800 para R$ 3.800. |
| DASH-03 | Transação anterior ao mês | Criar receita de R$ 100 em maio e consultar junho | Não alterar saldo mensal; alterar saldo acumulado | Passou | Receita entrou somente em `currentBalance`. |
| DASH-04 | Transação posterior ao mês | Criar despesa de R$ 900 em julho e consultar junho | Não entrar nos valores de junho | Passou | `currentBalance` de junho permaneceu sem a despesa futura. |
| DASH-05 | Receitas e despesas do mês | Criar receita de R$ 500 e despesa de R$ 200 em junho | Totais mensais e saldo líquido corretos | Passou | `totalIncome=7000.00`, `totalExpense=2506.25`, `balance=4493.75`. |
| DASH-06 | Saldo acumulado | Consultar junho após transações anteriores e atuais | Saldo inicial + receitas até junho - despesas até junho | Passou | `currentBalance=8393.75`. |
| DASH-07 | Mês sem transações | Consultar agosto/2026 | `balance=0`, mantendo saldo acumulado | Passou | `currentBalance=7493.75`, já considerando julho. |
| DASH-08 | Múltiplas contas | Criar uma terceira conta e consultar dashboard | Somar todos os saldos iniciais do usuário | Passou | Saldos seed + conta manual totalizaram R$ 3.800. |
| DASH-09 | Pagas e pendentes | Criar/alterar despesa entre paga e pendente | Atualizar `paidExpenses`/`unpaidExpenses` | Passou | Alterações persistiram e filtro `isPaid` respondeu corretamente. |
| DASH-10 | Exibição no frontend | Abrir cards do dashboard | Exibir valores retornados pela API | Melhoria futura | O frontend exibe apenas `balance`; `initialBalance` e `currentBalance` não existem no tipo TypeScript nem nos cards. Nenhuma alteração foi feita nesta etapa. |

## 6. Importação CSV

| ID | Fluxo | Passos | Resultado esperado | Status | Observações |
| -- | ----- | ------ | ------------------ | ------ | ----------- |
| CSV-01 | Preview válido | Enviar CSV com descrição, valor, tipo e data | Uma linha válida | Passou | Preview retornou `validRows=1`. |
| CSV-02 | Categorização automática | Criar regra `manualmarket` e deixar categoria vazia | Resolver categoria pela palavra-chave | Passou | Categoria correta retornada no preview. |
| CSV-03 | Primeiro commit | Confirmar CSV válido | Criar uma transação e um lote com hash | Passou | `successRows=1`; transação encontrada uma única vez. |
| CSV-04 | Dashboard após importação | Consultar mês da linha importada | Incluir valor importado | Passou | Total de despesas de julho atualizado. |
| CSV-05 | Preview do mesmo CSV | Repetir preview após commit | Marcar linha como duplicada | Passou | Erro: `Este lançamento já foi importado.` |
| CSV-06 | Commit do mesmo arquivo | Repetir commit sem alterar conteúdo | Bloquear reimportação | Passou | HTTP 400: `Este arquivo já foi importado.` |
| CSV-07 | Arquivo diferente com linha igual | Enviar uma linha repetida e uma nova | Uma inválida e uma válida | Passou | `validRows=1`, `invalidRows=1`. |
| CSV-08 | Commit parcial | Confirmar arquivo misto | Salvar somente a linha nova | Passou | `successRows=1`, `failedRows=1`. |
| CSV-09 | Data inválida | Enviar `31/31/2026` | Marcar linha inválida | Passou | Erro de data no preview. |
| CSV-10 | Valor inválido | Enviar `abc` | Marcar linha inválida | Passou | Erro de valor no preview. |
| CSV-11 | Categoria não reconhecida | Não enviar categoria nem regra compatível | Marcar linha inválida | Passou | Erro: `Category not found.` |
| CSV-12 | Mensagem visual no frontend | Executar os casos 05, 06 e 11 pela interface | Exibir mensagem retornada pela API | Bloqueado | O frontend servido respondeu HTTP 200 e o cliente propaga `body.message`, mas não foi executada automação de navegador nesta rodada. Deve ser confirmado visualmente. |

## 7. Importação Excel

| ID | Fluxo | Passos | Resultado esperado | Status | Observações |
| -- | ----- | ------ | ------------------ | ------ | ----------- |
| XLSX-01 | XLSX com campos textuais | Enviar workbook com data `yyyy-MM-dd` armazenada como texto | Preview e commit válidos | Passou | `validRows=1`, `successRows=1`. |
| XLSX-02 | Reimportar XLSX válido | Repetir commit do workbook textual | Bloquear o mesmo arquivo | Passou | HTTP 400: `Este arquivo já foi importado.` |
| XLSX-03 | XLSX sem relacionamentos | Remover `accountId` e `categoryId` | Informar os dois erros | Passou | `Account not found.` e `Category not found.` |
| XLSX-04 | XLSX real gerado pelo LibreOffice | Converter CSV válido para XLSX e enviar | Reconhecer a data da planilha | Passou | O serial numérico `46223` foi convertido para `2026-07-20`; preview e commit concluíram com uma linha válida. |
| XLSX-05 | Commit inválido seguido de nova tentativa | Confirmar arquivo sem linhas válidas e tentar novamente | Permitir nova tentativa após correção | Passou | Duas tentativas retornaram o erro de validação esperado e nenhum `ImportBatch` foi persistido. |
| XLSX-06 | Defaults pela interface | Importar planilha sem GUIDs internos | Permitir escolher conta/categoria padrão | Melhoria futura | Requests Excel não possuem defaults. Planilhas precisam carregar GUIDs ou usar regra para categoria; conta continua obrigatória na planilha. |

## 8. Importação PDF

| ID | Fluxo | Passos | Resultado esperado | Status | Observações |
| -- | ----- | ------ | ------------------ | ------ | ----------- |
| PDF-01 | PDF com camada de texto | Gerar PDF com duas linhas reconhecíveis e enviar preview | Extrair duas transações preservando linhas | Passou | O extrator de ordem visual preservou as duas linhas, descrições e valores. |
| PDF-02 | Commit do PDF textual | Confirmar o preview anterior | Salvar duas transações corretas | Passou | `successRows=2`; descrições `ManualMarket PDF` e `ManualMarket Livraria` confirmadas. |
| PDF-03 | Reimportar PDF | Repetir preview e commit | Identificar duplicata e bloquear arquivo | Passou | Preview marcou a linha gerada como duplicada e commit retornou HTTP 400. |
| PDF-04 | PDF textual não reconhecido | Enviar PDF sem linhas de compra | Não criar transações | Passou | Preview HTTP 200 com `totalRows=0`; commit HTTP 400. |
| PDF-05 | Mensagem de PDF não reconhecido | Confirmar PDF sem linhas | Mensagem amigável em português | Passou | Retorno: `Não foi possível importar o PDF porque nenhum lançamento válido foi encontrado.` |
| PDF-06 | PDF escaneado | Enviar PDF contendo somente imagem | Informar ausência de OCR sem criar dados | Bloqueado | Preview retornou zero linhas. OCR não é suportado, conforme limitação atual. |
| PDF-07 | Validação visual no frontend | Selecionar PDFs pela interface e revisar preview | Mostrar linhas e erros corretamente | Bloqueado | Requer interação visual/automação de navegador. O frontend está acessível, mas esse teste não foi automatizado. |

## 9. Exclusão de contas

| ID | Fluxo | Passos | Resultado esperado | Status | Observações |
| -- | ----- | ------ | ------------------ | ------ | ----------- |
| ACC-01 | Conta sem uso | Criar e excluir | HTTP 204 | Passou | Exclusão concluída. |
| ACC-02 | Conta com transação | Criar conta, criar transação e excluir | HTTP 409 com mensagem amigável | Passou | Mensagem em português confirmada. |
| ACC-03 | Conta em recorrência | Criar conta, criar recorrência e excluir | HTTP 409 controlado | Passou | Nenhum erro 500. |
| ACC-04 | Mensagem no frontend | Excluir conta em uso pela interface | Exibir texto da API | Bloqueado | Cliente propaga a mensagem; confirmação visual pendente. |

## 10. Exclusão de categorias

| ID | Fluxo | Passos | Resultado esperado | Status | Observações |
| -- | ----- | ------ | ------------------ | ------ | ----------- |
| CAT-01 | Categoria sem uso | Criar e excluir | HTTP 204 | Passou | Exclusão concluída. |
| CAT-02 | Categoria com transação | Criar categoria/transação e excluir | HTTP 409 amigável | Passou | Nenhum erro 500. |
| CAT-03 | Categoria com subcategoria | Criar pai/filha e excluir pai | HTTP 409 controlado | Passou | Mensagem funcional, mas ainda em inglês. |
| CAT-04 | Categoria em recorrência | Criar regra vinculada e excluir | HTTP 409 amigável | Passou | Nenhum erro 500. |
| CAT-05 | Categoria em regra de categorização | Criar keyword rule e excluir | HTTP 409 amigável | Passou | Nenhum erro 500. |
| CAT-06 | Mensagem de subcategoria | Repetir CAT-03 | Mensagem amigável em português | Passou | Retorno: `Não é possível excluir esta categoria porque ela possui subcategorias.` |
| CAT-07 | Mensagens no frontend | Executar bloqueios pela interface | Exibir texto da API | Bloqueado | Confirmação visual pendente. |

## 11. Regressão dos fluxos principais

| ID | Fluxo | Passos | Resultado esperado | Status | Observações |
| -- | ----- | ------ | ------------------ | ------ | ----------- |
| REG-01 | Criar receita | POST de receita paga | HTTP 201 | Passou | Persistida corretamente. |
| REG-02 | Criar despesa | POST de despesa pendente | HTTP 201 | Passou | Persistida corretamente. |
| REG-03 | Editar transação | PUT alterando descrição/valor | HTTP 204 | Passou | Alterações persistidas. |
| REG-04 | Marcar paga | PUT com `isPaid=true` e data | HTTP 204 | Passou | Status aceito. |
| REG-05 | Marcar pendente | PUT com `isPaid=false` sem data | HTTP 204 | Passou | Status aceito. |
| REG-06 | Filtrar transações | Filtrar por período, conta, tipo e status | Retornar somente lançamento correspondente | Passou | Uma despesa pendente retornada. |
| REG-07 | Excluir transação | DELETE por ID | HTTP 204 | Passou | Exclusão concluída. |
| REG-08 | Listar categorias | GET `/api/categories` | HTTP 200 com seed | Passou | Categorias disponíveis. |
| REG-09 | Listar contas | GET `/api/accounts` | HTTP 200 com seed | Passou | Contas disponíveis. |
| REG-10 | Dashboard mensal | Consultar após mutações | Totais atualizados | Passou | Confirmado em DASH-05. |
| REG-11 | Previsão mensal | GET de três meses | Três itens | Passou | Endpoint respondeu corretamente. |
| REG-12 | Criar parcelamento | Criar compra em três parcelas | Gerar três transações | Passou | Três descrições `Notebook Manual` encontradas. |
| REG-13 | Criar recorrência | POST da regra | HTTP 201 | Passou | Regra listável. |
| REG-14 | Gerar recorrências | Gerar até data final | Criar ocorrências esperadas | Passou | Pelo menos duas ocorrências geradas. |
| REG-15 | Frontend disponível | Consultar `http://localhost:3000` | HTTP 200 | Passou | Nginx respondeu corretamente. |
| REG-16 | Fluxos visuais | Executar formulários e ações no navegador | Feedback visual correto | Bloqueado | Não houve automação de navegador nem inspeção visual nesta rodada. |

## 12. Evidências coletadas

### Docker

```text
fintrack-api       Up
fintrack-postgres  Up (healthy)
fintrack-web       Up
```

### Migrations

```text
20260601192223_InitialCreate
20260602013446_AddDemoSeedData
20260604224820_AddCategoryKeywordRules
20260630012939_AddImportDeduplication
```

### Integridade de hashes

```text
ImportBatches: 6
ImportBatches com ContentHash: 6
Transactions importadas com ImportHash: 4

IX_Transactions_UserId_ImportHash
IX_ImportBatches_UserId_ContentHash
```

### Bateria HTTP principal

```text
38 cenários passaram
0 cenários falharam
```

Após as correções, a revalidação adicional confirmou:

```text
XLSX com data serial do LibreOffice: passou
Nova tentativa de arquivo totalmente inválido: passou
PDF textual com duas linhas: passou
Mensagem de PDF não reconhecido: passou
Mensagem de categoria com subcategoria: passou
```

## 13. Bugs encontrados e corrigidos

1. **Excel serial de data**: corrigido com conversão do sistema serial padrão do Excel para `DateOnly`.
2. **PDF perde separação de linhas**: corrigido usando `ContentOrderTextExtractor`, recomendado pelo PdfPig, no lugar de `page.Text`.
3. **Lote inválido bloqueia nova tentativa**: corrigido retornando a falha antes de persistir `ImportBatch`.
4. **Mensagens inconsistentes**: mensagens de subcategoria e PDF sem lançamentos foram traduzidas e validadas.

## 14. Melhorias futuras

- exibir `initialBalance` e `currentBalance` no frontend;
- adicionar teste visual ou E2E com Playwright;
- fornecer defaults de conta/categoria também para Excel;
- oferecer mensagem explícita para PDF escaneado sem OCR;
- padronizar as demais mensagens técnicas para português;
- avaliar workbooks que utilizem o sistema de datas Excel 1904, pois a conversão atual usa o sistema padrão 1900.

## 15. Próximo passo recomendado

Executar a validação visual pendente no frontend, especialmente mensagens de erro e estados de importação. Depois disso, o próximo incremento de backend pode tratar defaults para Excel e a mensagem específica para PDFs escaneados, antes de avançar para preview editável ou exportações.
