# Liteson

Liteson is a lightweight JSON serializer and deserializer for .NET (Standard/ Core/ 4.5+) written in C#. This library can be used in other libraries which don't want to add any extra dependencies to json serializers. 

## Using Liteson in your project
The recommended way of integrating Liteson into your project is to include the generated [amalgamated source](#generating-amalgamated-source-file) (single file containing whole code).

## Serialization
```cs
JsonConvert.Serialize(new Data { Item = "test" });
```
## Deserialization
```cs
JsonConvert.Deserialize<Data>("{\"Item\":\"test\"}");
```
## Generating amalgamated source file
In order to generate amalgamated source file you need to:
* Clone this repository
* Compile Liteson.sln
* Run Liteson.Merger
* Copy created amalgamated source (Liteson.merged.cs) file from sln root directory

## Features
* Enum values can be serialized as numeric values or string names
* Custom property names with [JsonProperty] attribute
* camelCase names support
* Runtime property type selection (you need to provide ITypeSelector for this)

## Runtime type selection
Define type selector:
```cs
public class Abstract { }
public class Derived : Abstract { }
public class Data
{
    public Abstract Value { get; set; }
    public string ValueType { get; set; }
}

public class TypeSelector : ITypeSelector
{
    public Type SupportedType => typeof(Abstract);
	public Type FindPropertyType(string property, object parent) => typeof(Derived);
}
```
Register it in serialization settings:
```cs
var settings = new SerializationSettings().AddTypeSelector(new TypeSelector());
var result = JsonConver.Deserialize<Data>("{}", settings);
```
## License
MIT License