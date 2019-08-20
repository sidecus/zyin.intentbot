# Motivation Simple bot made simple
Zyin.IntentBot is a Botframework V4 extension library to help you build "simple" in a simple way.

In my mind a simple bot can be any bot which:
- Takes a user intent and gives back an answer, or takes an action for the user
- Takes an intent, asks uesr for more inputs, and then takes an action
- Can require authentication as needed
- Can fall back to certain actions if the intent is unknown

This is definitely doable via Botframework V4, however, it requires a lot of scaffolding and some good understanding about BotFramework before you can even start. For example, you'll have to write custom dialogs for your main flow; you'll likely have to deal with intent handling and intent fallback with lots of if/else branches; you'll have to use WaterfallSteps for user input collection.

That's **not simple**.

I started writing some code to help reduce repeated work for myself. Later I realize it might be helpful to others as well.

# Show me how simple it is
## Implement an IIntentService which does the intent parsing.
This is to map user input to intent. You can use built in ```LuisService``` if you want.
## Identify an intent and take an action
1. Define a custom intent context. Override the IntentName getter.
```
  public class GreetingsContext : IntentContext
  {
    public override string IntentName => SampleIntentService.Intent_Greetings;
  }
```
2. Define a custom handler for the intent which does the real work
```
  public class GreetingsIntentHandler : IntentHandler<GreetingsContext>
```
3. Register the intent into DI
```
  services.AddSimpleIntent<GreetingsContext, GreetingsIntentHandler>();
```
**That's it**.
Example: [GreetingsIntent.cs](https://github.com/sidecus/Zyin.IntentBot/blob/master/sample/Areas/GreetingsIntent.cs)

## Need user input before taking the action?
Sure. Just define your required user input as properties in your IntentContext class, and use ```PromptProperty``` attribute to annonate them. Next you need to register it as an user input intent:
```
  services.AddUserInputIntent<AddNumberIntentContext, AddNumberIntentHandler>();
```
Zyin.IntentBot intent factory will automatically handle the logic for you to ask user for inputs, validation, and invoke your intent handler when all info are there.
Example: [AddNumberIntent.cs](https://github.com/sidecus/Zyin.IntentBot/blob/master/sample/Areas/AddNumberIntent.cs)

You can also implement your own property validation via ```TypedPromptProvider```. [Example](https://github.com/sidecus/Zyin.IntentBot/blob/master/sample/Areas/SampleNumberPromptProvider.cs)

## Need to handle unknown intents (catch all scenarios)
Just implement and register a handler with built in ```FallbackContext```.
```
  services.AddSimpleIntent<FallbackContext, SampleFallbackHandler>();
```

## One of my intent requires user authentication
OK. If you are using Azure Bot Servie OAuthCard - simply inherit your intent context from ```AuthIntentContext``` instead of ```IntentContext```.

I hope you enjoy.

