using System;
using System.Collections.Generic;
using StardewModdingAPI;
using System.Threading;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Internal;
using RidgesideVillage;

namespace AtraCore.Framework.ItemResolvers;

// Taken with permission from AtraCore. Thank you, Atra <3
//https://github.com/atravita-mods/StardewMods

internal class ChooseKQuery
{
    private static IModHelper Helper;
    private static IMonitor Monitor;
    internal static void Initialize(IMod ModInstance)
    {
        Helper = ModInstance.Helper;
        Monitor = ModInstance.Monitor;

        ItemQueryResolver.Register("rsv_choose_k", ChooseKQuery.ChooseK);
    }


    /// <summary>
    /// Of the n items given, choose k with equal changes.
    /// </summary>
    /// <inheritdoc cref="ResolveItemQueryDelegate"/>
    internal static IEnumerable<ItemQueryResult> ChooseK(string key, string? arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string>? avoidItemIds, Action<string, string> logError)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "arguments should not be null or whitespace");
            yield break;
        }

        string[] args = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (!ArgUtility.TryGetInt(args, 0, out int count, out string error))
        {
            ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error);
            yield break;
        }

        var candidates = new ArraySegment<string>(args, 1, args.Length - 1);

        HashSet<string>? prev = avoidRepeat ? new() : null;

        if (args.Length - 1 <= count)
        {
            foreach (string candidate in candidates)
            {
                if (avoidItemIds?.Contains(candidate) == true || prev?.Add(candidate) == true)
                {
                    continue;
                }

                if (ItemRegistry.Create(candidate, allowNull: true) is { } item)
                {
                    yield return new(item);
                }
                else
                {
                    Log.Trace($"{candidate} does not correspond to a valid item.");
                }
            }
        }

        int idx = candidates.Count - 1;
        int final = idx - count;

        Random random = context.Random ?? Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode("choose_k"));

        while (idx > final)
        {
            int j = random.Next(idx + 1);
            string candidate = candidates[j];

            if (avoidItemIds?.Contains(candidate) != true && prev?.Add(candidate) != true)
            {
                if (ItemRegistry.Create(candidate, allowNull: true) is { } item)
                {
                    yield return new(item);
                }
                else
                {
                    Log.Trace($"{candidate} does not correspond to a valid item.");
                }
            }

            if (j != idx)
            {
                (candidates[j], candidates[idx]) = (candidates[idx], candidates[j]);
            }

            idx--;
        }
    }
}
