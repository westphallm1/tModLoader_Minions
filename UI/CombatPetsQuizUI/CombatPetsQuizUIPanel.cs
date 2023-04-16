using AmuletOfManyMinions.Core.Minions.CombatPetsQuiz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
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

		private TextButton nextButton;

		public CombatPetsQuizModPlayer ModPlayer => Main.player[Main.myPlayer].GetModPlayer<CombatPetsQuizModPlayer>();
		public bool IsActive => ModPlayer.IsTakingQuiz;

		// this is a very hacky way to implement text scrolling
		private string lastDisplayedText = default;
		private int textSwitchTime = 0;

		public override void OnInitialize()
		{
			base.OnInitialize();
			LineHeight = TextFont.MeasureString(" ").Y;
			questionPanel = new CombatPetsQuizUIPanel();
			answerPanel = new CombatPetsQuizUIPanel() { AllowClickText = true };
			nextButton = new TextButton() { Text = "Next" };
			Append(questionPanel);
			Append(answerPanel);
			Append(nextButton);
			answerPanel.OnLeftMouseUp += this.AnswerPanel_OnMouseUp;
			nextButton.OnLeftMouseUp += this.NextButton_OnMouseUp;
		}

		private void NextButton_OnMouseUp(UIMouseEvent evt, UIElement listeningElement)
		{
			ModPlayer.AdvanceDialog();
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		private void AnswerPanel_OnMouseUp(UIMouseEvent evt, UIElement listeningElement)
		{
			if(answerPanel.HighlightedLine > -1)
			{
				ModPlayer.AnswerQuestion(answerPanel.HighlightedLine);
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if(!IsActive) { return; }
			base.DrawSelf(spriteBatch);
			if(ModPlayer.ShouldShowPortrait)
			{
				DrawPortrait(spriteBatch);
			}
		}
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			if(!IsActive) { return; }
			base.DrawChildren(spriteBatch);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if(IsActive)
			{
				var currentText = ModPlayer.CurrentQuiz.CurrentDialogText;
				if(lastDisplayedText != currentText)
				{
					textSwitchTime = (int)Main.GameUpdateCount;
					lastDisplayedText = currentText;
				}
				int charactersToDisplay = Math.Min(currentText.Length, 2 * (int)(Main.GameUpdateCount - textSwitchTime));
				if (charactersToDisplay < 0)
				{
					//Fallback when in MP Main.GameUpdateCount behaves weirdly, causing the calculated index to be negative
					charactersToDisplay = currentText.Length;
				}
				questionPanel.TextLines = TextFont.CreateWrappedText(
					currentText.Substring(0, charactersToDisplay), MaxTextboxWidth - 4 * MarginSize).Split('\n');
				if(charactersToDisplay == currentText.Length && ModPlayer.CurrentQuiz.CurrentState == QuizState.QUIZ)
				{
					answerPanel.TextLines = ModPlayer.CurrentQuiz.CurrentQuestion.AnswerTexts.Select(t => t.ToString()).ToArray();
				} else
				{
					answerPanel.TextLines = default;
				}
				AnchorToScreenPosition();
			} else
			{
				// shove off of the bottom of the screen
				Top.Pixels = Main.screenHeight;
				return;
			}
		}

		private void AnchorToScreenPosition()
		{

			// Set the size of the parent container
			float answerPanelHeight = answerPanel.MeasureLines().Y;
			float screenCenterX = Main.screenWidth / 2;
			float totalHeight = QuestionPanelHeight + answerPanelHeight + 5 * MarginSize;
			// fudge factor for margins
			Top.Pixels = Main.screenHeight - totalHeight;
			Height.Pixels = totalHeight;
			Left.Pixels = screenCenterX - MaxTextboxWidth / 2;
			Width.Pixels = MaxTextboxWidth;

			// No real reason to do this all in one method, but...
			AnchorQuestionPanel();
			AnchorAnswerPanel();
			AnchorNextButton();

		}

		internal void AnchorQuestionPanel()
		{
			// Set the size of the narration text box (fixed)
			questionPanel.Top.Pixels = Height.Pixels - QuestionPanelHeight - 2 * MarginSize;
			questionPanel.Height.Pixels = QuestionPanelHeight;
			questionPanel.Left.Pixels = 0;
			questionPanel.Width.Pixels = MaxTextboxWidth;
		}

		internal void AnchorAnswerPanel()
		{
			// Set the size of the answer options text box dynamically based on text
			Vector2 answerPanelSize = answerPanel.MeasureLines();
			answerPanelSize.X = Math.Max(answerPanelSize.X + 4 * MarginSize, MinTextboxWidth);
			if(answerPanel.IsEmpty)
			{
				// shove off of the bottom of the screen
				answerPanel.Top.Pixels = Main.screenHeight;
			} else
			{
				answerPanel.Top.Pixels = 0;
				answerPanel.Height.Pixels = answerPanelSize.Y + 2 * MarginSize;
				answerPanel.Left.Pixels = MaxTextboxWidth - answerPanelSize.X - 2 * MarginSize;
				answerPanel.Width.Pixels = answerPanelSize.X + 2 * MarginSize;
			}
		}

		internal void AnchorNextButton()
		{
			if(ModPlayer.CurrentQuiz.CurrentState == QuizState.QUIZ)
			{
				nextButton.Top.Pixels = Main.screenHeight;
				return;
			} 
			Vector2 nextButtonSize = nextButton.MeasureText();
			nextButton.Top.Pixels = questionPanel.Top.Pixels + QuestionPanelHeight - MarginSize - nextButtonSize.Y;
			nextButton.Left.Pixels = questionPanel.Left.Pixels + MaxTextboxWidth - 2 * MarginSize - nextButtonSize.X;
			nextButton.Width.Pixels = nextButtonSize.X;
			nextButton.Height.Pixels = nextButtonSize.Y;
		}

		internal void DrawPortrait(SpriteBatch spriteBatch)
		{
			Texture2D texture = ModPlayer.PortraitTexture.Value;
			float xPos = Left.Pixels + Width.Pixels - texture.Width;
			float yPos = Top.Pixels + questionPanel.Top.Pixels - texture.Height - MarginSize;
			spriteBatch.Draw(texture, new Vector2(xPos, yPos), texture.Bounds, Color.White, 0, default, 1, SpriteEffects.FlipHorizontally, 0);
		}
	}
	
	class TextButton : UIElement
	{
		internal string Text { get; set; }

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			float xPos = Parent.Left.Pixels + Left.Pixels;
			float yPos = Parent.Top.Pixels + Top.Pixels;
			Vector2 pos = new(xPos, yPos);
			// budget text outline
			Color textColor = IsMouseHovering ? Color.Yellow : Color.White;
			CombatPetsQuizUIPanel.DrawText(spriteBatch, Text, pos, textColor);
		}

		internal Vector2 MeasureText() => TextFont.MeasureString(Text);

	}

	class CombatPetsQuizUIPanel : UIPanel
	{

		// todo change this dynamically
		internal string[] TextLines;

		internal bool AllowClickText;

		internal int HighlightedLine { get; private set; } = -1;

		internal bool IsEmpty => (TextLines?.Length ?? 0) == 0;
		
		public override void OnInitialize()
		{
			base.OnInitialize();
			IgnoresMouseInteraction = false;
		}

		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			if(IsEmpty)
			{
				return;
			}
			base.DrawChildren(spriteBatch);
			for(int i = 0; i < TextLines.Length; i++)
			{
				DrawText(spriteBatch, TextLines[i], i);
			}
		}

		public Vector2 MeasureLines()
		{
			if(IsEmpty)
			{
				return default;
			}
			float ySum = LineHeight * TextLines.Length;
			float xMax = TextLines.Select(tl => TextFont.MeasureString(tl).X).Max();
			return new(xMax, ySum);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if(AllowClickText && TextLines != default && ContainsPoint(Main.MouseScreen))
			{
				float top = Parent.Top.Pixels + Top.Pixels + MarginSize;
				HighlightedLine = (int)((Main.MouseScreen.Y - top) / LineHeight);
			} else
			{
				HighlightedLine = -1;
			}
		}

		private void DrawText(SpriteBatch spriteBatch, string text, int line)
		{
			float xPos = Parent.Left.Pixels + Left.Pixels + 2 * MarginSize;
			float yPos = Parent.Top.Pixels + Top.Pixels + line * LineHeight + MarginSize;
			Vector2 pos = new(xPos, yPos);
			// budget text outline
			Color textColor = line == HighlightedLine ? Color.Yellow : Color.White;
			DrawText(spriteBatch, text, pos, textColor);
		}


		public static void DrawText(SpriteBatch spriteBatch, string text, Vector2 pos, Color textColor)
		{
			for(int i = -1; i < 2; i += 2)
			{
				for(int j = -1; j < 2; j += 2)
				{
					spriteBatch.DrawString(TextFont, text, pos + new Vector2(i, j), Color.Black);
				}
			}
			spriteBatch.DrawString(TextFont, text, pos, textColor);
		}
	}

	public class CombatQuizClickSuppressor: GlobalItem
	{
		// TODO figure out why item usage needs to be manually suppressed
		public override bool CanUseItem(Item item, Player player)
		{
			if(player.whoAmI == Main.myPlayer && UserInterfaces.quizUI.Children.Any(e=>e.IsMouseHovering))
			{
				return false;
			}
			return base.CanUseItem(item, player);
		}
	}
}
