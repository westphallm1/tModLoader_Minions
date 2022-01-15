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
	public static class UserInterfaces
	{
		internal static UserInterface tacticsInterface;
		internal static TacticsUIMain tacticsUI;
		internal static BuffRowClickCapture buffClickCapture;
		internal static CombatPetsQuizUIMain quizUI;

		private static GameTime _lastUpdateUiGameTime;

		public static void Load()
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
				state.Append(tacticsUI);
				state.Append(buffClickCapture);

				tacticsInterface.SetState(state);
			}
		}

		public static void Unload()
		{
			tacticsInterface = null;
			tacticsUI = null;
			buffClickCapture = null;
		}

		public static void UpdateUI(GameTime gameTime)
		{
			_lastUpdateUiGameTime = gameTime;
			if (tacticsInterface?.CurrentState != null)
			{
				tacticsInterface.Update(gameTime);
			}
		}

		public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
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
