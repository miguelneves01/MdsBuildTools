using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace MdsBuildTools.Patches
{
    /// <summary>
    /// Reflection handles into the game's Assembly-CSharp (found by decompiling MDS).
    /// We bind by name at runtime instead of compiling against the game DLL, so the repo
    /// never contains game code and CI can build without the game installed.
    /// If the game renames something, <see cref="Resolve"/> reports exactly what is missing.
    /// A few reflective reads per frame are negligible next to the game's own raycasts.
    /// </summary>
    internal static class GameRefs
    {
        public const string PlacingObjectsType = "Placing.PlacingObjects";
        public const string FurniturePieceType = "Furniture.FurniturePiece";
        public const string GlobalPlacingRulesType = "Placing.GlobalPlacingRules";

        public static Type? PlacingObjects { get; private set; }
        public static MethodInfo? MoveObject { get; private set; }
        public static MethodInfo? RotateNormalObject { get; private set; }

        private static FieldInfo? _hoveringObject;
        private static FieldInfo? _defaultParent;
        private static FieldInfo? _floorMask;
        private static FieldInfo? _currentPiece;
        private static MethodInfo? _spawnedFurniture;
        private static PropertyInfo? _currentHoveringLayer;
        private static PropertyInfo? _globalFreePlacement;

        /// <summary>Binds everything; returns the members that could not be found (empty = OK).</summary>
        public static List<string> Resolve()
        {
            var missing = new List<string>();

            PlacingObjects = AccessTools.TypeByName(PlacingObjectsType);
            if (PlacingObjects == null)
            {
                missing.Add(PlacingObjectsType);
                return missing;
            }

            MoveObject = Require(AccessTools.Method(PlacingObjects, "MoveObject", Type.EmptyTypes), "PlacingObjects.MoveObject()", missing);
            RotateNormalObject = Require(AccessTools.Method(PlacingObjects, "RotateNormalObject", new[] { typeof(bool), typeof(float) }),
                "PlacingObjects.RotateNormalObject(bool,float)", missing);

            _hoveringObject = Require(AccessTools.Field(PlacingObjects, "_hoveringObject"), "PlacingObjects._hoveringObject", missing);
            _defaultParent = Require(AccessTools.Field(PlacingObjects, "defaultParent"), "PlacingObjects.defaultParent", missing);
            _floorMask = Require(AccessTools.Field(PlacingObjects, "floorMask"), "PlacingObjects.floorMask", missing);
            _currentPiece = Require(AccessTools.Field(PlacingObjects, "_currentPiece"), "PlacingObjects._currentPiece", missing);
            _spawnedFurniture = Require(AccessTools.PropertyGetter(PlacingObjects, "SpawnedFurniture"), "PlacingObjects.SpawnedFurniture", missing);

            var piece = AccessTools.TypeByName(FurniturePieceType);
            _currentHoveringLayer = Require(piece == null ? null : AccessTools.Property(piece, "CurrentHoveringLayer"),
                "FurniturePiece.CurrentHoveringLayer", missing);

            var rules = AccessTools.TypeByName(GlobalPlacingRulesType);
            _globalFreePlacement = Require(rules == null ? null : AccessTools.Property(rules, "GlobalFreePlacement"),
                "GlobalPlacingRules.GlobalFreePlacement", missing);

            return missing;
        }

        public static GameObject? HoveringObject(object placing) => _hoveringObject?.GetValue(placing) as GameObject;
        public static Transform? DefaultParent(object placing) => _defaultParent?.GetValue(placing) as Transform;
        public static LayerMask FloorMask(object placing) => _floorMask == null ? default : (LayerMask)_floorMask.GetValue(placing);
        public static List<GameObject> SpawnedFurniture(object placing) =>
            _spawnedFurniture?.Invoke(placing, null) as List<GameObject> ?? new List<GameObject>();

        public static int HoveringLayer(object placing)
        {
            var piece = _currentPiece?.GetValue(placing);
            return piece == null || _currentHoveringLayer == null ? -1 : (int)_currentHoveringLayer.GetValue(piece);
        }

        public static bool IsFreePlacement => _globalFreePlacement != null && (bool)_globalFreePlacement.GetValue(null);

        private static T? Require<T>(T? member, string name, List<string> missing) where T : class
        {
            if (member == null) missing.Add(name);
            return member;
        }
    }
}
