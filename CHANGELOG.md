Performance:

- FrameBasedFlow: changed the default time resolution to 15.625ms (64 frame/sec)
- BlockCiper: introduced SettingsBuilder to reuse RSACryptoServiceProvider instance
- TimeFlow.Timer: changed simple lock to ReaderWriterLockSlim

## 0.11.1 (2018-05-28)

Features:

- changed flow name of the default TimeFlow
- added some log messages for startup/shutdown

## 0.11.0 (2018-05-11)

Features:

- added full support for generic list and map types (with C# 4.0 dynamic keyword)
- added full serialization support for embedded events

## 0.10.1 (2018-04-05)

Features:

- Transforms: added SimpleCipher class
- added udp link samples

## 0.10.0 (2018-03-15)

Features:

- introduced link strategies to encapsulate pluggable functionalities
- SessionBasedLink: removed session recovery support
- added helloworld samples
- *TcpClient: removed synchronous connect interface

## 0.9.19 (2018-01-26)

Features:

*TcpClient: added synchronous connect interface

## 0.9.18 (2018-01-26)

Bugfixes:

- ConcurrentEventQueue: fixed the bug that the final event is not properly
  dequeued before closing

## 0.9.17 (2018-01-26)

Features:

- AbstractTcpClient: added Connect(string) interface
- removed AddressUtil

## 0.9.16 (2018-01-10)

Features:

- added support for link-local event factories
- added ThreadPoolFlow which works with .NET managed thread pool
- Scope: now supports setting event after instantiation

## 0.9.15 (2018-01-05)

Features:

- EventSink: Bind/Unbind methods now returns the binder token

## 0.9.14 (2017-12-19)

Features:

- added serialization support for more list types

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
