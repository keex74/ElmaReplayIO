This repository contains C# tools to read Elasto Mania replay files.

The code has been converted from the Rust implementation by Hexjelly
found at:
[https://github.com/elmadev/elma-rust](https://github.com/elmadev/elma-rust)

The Rust implementation is published under MIT license, Copyright (c) 2016 Hexjelly.

Basic Usage:
To parse replay data from an input data stream (e.g. a FileStream):
```csharp
var replay = ElmaReplayIO.Replay.ParseFrom(stream);
```

A replay is a collection of one or multiple Rides. The first ride can be accessed via:
```csharp
var mainRide = replay.MainRide;
```

Other rides can be accessed by index via
```csharp
var secondRide = replay[1];
```
