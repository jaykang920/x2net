## 0.9.12 (2017-09-08)

Features:

- Links/Socket: added AddressUtil class to support network address-related tasks

BugFixes:

- Buffer: fixed the bug that the current block is not updated properly after
trimming exactly on buffer block boundaries

## 0.9.11 (2017-08-22)

Features:

- Case: added OnStart/OnStop handlers to be invoked on flow start/stop

## 0.9.10 (2017-08-10)

Bugfixes:

- LinkSession: fixed exception in SendInternal for a disposed session
- *TcpClient: fixed exception in TeardownInternal

## 0.9.9 (2017-07-24)

Features:

- Transforms/BlockCipher: added configurable settings

BugFixes:

- AbstractTcpClient: fixed crash on close ConnectAndSend calls
- AbstractTcpClient/Server: fixed internal setup sequence

## 0.9.8 (2017-07-06)

Features:

- Hub: added Insert() method to support hub case insertion

## 0.9.7 (2017-07-04)

Features:

- xpiler handles XML comments again

Changes:

- changed the internal logger class name from Log to Trace

## 0.9.6 (2017-06-27)

Fixed NuGet packages for .NET Core

## 0.9.5 (2017-06-26)

NuGet packages now support .NET Core 2.0

Features:

- Hub: added Hub.Case class to support application level initialization/cleanup

## 0.9.4 (2017-06-21)

Features:

- Hub: added GetFlows() methods to retrieve an array of attached flows by name or type

BugFixes:

- @barowa fixed the TCP socket link packet framing bug
