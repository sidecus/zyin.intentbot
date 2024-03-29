# Build simple bots in a simple way
zyin.intentbot is an Asp.Net Core 2 extension library for Botframework V4 to help you build "simple" bots in a simple way.

A "simple bot" can be any bot which:
- Takes a user intent and gives back an answer, or takes an action for the user
- Takes an intent, asks uesr for more inputs, and then takes an action
- Can require authentication as needed
- Falls back to certain actions if the intent is unknown

Building these with botframework requires quite some scaffolding and good understanding about BotFramework itself before you can even start. For example, you'll have to write custom dialogs for your main flow; you'll likely have to deal with intent handling and intent fallback with lots of if/else branches; you'll have to use WaterfallSteps for each user input collection scenarios.

That's not that simple. We want people to focus on their business logic isntead of botframework internals so that they can build their scenarios faster.

I started writing some code to reduce repeated work for myself when creating simple bots. Now I think it might be helpful to others as well.

# Is it really simple?
Let's see. As a prerequisite, you need to implement IIntentService which does the intent parsing. This is to map user input to known intent name string. You can use LUIS with built-in ```LuisService```, or create your own intent service.
## Identify an intent and take an action
1. Define a custom intent context. Override the IntentName getter.
```
  public class GreetingsContext : IntentContext
  {
    public override string IntentName => SampleIntentService.Intent_Greetings;
  }
```
2. Define a custom handler for the intent which does the real work, and override ```ProcessIntentInternalAsync```.
```
  public class GreetingsIntentHandler : IntentHandler<GreetingsContext>
```
3. Register the intent into DI
```
  services.AddSimpleIntent<GreetingsContext, GreetingsIntentHandler>();
```
**That's it**.
Example: [GreetingsIntent.cs](https://github.com/sidecus/zyin.intentbot/blob/master/sample/Areas/GreetingsIntent.cs)

## Need user input before taking the action?
Sure. Just define required user input as properties in your IntentContext class, and annotate with ```PromptPropertyAttribute```.
```
[PromptProperty(prompt:"First value (1 to 10)?", promptProvider:typeof(SampleNumberPromptProvider), order: 0)]
public int? First { get; set; }

[PromptProperty("Second value (any int32)?", order: 1)]
public int? Second { get; set; }
```
After defining the IntentContext, register it as an **user input intent**:
```
  services.AddUserInputIntent<AddNumberIntentContext, AddNumberIntentHandler>();
```
Zyin.IntentBot intent factory will automatically handle the logic for you to ask user for inputs, validate the value, and invoke your intent handler when all info are there.
Example: [AddNumberIntent.cs](https://github.com/sidecus/Zyin.IntentBot/blob/master/sample/Areas/AddNumberIntent.cs)

You can also implement your own property validation by extending ```TypedPromptProvider```. Example:[SampleNumberPromptProvider.cs](https://github.com/sidecus/zyin.intentbot/blob/master/sample/Areas/SampleNumberPromptProvider.cs)

## Need to handle unknown intents (catch all scenarios)
This can be done easily. Just implement and register a handler with built in ```FallbackContext```.
```
  services.AddSimpleIntent<FallbackContext, SampleFallbackHandler>();
```

## My intent requires user authentication
If you are using Azure Bot Servie, good news, it's also simple. Just inherit your intent context from ```AuthIntentContext``` instead of ```IntentContext```. Now your intent will trigger the OAuth flow. Note, for this to work you'll need to specify bot secrets, your AAD app secrets, as well as  ```OAuthConnectionName``` in the appSettings.json (or via user secrets). Example: [WhoAmIIntent.cs](https://github.com/sidecus/zyin.intentbot/blob/master/sample/Areas/WhoAmIIntent.cs)

Check the sample project for more details, especially Startup.cs and files under /sample/Areas.

## Other helper classes
There are quite a few helper classes to make your coding even more efficient. This includes:
- ```ImageIntentResponseIntentHandler``` to handle simple responses with image
- ```JsonCardResponseIntentHandler``` to handle simple response with static JSON based adaptive card
- ```JsonAdaptiveCard``` to help produce AdaptiveCard responses from JSON file (with naive formatting capability)
- ```OBOTokenService``` to exchange AAD token with different services
- ```AADv2HttpService``` to make it easier to create new AAD Bearer token based RESTful service clients
- and much more

# Enjoy and Happy Coding!

