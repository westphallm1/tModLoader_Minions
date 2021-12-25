using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static AmuletOfManyMinions.UI.CombatPetsQuizUI.CombatPetsQuizUIMain;

namespace AmuletOfManyMinions.UI.CombatPetsQuizUI
{

	/// <summary>
	/// Dialog panel that displays under the player while they're taking the
	/// Elemental Pals Personality Quiz
	/// </summary>
	class CombatPetsQuizUIMain : UIElement
	{
		public static readonly float MaxTextboxWidth = 600f;
		public static readonly float MinTextboxWidth = 120f;
		// answer panel height set dynamically
		public static readonly float MarginSize = 8f;
		public static float LineHeight { get; private set; }

		public static float QuestionPanelHeight => 3 * LineHeight + 2 * MarginSize;
		public static DynamicSpriteFont TextFont => FontAssets.MouseText.Value;

		private CombatPetsQuizUIPanel questionPanel;
		private CombatPetsQuizUIPanel answerPanel;

		// Todo populate dynamically
		private List<string> questionLines;
		private List<string> answerLines;

		// TODO ModPlayer to store game state for this
		public bool IsActive => false;
		public override void OnInitialize()
		{
			base.OnInitialize();
			LineHeight = TextFont.MeasureString(" ").Y;
			questionPanel = new CombatPetsQuizUIPanel();
			answerPanel = new CombatPetsQuizUIPanel();
			Append(questionPanel);
			Append(answerPanel);
			questionLines = new List<string>
			{
				"What is your favorite color?"
			};
			answerLines = new List<string>
			{
				"Red.",
				"Green.",
				"Blue."
			};
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if(!IsActive) { return; }
			base.DrawSelf(spriteBatch);
		}
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			if(!IsActive) { return; }
			base.DrawChildren(spriteBatch);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			questionPanel.TextLines = questionLines;
			answerPanel.TextLines = answerLines;
			AnchorToScreenPosition();
		}

		private void AnchorToScreenPosition()
		{
			Vector2 answerPanelSize = answerPanel.MeasureLines();
			answerPanelSize.X = Math.Max(answerPanelSize.X, MinTextboxWidth);
			// No real reason to do this all in one method, but...

			// Set the size of the parent container
			float screenCenterX = Main.screenWidth / 2;
			float totalHeight = QuestionPanelHeight + answerPanelSize.Y + 5 * MarginSize;
			// fudge factor for margins
			Top.Pixels = Main.screenHeight - totalHeight;
			Height.Pixels = totalHeight;
			Left.Pixels = screenCenterX - MaxTextboxWidth / 2;
			Width.Pixels = MaxTextboxWidth;

			// Set the size of the narration text box (fixed)
			questionPanel.Top.Pixels = Height.Pixels - QuestionPanelHeight - 2 * MarginSize;
			questionPanel.Height.Pixels = QuestionPanelHeight;
			questionPanel.Left.Pixels = 0;
			questionPanel.Width.Pixels = MaxTextboxWidth;

			// Set the size of the answer options text box dynamic based on text
			answerPanel.Top.Pixels = 0;
			answerPanel.Height.Pixels = answerPanelSize.Y + 2 * MarginSize;
			answerPanel.Left.Pixels = MaxTextboxWidth - answerPanelSize.X - 2 * MarginSize;
			answerPanel.Width.Pixels = answerPanelSize.X + 2 * MarginSize;

		}


	}

	class CombatPetsQuizUIPanel : UIPanel
	{

		// todo change this dynamically
		internal List<string> TextLines = new List<string>();


		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			base.DrawChildren(spriteBatch);
			for(int i = 0; i < TextLines.Count; i++)
			{
				DrawText(spriteBatch, TextLines[i], i);
			}
		}

		public Vector2 MeasureLines()
		{
			float ySum = LineHeight * TextLines.Count;
			float xMax = TextLines.Select(tl => TextFont.MeasureString(tl).X).Max();
			return new(xMax, ySum);
		}


		private void DrawText(SpriteBatch spriteBatch, string text, int line)
		{
			// TODO line wrap? Maybe?
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			float xPos = Parent.Left.Pixels + Left.Pixels + MarginSize;
			float yPos = Parent.Top.Pixels + Top.Pixels + line * LineHeight + MarginSize;
			// budget text outline
			Vector2 pos = new Vector2(xPos, yPos);
			for(int i = -1; i < 2; i += 2)
			{
				for(int j = -1; j < 2; j += 2)
				{
					spriteBatch.DrawString(font, text, pos + new Vector2(i, j), Color.Black);
				}
			}
			spriteBatch.DrawString(font, text, pos, Color.White);
		}
	}
}
