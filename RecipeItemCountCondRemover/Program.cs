using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System.Diagnostics;

namespace RecipeItemCountCondRemover
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "RecipeItemCountCondRemover.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            foreach (var constructibleObject in state.LoadOrder.PriorityOrder.ConstructibleObject().WinningOverrides())
            {
                if (constructibleObject.Items == null || constructibleObject.Conditions == null)
                {
                    continue;
                }
                var conditions = constructibleObject.Conditions
                    .Where(c => c.Data.Function == Condition.Function.GetItemCount)
                    .ToList();
                if (conditions?.Count() > 0)
                {
                    Console.WriteLine("Patching COBJ " + constructibleObject.ToString());
                    var patchConstructibleObject = state.PatchMod.ConstructibleObjects.GetOrAddAsOverride(constructibleObject);
                    Debug.Assert(patchConstructibleObject.Items != null, "patchConstructibleObject.Items != null");
                    foreach (var c in conditions)
                    {
                        patchConstructibleObject.Conditions.Remove(c.DeepCopy());
                    }
                }
            }
        }
    }
}
