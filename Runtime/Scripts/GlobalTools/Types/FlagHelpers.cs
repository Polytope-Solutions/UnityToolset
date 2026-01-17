using System;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
    public static class EnumFlags {
        public static bool HasAny<TFlag>(this TFlag valueRaw, TFlag flagsRaw)
            where TFlag : Enum {
            ulong value = Convert.ToUInt64(valueRaw);
            ulong flags = Convert.ToUInt64(flagsRaw);
            return (value & flags) != 0;
        }

        public static bool HasAll<TFlag>(this TFlag valueRaw, TFlag flagsRaw)
            where TFlag : Enum {
            ulong value = Convert.ToUInt64(valueRaw);
            ulong flags = Convert.ToUInt64(flagsRaw);
            return (value & flags) == flags;
        }

        public static TFlag Set<TFlag>(this TFlag valueRaw, TFlag flagsRaw)
            where TFlag : Enum {
            ulong value = Convert.ToUInt64(valueRaw);
            ulong flags = Convert.ToUInt64(flagsRaw);
            return (TFlag)Enum.ToObject(typeof(TFlag), value | flags);
        }

        public static TFlag Clear<TFlag>(this TFlag valueRaw, TFlag flagsRaw)
            where TFlag : Enum {
            ulong value = Convert.ToUInt64(valueRaw);
            ulong flags = Convert.ToUInt64(flagsRaw);
            return (TFlag)Enum.ToObject(typeof(TFlag), value & ~flags);
        }
    }
}