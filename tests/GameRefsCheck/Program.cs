// Verifies every game member MdsBuildTools binds by reflection exists in a given Assembly-CSharp.dll.
// Usage: dotnet run --project tests/GameRefsCheck -- path/to/Assembly-CSharp.dll
using Mono.Cecil;

var path = args.Length > 0 ? args[0] : "decompiled/Assembly-CSharp.dll";
var module = ModuleDefinition.ReadModule(path);
TypeDefinition? T(string n) => module.GetType(n);

var checks = new (string what, Func<bool> ok)[]
{
    ("Placing.PlacingObjects", () => T("Placing.PlacingObjects") != null),
    ("MoveObject()", () => T("Placing.PlacingObjects")!.Methods.Any(m => m.Name == "MoveObject" && m.Parameters.Count == 0)),
    ("RotateNormalObject(bool,float)", () => T("Placing.PlacingObjects")!.Methods.Any(m => m.Name == "RotateNormalObject"
        && m.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(new[] { "System.Boolean", "System.Single" })
        && m.Parameters[0].Name == "preciseRotation" && m.Parameters[1].Name == "delta")),
    ("_hoveringObject : GameObject", () => Field("_hoveringObject", "UnityEngine.GameObject")),
    ("defaultParent : Transform", () => Field("defaultParent", "UnityEngine.Transform")),
    ("floorMask : LayerMask", () => Field("floorMask", "UnityEngine.LayerMask")),
    ("_currentPiece : FurniturePiece", () => Field("_currentPiece", "Furniture.FurniturePiece")),
    ("SpawnedFurniture : List<GameObject>", () => T("Placing.PlacingObjects")!.Properties.Any(p => p.Name == "SpawnedFurniture")),
    ("FurniturePiece.CurrentHoveringLayer : int", () => T("Furniture.FurniturePiece")!.Properties.Any(p => p.Name == "CurrentHoveringLayer" && p.PropertyType.FullName == "System.Int32")),
    ("GlobalPlacingRules.GlobalFreePlacement", () => T("Placing.GlobalPlacingRules")!.Properties.Any(p => p.Name == "GlobalFreePlacement")),
};

bool Field(string name, string type) => T("Placing.PlacingObjects")!.Fields.Any(f => f.Name == name && f.FieldType.FullName == type);

var failed = 0;
foreach (var (what, ok) in checks)
{
    bool pass; try { pass = ok(); } catch { pass = false; }
    Console.WriteLine($"{(pass ? "OK  " : "FAIL")} {what}");
    if (!pass) failed++;
}
return failed;
