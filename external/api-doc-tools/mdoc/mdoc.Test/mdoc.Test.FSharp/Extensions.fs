module Extensions

module MyModule1 =

    // Define a type.
    type MyClass() =
      member this.F() = 100

    // Define type extension.
    type MyClass with
       member this.G() = 200

module MyModule2 =
   let function1 (obj1: MyModule1.MyClass) =
      // Call an ordinary method.
      printfn "%d" (obj1.F())
      // Call the extension method.
      printfn "%d" (obj1.G())

// Define a new member method FromString on the type Int32.
type System.Int32 with
    member this.FromString( s : string ) =
       System.Int32.Parse(s)

let testFromString str =
    let mutable i = 0
    // Use the extension method.
    i <- i.FromString(str)
    printfn "%d" i

testFromString "500"