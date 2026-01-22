Feature: Login (resolved)

  Scenario: Login com gaps visíveis
    Given I open the "Login" page
    # UIGAP: UIGAP-0001 UI_MAP_KEY_NOT_FOUND — Element key não encontrada no UIMap
    When I fill "login.username" with "demo"
    When I fill "login.password" with "demo"
    # UIGAP: UIGAP-0002 AMBIGUOUS_MATCH — Mais de um candidato para o testId "btn-login"
    And I click "login.submit"
    Then I should see "home.welcome"
