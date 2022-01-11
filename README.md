# Requester
[![NuGet version](https://badge.fury.io/nu/RequesterLib.svg)](https://badge.fury.io/nu/RequesterLib)

Simple library to make request interactions with endpoints easier.

Example usage:
```c#
var requester = new Requester.Builder().WithDefaultHeader("User-Agent", "Test").Build();
var outputTypeInstance = await requester.PostAsJsonAndGetFromJsonAsync<InputType, OutputType>("<URL>", inputTypeInstance);
await requester.PostAsJsonAsync<InputType>("<URL>", inputTypeInstance);
var jsonElement = await requester.GetJsonElementAsync("<URL>");
```
