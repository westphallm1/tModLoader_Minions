using AmuletOfManyMinions.Core.Minions;
using AmuletOfManyMinions.Core.Minions.Tactics;
using AmuletOfManyMinions.Core.Minions.Tactics.TargetSelectionTactics;
using AmuletOfManyMinions.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.UI;
using Terraria.Audio;

namespace AmuletOfManyMinions.UI.TacticsUI
{
	//Because it's a UIPanel, it automatically draws the default blue background
	internal class TacticsPanel : UIElement
	{
		private readonly List<TacticButton> buttons;

		private bool gotTacticFromPlayer = false; //Safety check to prevent index out of bounds
		private int selectedIndex = 0; //From the buttons list

		/// <summary>
		/// Currently selected tactic. Can be null if world isn't loaded in yet or player hasn't supplied any info
		/// </summary>
		internal TargetSelectionTactic SelectedTactic => gotTacticFromPlayer ? buttons[selectedIndex].Tactic : null;

		internal TacticsPanel(List<TacticButton> buttons)
		{
			this.buttons = buttons;
		}

		public override void OnInitialize()
		{
			foreach (var button in buttons)
			{
				button.OnClick += Button_OnClick;
				//Padding of the panel screws up alignment for the buttons, so revert it
				button.Top.Pixels -= this.PaddingTop;
				button.Left.Pixels -= this.PaddingLeft;
				Append(button);
			}
		}

		private void Button_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if (listeningElement is TacticButton clickedButton)
			{
				if (selectedIndex != clickedButton.index)
				{
					selectedIndex = clickedButton.index;
					SoundEngine.PlaySound(SoundID.MenuTick);
				}

				//Recalculate selected status for each button, and set players tactic
				foreach (var button in buttons)
				{
					bool selected = selectedIndex == button.index;
					button.SetSelected(selected);

					if (selected)
					{
						Main.LocalPlayer.GetModPlayer<MinionTacticsPlayer>().SetTactic(button.ID);
					}
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (ContainsPoint(Main.MouseScreen))
			{
				//Prevents drawing item textures on mouseover (bed, selected tool etc.)
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.cursorItemIconEnabled = false;
				Main.ItemIconCacheUpdate(0);
			}
			base.DrawSelf(spriteBatch);
			Color color = Color.White * 0.85f;
			Vector2 bgDrawPos = new Vector2(GetDimensions().X, GetDimensions().Y);
			spriteBatch.Draw(UserInterfaces.tacticsUI.bgLargeTexture.Value, bgDrawPos, null, color, 0f, Vector2.Zero, 1, 0f, 0);
		}

		/// <summary>
		/// This gets called once when the player enters the world to initialize the UI with values
		/// </summary>
		/// <param name="id">Tactic ID</param>
		internal void SetSelected(int id)
		{
			gotTacticFromPlayer = true;
			foreach (var button in buttons)
			{
				bool selected = button.ID == id;
				button.SetSelected(selected);
			}
		}
	}
}
