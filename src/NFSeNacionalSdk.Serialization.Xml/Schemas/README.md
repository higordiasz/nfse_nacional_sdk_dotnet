# Embedded official NFS-e schemas

The SDK selects schemas through an immutable `NFSeLayoutProfile`; the XML `versao` remains `1.01` in both profiles.

| Profile | Official artifact | Published | Relevant changes |
| --- | --- | --- | --- |
| `LegacyV101_202602` | `NFSe-ESQUEMAS_XSD-v1.01-20260209` | 2026-02-09 | Production baseline, including the NT 004 IBS/CBS groups and `tpRetPisCofins` from NT 007; numeric CNPJ. |
| `RtcV101_202607` | `NFSe-ESQUEMAS_XSD-PRODREST-v1.01-20260727` | 2026-07-27 | RTC/IBS/CBS groups and alphanumeric CNPJ. Activated for production CNPJ handling on 2026-08-10 according to the official deployment log. |

Sources:

- https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual
- https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/producao-restrita
- https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/rtc
- https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/atualizacoes-e-implantacoes

The August 2026 production documentation page still links the February production ZIP even though the official deployment log states that alphanumeric CNPJ support was activated in production on 2026-08-10. The current profile therefore embeds the latest official July bundle that contains the corresponding XSD patterns, while the February bundle remains available as an explicit legacy profile.

The February schemas already contain the NT 004 IBS/CBS structures. Their presence in the contract is kept separate from the later operational milestone that made those groups mandatory in production on 2026-08-03.

Known July-bundle inconsistency: `TSCNPJ` and `TSIdPedRegEvt` allow the alphanumeric CNPJ positions, while `TSChaveNFSe` allows letters in a different position range. Consequently, some access keys carrying an alphanumeric CNPJ cannot validate as an event request even though `CNPJAutor` itself validates. The SDK intentionally does not patch the official XSD.

NT 009 is intentionally not represented: the official RTC page says it is not deployed in either environment and has no deployment date. Its preliminary annex is not a business-rule contract.
