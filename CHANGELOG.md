# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] — 2026-05-17

### Added
- Rule 9 cross-platform type alignment: `TaskRunLevel` and `TaskTriggerType` changed from `string` to enum types
- `IsSystemContext()` helper updated to compare against `TaskRunLevel.Highest`
- Elevation error translation for write cmdlets
- `STATUS.md` and `AGENTS.md` contributor documentation
- `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`
- CODEOWNERS file
- PR validation workflow (`pr-validation.yml`)
- GitHub issue templates (bug report, feature request, code review finding)
- PR template with build/test checklist
- OpenCode configuration (`.opencode/`) for standalone development
- Copyright headers on all source files (Rule 10)

### Fixed
- `SetScheduledTaskCommand` stub test now expects `NotImplementedException`
- `SetScheduledTaskCommand` stub writes `ErrorRecord` instead of throwing (Rule 28)
- `SupportsShouldProcess` removed from stub cmdlets (Rule 25)
- Elevation tests on Windows now skipped correctly

### Changed
- All 22 linux-rules.md applied and verified
- Pester test assertion for default RunLevel updated (`'LeastPrivilege'` instead of `'Limited'`)

## [0.3.0] — 2026-05-09

### Fixed
- `$script:isLinux` collision → `$script:onLinux`
- `fail-fast: false` in GHA matrix
- All 8 integration `Describe` blocks now guard on `$script:hasSystemd`
- `BeOfType` null quirk → `Should -Not -Throw`
- openSUSE image now includes `gawk`+`findutils`

## [0.2.0] — 2026-05-08

### Added
- Weekly/AtStartup trigger integration tests
- Pipeline bulk disable/enable tests
- `Start-ScheduledTask` marker-file run test
- `Get-ScheduledTaskInfo` LastRunTime assertion
- NUnitXML test output and artifact upload
- Windows pester job

## [0.1.0] — 2026-05-05

### Added
- Initial release
- 13 full cmdlets + 2 stubs
- 63 Pester tests
- `getuid()` P/Invoke replaces `id -u` subprocess
- 5-distro GHA matrix
