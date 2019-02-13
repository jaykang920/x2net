## 0.18.0 (2019-02-13)

Bugfixes:

- WaitForTask: @barowa fixed the performance issue on Linux

Features:

- AbstractTcpClient: applied exponential backoff to connection retry interval
- AbstractTcpClient: randomized reconnect delay
- Consts: added GetEnumerator() to enumerate key/value pairs

## 0.17.1 (2018-11-28)

Bugfixes:

- AbstractTcpClient: now it doesn't reconnect on active close

## 0.17.0 (2018-11-27)

Features:

- xpiler: now supports wildcard search pattern matching in arguments (not regex)
- introduced builtin event LocalEvent
- ServerLink: added methods GetSession(s)

Bugfixes:

- Scope: fixed possible crash on exit in ThreadPoolFlow

## 0.16.0 (2018-11-15)

Features:

- added NamedPipeClient/Server link cases
- added BlockingCollectionEventQueue contribued by @barowa

## 0.15.0 (2018-10-18)

Bugfixes:

- BlockCipher.InverseTransform: fixed the last ciphertext block copy bug

Features:

- added target framework netcoreapp2.1
- xpiler: now preserves property name cases for cells/events marked as 'asIs'

## 0.14.0 (2018-09-16)

Features:

- xpiler: now preserves property name cases for local cells/events
- Coroutine: added 4-arg WaitForCompletion
- Coroutine: added WaitForTask
- Scope: added support for extending

## 0.13.0 (2018-07-27)

Features:

- Flow: changed interface - replaced Startup/Shutdown with Start/Stop
- Coroutine: renamed class WaitForNothing as WaitForNext
- Removed VerboseSerializer and VerboseDeserializer

## 0.12.1 (2018-07-18)

Features:

- Renamed class Binder as Binding

## 0.12.0 (2018-07-10)

Features:

- Coroutine: renamed WaitForSingle*/WaitForMultiple* as WaitFor*/WaitForAll*, respectively.
- Coroutine: added WaitForAnyEvent/Response feature

## 0.11.2 (2018-07-04)

Features:

- Coroutine: added Status property to indicate coroutine execution status
- Flow: added SlowScope* properties to override global config settings

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
