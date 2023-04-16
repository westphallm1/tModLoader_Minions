using AmuletOfManyMinions.Core.Minions.Tactics.TacticsGroups;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace AmuletOfManyMinions.Core.Minions.Tactics
{
	/// <summary>
	/// Manages things around TargetSelectionTactic
	/// </summary>
	public class TargetSelectionTacticHandler : ModSystem
	{
		/// <summary>
		/// Represents both the limit and a special value that if an ID of this type is returned, it is treated as an assigned default tactic
		/// </summary>
		public const byte DefaultTacticID = byte.MaxValue;

		internal static int Count { get; private set; }

		public static Type DefaultTacticType => typeof(ClosestEnemyToMinion);

		public static List<byte> OrderedIds { get; private set; }
		private static List<TargetSelectionTactic> TacticDatas { get; set; }

		internal static List<TacticsGroup> TacticsGroups { get; private set; }
		internal static LocalizedText TacticsGroupDescription { get; private set; }
		internal static List<LocalizedText> TacticsGroupNames { get; private set; }
		internal static LocalizedText CloseMinionTacticsText { get; private set; }
		internal static LocalizedText OpenMinionTacticsText { get; private set; }

		public static Dictionary<Type, byte> TypeToID { get; private set; }
		public static Dictionary<string, byte> NameToID { get; private set; }

		//"static" properties that are best to be cached
		public static List<LocalizedText> DisplayNames { get; private set; }
		public static List<LocalizedText> Descriptions { get; private set; }
		public static List<Asset<Texture2D>> Textures { get; private set; }
		public static List<Asset<Texture2D>> OutlineTextures { get; private set; }
		public static List<Asset<Texture2D>> SmallTextures { get; private set; }

		public static List<Asset<Texture2D>> GroupTextures { get; private set; }
		public static List<Asset<Texture2D>> GroupOutlineTextures { get; private set; }
		public static List<Asset<Texture2D>> GroupOverlayTextures { get; private set; }

		public override void OnModLoad()
		{
			TacticDatas = new List<TargetSelectionTactic>();
			TacticsGroups = new List<TacticsGroup>();
			TacticsGroupNames = new List<LocalizedText>();
			TypeToID = new Dictionary<Type, byte>();
			NameToID = new Dictionary<string, byte>();
			DisplayNames = new List<LocalizedText>();
			Descriptions = new List<LocalizedText>();
			Textures = new List<Asset<Texture2D>>();
			OutlineTextures = new List<Asset<Texture2D>>();
			SmallTextures = new List<Asset<Texture2D>>();
			GroupTextures = new List<Asset<Texture2D>>();
			GroupOutlineTextures = new List<Asset<Texture2D>>();
			GroupOverlayTextures = new List<Asset<Texture2D>>();
			OrderedIds = new List<byte>();

			RegisterTacticDatas();
			RegisterTacticsGroups();
		}

		public override void OnModUnload()
		{
			Count = 0;
			TacticDatas = null;
			TacticsGroups = null;
			TacticsGroupNames = null;
			TypeToID = null;
			NameToID = null;
			DisplayNames = null;
			Descriptions = null;
			Textures = null;
			OutlineTextures = null;
			SmallTextures = null;
			GroupTextures = null;
			GroupOutlineTextures = null;
			GroupOverlayTextures = null;
			OrderedIds = null;
		}

		private static void RegisterTacticDatas()
		{
			Type targetSelectionTacticDataType = typeof(TargetSelectionTactic);
			IEnumerable<Type> tacticDataTypes =
				AssemblyManager.GetLoadableTypes(ModContent.GetInstance<AmuletOfManyMinions>().Code)
				.Where(t => !t.IsAbstract && t.IsSubclassOf(targetSelectionTacticDataType));

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
				var immediate = AssetRequestMode.ImmediateLoad; //Directly used in UI to set dimensions
				Textures.Add(ModContent.Request<Texture2D>(texture, immediate));
				OutlineTextures.Add(ModContent.Request<Texture2D>(texture + "_Outline", immediate));
				SmallTextures.Add(ModContent.Request<Texture2D>(texture + "_Small", immediate));

				byte id = (byte)count;
				Count++;
				TypeToID[type] = id;
				NameToID[name] = id;
				tactic.ID = id;
			}

			OrderedIds = new List<byte>
			{
				//First row
				GetTactic<ClosestEnemyToMinion>().ID,
				GetTactic<StrongestEnemy>().ID,
				GetTactic<LeastDamagedEnemy>().ID,
				GetTactic<SpreadOut>().ID,

				//Second row
				GetTactic<ClosestEnemyToPlayer>().ID,
				GetTactic<WeakestEnemy>().ID,
				GetTactic<MostDamagedEnemy>().ID,
				GetTactic<AttackGroups>().ID,
			};
		}

		private void RegisterTacticsGroups()
		{
			TacticsGroupDescription = Language.GetOrRegister(Mod.GetLocalizationKey($"TacticsGroups.Description"));
			CloseMinionTacticsText = Language.GetOrRegister(Mod.GetLocalizationKey($"TacticsGroups.Close"));
			OpenMinionTacticsText = Language.GetOrRegister(Mod.GetLocalizationKey($"TacticsGroups.Open"));
			var immediate = AssetRequestMode.ImmediateLoad;
			for (int i = 0; i < MinionTacticsPlayer.TACTICS_GROUPS_COUNT; i++)
			{
				TacticsGroup group = new TacticsGroup(i);
				TacticsGroups.Add(group);
				TacticsGroupNames.Add(Language.GetOrRegister(Mod.GetLocalizationKey($"TacticsGroups.{group.NameKey}")));
				GroupTextures.Add(ModContent.Request<Texture2D>(group.Texture, immediate));
				GroupOutlineTextures.Add(ModContent.Request<Texture2D>(group.Texture + "_Outline", immediate));
				GroupOverlayTextures.Add(ModContent.Request<Texture2D>(group.Texture + "_Overlay", immediate));
			}
		}

		public override void SetStaticDefaults()
		{
			////Need to "refresh" lang stuff here
			//foreach (var item in TacticsGroupNames)
			//{
			//	_ = item;
			//}
			//_ = TacticsGroupDescription;
			//_ = CloseMinionTacticsText;
			//_ = OpenMinionTacticsText;
			//foreach (var item in DisplayNames)
			//{
			//	_ = item;
			//}
			//foreach (var item in Descriptions)
			//{
			//	_ = item;
			//}
		}

		public static LocalizedText GetDisplayName(byte id)
		{
			return DisplayNames[id];
		}

		public static LocalizedText GetDescription(byte id)
		{
			return Descriptions[id];
		}

		public static Asset<Texture2D> GetTexture(byte id)
		{
			return Textures[id];
		}

		public static Asset<Texture2D> GetOutlineTexture(byte id)
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
