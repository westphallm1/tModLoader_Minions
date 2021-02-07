using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.Tactics
{
	/// <summary>
	/// Manages things around TargetSelectionTactic
	/// </summary>
	public static class TargetSelectionTacticHandler
	{
		/// <summary>
		/// Represents both the limit and a special value that if an ID of this type is returned, it is treated as an assigned default tactic
		/// </summary>
		public const byte DefaultTacticID = byte.MaxValue;

		internal static int Count { get; private set; }

		public static Type DefaultTacticType => typeof(ClosestEnemyToMinion);

		private static List<TargetSelectionTactic> TacticDatas { get; set; }
		public static Dictionary<Type, byte> TypeToID { get; private set; }
		public static Dictionary<string, byte> NameToID { get; private set; }

		//"static" properties that are best to be cached
		public static List<string> DisplayNames { get; private set; }
		public static List<string> Descriptions { get; private set; }
		public static List<Texture2D> Textures { get; private set; }
		public static List<Texture2D> OutlineTextures { get; private set; }

		public static Mod Mod { get; private set; }

		public static void Load()
		{
			TacticDatas = new List<TargetSelectionTactic>();
			TypeToID = new Dictionary<Type, byte>();
			NameToID = new Dictionary<string, byte>();
			DisplayNames = new List<string>();
			Descriptions = new List<string>();
			Textures = new List<Texture2D>();
			OutlineTextures = new List<Texture2D>();

			RegisterTacticDatas();
		}

		public static void Unload()
		{
			Count = 0;
			TacticDatas = null;
			TypeToID = null;
			NameToID = null;
			DisplayNames = null;
			Descriptions = null;
			Textures = null;
			OutlineTextures = null;
		}

		private static void RegisterTacticDatas()
		{
			Type targetSelectionTacticDataType = typeof(TargetSelectionTactic);
			IEnumerable<Type> tacticDataTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(targetSelectionTacticDataType));

			foreach (var type in tacticDataTypes)
			{
				TargetSelectionTactic tactic = (TargetSelectionTactic)Activator.CreateInstance(type);

				int count = TacticDatas.Count;

				string name = tactic.Name;
				if (NameToID.ContainsKey(name))
				{
					throw new Exception($"A {nameof(TargetSelectionTactic)} of name '{name}' has already been added!");
				}

				if (count >= DefaultTacticID)
				{
					throw new Exception($"Data limit of {DefaultTacticID} reached!");
				}

				TacticDatas.Add(tactic);
				DisplayNames.Add(tactic.DisplayName);
				Descriptions.Add(tactic.Description);
				string texture = tactic.Texture;
				Textures.Add(ModContent.GetTexture(texture));
				OutlineTextures.Add(ModContent.GetTexture(texture + "_Outline"));

				byte id = (byte)count;
				Count++;
				TypeToID[type] = id;
				NameToID[name] = id;
				tactic.ID = id;
			}
		}

		public static string GetDisplayName(byte id)
		{
			return DisplayNames[id];
		}

		public static string GetDescription(byte id)
		{
			return Descriptions[id];
		}

		public static Texture2D GetTexture(byte id)
		{
			return Textures[id];
		}

		public static Texture2D GetOutlineTexture(byte id)
		{
			return OutlineTextures[id];
		}

		/// <summary>
		/// Fetches a tactic given its type.
		/// </summary>
		/// <returns>The tactic</returns>
		public static TargetSelectionTactic GetTactic<T>() where T : TargetSelectionTactic
		{
			return GetTactic(TypeToID[typeof(T)]);
		}

		/// <summary>
		/// Fetches a tactic given its id. Defaults to the default tactic if id is not matching
		/// </summary>
		/// <param name="id">Tactic ID</param>
		/// <returns>The tactic</returns>
		public static TargetSelectionTactic GetTactic(byte id)
		{
			if (id == DefaultTacticID || id >= TacticDatas.Count)
			{
				id = TypeToID[DefaultTacticType];
			}
			return TacticDatas[id];
		}

		/// <summary>
		/// Fetches a tactic given its name. Defaults to the default tactic if name is not matching
		/// </summary>
		/// <param name="name">Tactic name</param>
		/// <returns>The tactic</returns>
		public static TargetSelectionTactic GetTactic(string name)
		{
			byte id = DefaultTacticID;
			if (NameToID.ContainsKey(name))
			{
				id = NameToID[name];
			}
			return GetTactic(id);
		}
	}
}
