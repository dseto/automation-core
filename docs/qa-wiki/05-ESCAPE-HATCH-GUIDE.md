# 05 - Mecanismo de Escape-Hatch: Executando JavaScript

**O que é um "Escape-Hatch"?**

É uma "saída de emergência". A plataforma foi projetada para que você não precise programar. No entanto, em situações muito raras e complexas, pode ser necessário executar uma pequena porção de código JavaScript diretamente no navegador. Este é o seu último recurso.

**Quando usar?**
- Manipulação de `localStorage` ou `sessionStorage`.
- Interação com componentes de terceiros que não expõem `data-testid`s (ex: um mapa interativo).
- Disparar eventos de navegador customizados.
- Clicar em um elemento que não responde ao clique padrão do Selenium.

**Aviso:** Use com moderação. A execução de JavaScript acopla seu teste à implementação da página, tornando-o mais frágil. Sempre prefira usar os steps padrão.

---

## ⚙️ Comandos Disponíveis

Existem dois steps de Escape-Hatch disponíveis.

### 1. Execução de Script Global

Este step executa um script no contexto geral da página (nível `window`).

**Gherkin:**
```gherkin
Quando eu executo o script JS "<seu_script_aqui>"
```

**Exemplos:**

-   **Definir um item no localStorage:**
    ```gherkin
    Quando eu executo o script JS "localStorage.setItem('feature_flag', 'true');"
    ```

-   **Rolar a página para o final:**
    ```gherkin
    Quando eu executo o script JS "window.scrollTo(0, document.body.scrollHeight);"
    ```

### 2. Execução de Script em um Elemento

Este step executa um script onde o elemento que você especifica é passado como o primeiro argumento (`arguments[0]`).

**Gherkin:**
```gherkin
Quando eu executo o script "<seu_script_aqui>" no elemento "<nome_do_elemento>"
```

**Exemplos:**

-   **Forçar um clique em um elemento problemático:**
    ```gherkin
    Quando eu executo o script "arguments[0].click();" no elemento "botao_submit_legado"
    ```

-   **Alterar o valor de um campo de data (que não é um input padrão):**
    ```gherkin
    Quando eu executo o script "arguments[0].value = '2026-01-20';" no elemento "campo_data_custom"
    ```

-   **Mudar a visibilidade de um elemento oculto:**
    ```gherkin
    Quando eu executo o script "arguments[0].style.display = 'block';" no elemento "menu_escondido"
    ```

---

## 📖 Boas Práticas e Cuidados

1.  **Sempre tente os steps padrão primeiro.** O Escape-Hatch é a exceção, não a regra.
2.  **Mantenha os scripts curtos e simples.** Se a lógica for complexa, ela provavelmente deveria estar no código da aplicação, não no teste.
3.  **Não coloque lógica de negócio no JavaScript.** O script deve apenas manipular a UI de forma pontual.
4.  **Documente o porquê.** Adicione um comentário no seu arquivo `.feature` explicando por que o Escape-Hatch foi necessário.
    ```gherkin
    # Usando JS para clicar pois o botão tem um event listener que bloqueia o clique padrão
    Quando eu executo o script "arguments[0].click();" no elemento "botao_problematico"
    ```

O Escape-Hatch é uma ferramenta poderosa, mas com grande poder vem grande responsabilidade. Use-o para destravar seus testes, não como uma muleta para evitar o mapeamento correto de elementos.
