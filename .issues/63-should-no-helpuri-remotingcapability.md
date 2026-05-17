---
name: SHOULD — No HelpUri or RemotingCapability on any cmdlet
labels: [enhancement, SHOULD]
---

## Rule violated
- **Rule number:** Rule 12
- **Rule name:** Every cmdlet must declare HelpUri and RemotingCapability

## Location
- **Files:** All 15 cmdlets in `src/ScheduledTasks.Linux.Native/Commands/`

## What's wrong
No `[Cmdlet]` attribute includes `HelpUri` or `RemotingCapability`.

## How to fix
Add `HelpUri` (pointing to Microsoft Learn pages for Windows equivalents) and `RemotingCapability = RemotingCapability.SupportedByCommand` to all `[Cmdlet]` attributes.

## Severity
- [ ] SHOULD — should be fixed before merge
