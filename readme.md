**THIS IS DEPRECATED IN FAVOR OF ITS REPLACEMENT**

https://github.com/lostinplace/EmpyrionAPITools

I'm leaving it here for posterity

# Empyrion API Message Broker

## FAQ

### Q:  What is this?

The Empyrion mod api is kind of unintuitive and difficult to work with.  In the course of developing a few mods, I developed an understanding of what was available, and how to make it easier to work with.  I developed this library to enable callback-style management of API requests and responses.

### Q:  How do I use it?

The concepts are pretty simple, there are two types of calls, calls with responses that you care about and calls with response you don't care about.

So first of all, we have the concept of an API Command, or `APICmd` which encapsulates the request's CmdId, and the data to be passed along as an argument.  A request for the `PlayerInfo` of a player with the `entityId` 68 would be described as such:

```csharp
var playerInfoCmd = new APICmd(CmdId.Request_Player_Info, new Id(68));

```

once we've described the request, we need to decide what we want to do with the response.  in this case, we just want to write a line to the log, so we implement a function that takes a `PlayerInfo` argument, and writes it to the log:

```csharp
var handler = (PlayerInfo result) => {
  GameAPI.Console_Write($"player info received {result.toString}");
}
```

Now that we have that, we instantiate a new `MessageBroker` to handle our calls for us, and we use it submit the call by providing our command, and the expected type of the result.  In order to do this, we add the references to our mod in the appropriate places.  NOte that in order for the broker to handle incoming messages, you must call it explicitly in the `Game_Event` method of your mod:

```csharp
public class DebugMod : ModInterface {

  static MessageBroker broker;
  
  public void Game_Start(ModGameAPI dediAPI)
  {
     broker = new MessageBroker(dediAPI);
  }
  
  public void Game_Event(CmdId eventId, ushort seqNr, object data)
  {
    broker.HandleMessage(eventId, seqNr, data);
  }

  public void Game_Update()
  {
    var playerInfoCmd = new APICmd(CmdId.Request_Player_Info, new Id(68));
    var handler = (PlayerInfo result) => {
      GameAPI.Console_Write($"player info received {result.toString}");
    }
    broker.ExecuteCommand<PlayerInfo>(playerInfoCmd, handler);
  }  
}
```

The request is submitted to the API, and wthe response is provided back to our handler, logging the data to the games dedicated server log file.

If we're concerned with the `CmdId` of the response, we can get that too. we just submit a handler that takes two arguments, the first will be the `CmdId` of the response:

```csharp
var handler = (CmdId responseType, PlayerInfo result) => {
  if(responseType == CmdId.Event_Error){
    GameAPI.Console_Write($"Error with PlayerInfo request");
  } else {
    GameAPI.Console_Write($"player info received {result.toString}");
  }
}
```

Alternately, a call for `Request_Player_SetCredits(IdCredits)` probably doesn't have an important response if you've done your job right.  You expect that it will be ok, else it will throw an error.  In order to do this, you don't even need to specify a handler:

```csharp
var setCreditsCmd = new APICmd(CmdId.Request_Player_SetCredits, new IdCredits(68,10.0));
broker.ExecuteCommand(setCreditsCmd);
```

### Q:  How does this manage responses to my requests?

In the ModAPI, the `seqNr` that is submitted with your request is presented back as a response.  The MessageBroker tracks the `seqNr` of every managed request, and asserts that an incoming event is a response to the request.  If you're using the same `seqNr` for other purposes there will be collisions that could be problematic, but as it stands the MessageBroker uses a pool of numbers between 1000 and 64000, so there's a lot of room in the pool.

### Q:  What does it do with events that aren't responses to my requests?

Unless there is a SeqNr collison, it will do nothing with them.  This may change with later versions.

Any event like `Event_Statistics` that is generated as a result of a game event and not a request has a `seqNr` of 0, so the broker doesn't touch it.  Once again, I may change that in the future.

### Q:  How do I include this in my project?

That's up to you, but I use the MSBuild.ILMerge Task available at https://www.nuget.org/packages/MSBuild.ILMerge.Task/

To use that, just follow the instructions from the package above to install it. Then import the dll found in the `bin/Release` folder of this repo, as a project reference to your mod project and make sure that copy local is set to true in the reference properties.  You can always build from source as well.

### Q:  How do you feel about pull requests?

My fondest dreams are of pull requests.  If you want to contribute, awesome.  Just submit the PR in an issue so I can track changes.
