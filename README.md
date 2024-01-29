# Nua

A simple scripting language similar to Lua

## Usage

```csharp
// create a nua runtime
var runtime = new NuaRuntime();

// evaluate an expression
var result = runtime.Evaluate("114000 + 514")
```

## Syntaxes

```txt
# some number
v1 = 114514

# some boolean
v2 = true

# some string
v3 = "string value"

# some table (key value pairs)
v4 = {
  "a": 123,
  "b": 456,

  # when the key name is a string, 
  c: 789
}

# some list
v5 = [
  123,
  true,
  "string item",
  { "key1": "value1" },
  [ "inner list item", "item2" ]
]

if v1 > 10 {
  print("value 1 is greater than 10")
}

if v2 {
  print("value 2 is true")
} else {
  print("value 2 is false")
}

if len(v4) > 3 {
  print("table length is greater than 3")
} elif len(v4) > 1 {
  print("table length is greater than 3")
} else {
  print("table has no element")
}

# print value from 1 to 10
print("list elements")
for v of 1, 10 {
  print(v)
}

# enumerate all key-value pairs in table
print("key-value pairs")
for key, value in v4 {
  print(key)
  print(value)
}
```
