# jcstack.abp.modules
Shared reusable JcStack ABP modules extracted from `Asms`.

## Included modules
- `jcstack.abp.auditlogging` — enhanced audit logging module
- `jcstack.abp.blobstorage` — file and blob storage module
- `jcstack.abp.identity` — extended ABP identity module
- `jcstack.abp.excel` — Excel import/export abstractions and MiniExcel integration
- `jcstack.abp.message` — messaging, emailing, and SignalR notifications module

## Layout
Each module keeps its own ABP layered structure and may include `src/`, `test/`, `common.props`, and its own solution file.

## Consumption
In `Asms`, this repository is consumed as the `modules/jcstack.abp.modules` git submodule.
