﻿












using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Merlin
{
	public static class HarvestableObjectViewExtensions
	{
		static HarvestableObjectViewExtensions()
		{

		}

		public static int GetTier(this HarvestableObjectView instance)
		{
			return instance.HarvestableObject.sj();
		}

		public static string GetResourceType(this HarvestableObjectView instance)
		{
			return instance.HarvestableObject.ss().u;
		}

		public static int GetRareState(this HarvestableObjectView instance)
		{
			return instance.HarvestableObject.sm();
		}

		public static int GetCurrentCharges(this HarvestableObjectView instance)
		{
			return (int)instance.HarvestableObject.si();
		}

		public static long GetMaxCharges(this HarvestableObjectView instance)
		{
			return instance.HarvestableObject.so();
		}

		public static bool IsLootProtected(this HarvestableObjectView instance)
		{
			return !instance.HarvestableObject.sq();
		}

		public static bool CanLoot(this HarvestableObjectView instance, LocalPlayerCharacterView player)
		{
			if (instance.IsLootProtected())
				return false;

			var requiresTool = instance.RequiresTool();
			var tool = instance.GetTool(player);

			if (requiresTool && tool == null)
				return false;

			var toolProxy = a4w.a(tool) as a38;
			var durability = a4w.b(tool.b3(), toolProxy.ba());
			if (requiresTool && durability <= 10)
				return false;

            return true;
		}

		public static arp GetTool(this HarvestableObjectView instance, LocalPlayerCharacterView player)
		{
			return instance.HarvestableObject.az(player.LocalPlayerCharacter, true);
		}

		public static bool RequiresTool(this HarvestableObjectView instance)
		{
			return instance.HarvestableObject.sd().ak();
		}
	}
}