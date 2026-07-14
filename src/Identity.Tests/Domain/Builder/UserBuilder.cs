using BuildingBlocks.Abstractions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.ValueObject;

namespace Identity.Tests.Domain.Builder;

/// <summary>
///   Builder de teste para User.
///   PRINCÍPIO TEÓRICO — Por que isso existe:
///   O problema que o Builder resolve é chamado de "Obscure Test" (teste obscuro)
///   no catálogo de XUnit Test Patterns (Gerard Meszaros). Quando o setup de um
///   teste é longo e repetitivo, o LEITOR do teste perde de vista qual parte do
///   setup é RELEVANTE para o que está sendo testado e qual é só "boilerplate
///   necessário para compilar".
///   O Builder aplica o princípio de "Intent-Revealing": cada teste customiza
///   SÓ o que importa para aquele cenário específico, e tudo o resto recebe um
///   valor padrão sensato e válido. Quem lê o teste enxerga imediatamente o que
///   está sendo variado.
///   Compare:
///   var user = User.Create(
///   CompleteName.Create("João", "Silva"),
///   Email.Create("joao@example.com"),
///   Password.Create("Senha@123"),
///   Role.Create("Customer", "Cliente padrão", RoleType.Customer)
///   ).Value;
///   versus:
///   var user = new UserBuilder().WithEmail("joao@example.com").Build();
///   A segunda versão deixa claro: "este teste se importa com o email,
///   o resto é irrelevante para o cenário".
/// </summary>
public sealed class UserBuilder
{
  private Email _email = Email.Create("default.user@example.com");

  private CompleteName _name = CompleteName.Create("Default", "User");

  private Password _password = Password.CreateWithNoProvider("DefaultPass@123");

  private Role _role = Role.Create("Default customer role");

  public UserBuilder WithName(string firstName, string lastName)
  {
    _name = CompleteName.Create(firstName, lastName);

    return this;
  }

  public UserBuilder WithEmail(string email)
  {
    _email = Email.Create(email);

    return this;
  }

  public UserBuilder WithPassword(string plainPassword)
  {
    _password = Password.CreateWithNoProvider(plainPassword);

    return this;
  }

  public UserBuilder WithRole(Role role)
  {
    _role = role;

    return this;
  }

  public UserBuilder AsAdmin()
  {
    _role = Role.Create("Administrator role", RoleType.Admin);

    return this;
  }

  /// <summary>
  ///   Constrói o User. Retorna a entidade diretamente (não o Result&lt;User&gt;)
  ///   porque, no contexto de um Builder de teste, assumimos que os dados
  ///   fornecidos são válidos — se não forem, o teste DEVE falhar ruidosamente
  ///   (a exceção/erro do próprio User.Create se propaga), o que é o
  ///   comportamento certo: um builder não deveria mascarar uma falha de setup.
  /// </summary>
  public User Build()
  {
    Result<User> user = User.Create(_name, _email, _password, _role);

    return user.Value;
  }
}
