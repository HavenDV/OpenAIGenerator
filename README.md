# OpenAIGenerator
Source generator, which allows you to add prompts from which code will be generated in deterministic mode

## Usage
1. Install package from nuget - https://www.nuget.org/packages/OpenAIGenerator/
2. Add API key to your project file:
```xml
  <PropertyGroup>
    <OpenAIGenerator_ApiKey>$(API_KEY)</OpenAIGenerator_ApiKey>
  </PropertyGroup>
```
3. Add .prompt files to your project like this:
```
Write simple calculator class in C#. It should have 2 methods: Add and Multiply.
Both methods should take 2 arguments and return result.
You can use only + and * operators.
You can't use any other methods or classes.
```

## Settings
```xml
  <PropertyGroup>
    <OpenAIGenerator_SystemMessage>You are a programmer.</OpenAIGenerator_SystemMessage> <!-- Default: empty -->
    <OpenAIGenerator_Temperature>0.9</OpenAIGenerator_Temperature> <!-- Default: 0.0 -->
    <OpenAIGenerator_Model>gpt-4</OpenAIGenerator_Model> <!-- Default: gpt-3.5-turbo -->
  </PropertyGroup>
```