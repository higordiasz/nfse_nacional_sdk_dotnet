# NFSe Nacional SDK for .NET

Open-source .NET SDK for Brazil's NFSe National Standard, focused on clean integration, serialization, transmission, and document lifecycle workflows.

## Status

> Project in early development stage.  
> The initial goal is to provide a clean, reusable, and ERP-agnostic SDK for integration with the Brazilian NFSe National Standard.

## Goals

- Provide a reusable .NET SDK for NFSe Nacional integration
- Keep the library independent from any specific ERP or business system
- Offer a clean and modern API for emission, consultation, event handling, and document lifecycle flows
- Support maintainable architecture, extensibility, and future community contributions

## Design Principles

- ERP-agnostic
- Clean architecture
- Explicit contracts
- Separation of concerns
- Testable components
- Modern .NET development practices

## Planned Features

- [ ] Environment configuration
- [ ] Request/response contracts
- [ ] XML serialization and deserialization
- [ ] HTTP transport layer
- [ ] NFSe emission workflow
- [ ] NFSe consultation by access key
- [ ] DPS consultation
- [ ] Event registration
- [ ] Event consultation
- [ ] Error normalization
- [ ] Logging support
- [ ] Dependency injection support
- [ ] Samples and usage examples

## Solution Structure

```text
src/
  NFSeNacionalSdk.Core
  NFSeNacionalSdk.Contracts
  NFSeNacionalSdk.Serialization.Xml
  NFSeNacionalSdk.Transport.Http
  NFSeNacionalSdk

tests/
  NFSeNacionalSdk.Tests

samples/
  NFSeNacionalSdk.Samples.Console
```

## Target Frameworks

The project currently targets:

- .NET 8
- .NET 10

## Contributing

Please read [CONTRIBUTING.md](./CONTRIBUTING.md) before opening issues or pull requests.

## Commit Convention

This project follows a conventional commit style.

Examples:

- `feat: add initial NFSe client contract`
- `fix: correct XML serialization for service amount`
- `docs: add project contribution guidelines`
- `refactor: simplify transport response handling`
- `test: add unit tests for environment options`

## Pull Requests

Pull requests should be small, focused, and clearly described.  
Please include context, motivation, and test coverage whenever applicable.

## License

This project is licensed under the MIT License.
