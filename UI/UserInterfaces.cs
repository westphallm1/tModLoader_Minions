using AmuletOfManyMinions.UI.CombatPetsQuizUI;
using AmuletOfManyMinions.UI.Common;
using AmuletOfManyMinions.UI.TacticsUI;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace AmuletOfManyMinions.UI
{
	/// <summary>
	/// Contains all UIs, and manages boilerplate drawing/updating
	/// </summary>
	public class UserInterfaces : ModSystem
	{
		internal static UserInterface tacticsInterface;
		internal static TacticsUIMain tacticsUI;
		internal static BuffRowClickCapture buffClickCapture;
		internal static CombatPetsQuizUIMain quizUI;

		private static GameTime _lastUpdateUiGameTime;

		public override void OnModLoad()
		{
			if (!Main.dedServ)
			{
				tacticsInterface = new UserInterface();

				tacticsUI = new TacticsUIMain();
				buffClickCapture = new BuffRowClickCapture();
				quizUI = new CombatPetsQuizUIMain();
				tacticsUI.Activate();
				buffClickCapture.Activate();
				quizUI.Activate();

				UIState state = new UIState();
				state.Append(quizUI);
				state.Append(buffClickCapture);
				// tacticsUI should take priority over buffClickCapture in the case that
				// they're both active
				state.Append(tacticsUI);

				tacticsInterface.SetState(state);
			}
		}

		public override void Unload()
		{
			tacticsInterface = null;
			tacticsUI = null;
			buffClickCapture = null;
		}

		/// <summary>
		/// Accurate in-UI Mouse position used to spawn UI outside UpdateUI()
		/// </summary>
		public static Vector2 MousePositionUI;

		public override void UpdateUI(GameTime gameTime)
		{
			_lastUpdateUiGameTime = gameTime;
			if (tacticsInterface?.CurrentState != null)
			{
				tacticsInterface.Update(gameTime);
			}

			MousePositionUI = Main.MouseScreen;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (index != -1)
			{
				layers.Insert(index, new LegacyGameInterfaceLayer(
					"AmuletOfManyMinions: Tactics UI",
					delegate
					{
						if (_lastUpdateUiGameTime != null && tacticsInterface?.CurrentState != null)
						{
							tacticsInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
