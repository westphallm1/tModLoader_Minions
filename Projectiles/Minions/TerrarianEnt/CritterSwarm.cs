using AmuletOfManyMinions.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;


namespace AmuletOfManyMinions.Projectiles.Minions.TerrarianEnt
{
	/// <summary>
	/// Uses ai[0] for npc target, ai[1] for "biome" to determine appearance
	/// </summary>

	class CritterSwarmProjectile : ModProjectile
	{
		// npc to stay on top of
		NPC clingTarget;

		private List<SwarmCritter> critters = new List<SwarmCritter>();

		// length of 'spawn' animation before damage dealing starts
		internal static int SpawnFrames = 45;

		public override string Texture => "Terraria/Images/Item_0";

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.timeLeft = 180;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(Projectile.localAI[0] < SpawnFrames)
			{
				return false;
			}
			for(int i = 0; i < critters.Count; i++)
			{
				Vector2 critterCenter = Projectile.Center + critters[i].offset;
				if(targetHitbox.Contains(critterCenter.ToPoint()))
				{
					return true;
				}
			}
			return false;
		}

		public override void AI()
		{
			base.AI();
			Projectile.localAI[0]++;
			// failsafe in case we got a bad NPC index
			if (Projectile.ai[0] == 0)
			{
				Projectile.Kill();
				return; 
			}
			// "on spawn" code
			if (clingTarget == null)
			{
				clingTarget = Main.npc[(int)Projectile.ai[0]];
				int radius = Math.Max(96, (clingTarget.width + clingTarget.height) / 2);
				//TODO 1.4 test if drawing these does not crash
				critters = CritterConfigs.GetCrittersForBiome((int)Projectile.ai[1], radius, Main.rand.Next(3, 5));
			}
			if (clingTarget.active)
			{
				Projectile.Center = clingTarget.Center;
			} 
			for(int i = 0; i < critters.Count; i++)
			{
				critters[i].Update((int)Projectile.localAI[0]);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float colorMult = Math.Min(0.75f, Math.Min(Projectile.timeLeft, Projectile.localAI[0]) / SpawnFrames);
			lightColor = Color.White * colorMult;
			for(int i = 0; i < critters.Count; i++)
			{
				critters[i].Draw(lightColor, (int)Projectile.localAI[0], Projectile.Center);
			}
			return false;
		}
	}

	internal static class CritterConfigs
	{
		internal delegate List<SwarmCritter> SwarmGenerator(int radius, int count);
		internal static Dictionary<int, SwarmGenerator[]> generators;
		public static void Load()
		{
			generators = new Dictionary<int, SwarmGenerator[]>();
			// forest
			generators[0] = new SwarmGenerator[]{GetBirdCritters, GetButterflyCritters};
			generators[1] = new SwarmGenerator[]{GetBirdCritters, GetButterflyCritters};
			// beach
			generators[2] = new SwarmGenerator[]{ GetGoldfish, GetJellyfish };
			// ice
			generators[3] = new SwarmGenerator[]{ GetIceBats, GetIceBats, GetBirdCritters };
			// jungle
			generators[4] = new SwarmGenerator[]{ GetBees, GetJungleBats, GetBuggys };
			// hallowed
			generators[5] = new SwarmGenerator[]{ GetHallowedEnemies, GetLightningBugs };
		}

		public static void Unload()
		{
			generators = null;
		}

		internal static List<SwarmCritter> GetCrittersForBiome(int biomeIdx, int radius, int count)
		{
			SwarmGenerator[] generator = generators[biomeIdx];
			return generator?[Main.rand.Next(generator?.Length ?? 0)]?.Invoke(radius, count) ?? new List<SwarmCritter>();
		}

		private static List<SwarmCritter> GetRandomElipseCritters(
			int[] critterTypes, int majorRadius, int nFrames = 5, int minFrame = 0, int maxFrame = 4, int count = 5)
		{
			List<SwarmCritter> critters = new List<SwarmCritter>();
			for(int i = 0; i < count; i++)
			{
				int critterType = Main.rand.Next(critterTypes);
				Main.instance.LoadNPC(critterType);
				Texture2D texture = TextureAssets.Npc[critterType].Value;
				critters.Add(new ElipseFlyingCritter(texture, i, nFrames, 
					minFrame: minFrame, maxFrame: maxFrame, 
					majorRadius: majorRadius, swarmSize: count));
			}
			return critters;
		}

		public static List<SwarmCritter> GetIceBats(int majorRadius, int count) =>
			GetRandomElipseCritters(new int[] { NPCID.IceBat }, majorRadius, nFrames: 4, count: count);

		public static List<SwarmCritter> GetHallowedEnemies(int majorRadius, int count)
		{
			var critters = new List<SwarmCritter>();
			for(int i = 0; i < count; i++)
			{
				if(Main.rand.Next(3) > 0)
				{
					critters.AddRange(GetRandomElipseCritters(new int[] { NPCID.IlluminantBat, NPCID.Pixie }, majorRadius, nFrames: 4, count: 1));
				} else
				{
					critters.AddRange(GetRandomElipseCritters(new int[] { NPCID.Gastropod }, majorRadius, nFrames: 11, maxFrame: 3, count: 1));
				}
				ElipseFlyingCritter added = (ElipseFlyingCritter)critters[critters.Count - 1];
				added.idx = i;
				added.swarmSize = count;
				added.scale = 0.75f;
			}
			return critters;
		}
			

		public static List<SwarmCritter> GetJungleBats(int majorRadius, int count) =>
			GetRandomElipseCritters(new int[] { NPCID.JungleBat }, majorRadius, nFrames: 5, count: count);

		public static List<SwarmCritter> GetLightningBugs(int majorRadius, int count) =>
			GetRandomElipseCritters(new int[] { NPCID.LightningBug }, majorRadius, nFrames: 4, maxFrame: 2, count: count);

		public static List<SwarmCritter> GetBees(int majorRadius, int count) =>
			GetRandomElipseCritters(new int[] { NPCID.Bee, NPCID.BeeSmall }, majorRadius, nFrames: 4, count: count);

		public static List<SwarmCritter> GetBuggys(int majorRadius, int count) =>
			GetRandomElipseCritters(new int[] { NPCID.Sluggy, NPCID.Grubby, NPCID.Sluggy }, majorRadius, nFrames: 4, count: count);

		public static List<SwarmCritter> GetGoldfish(int majorRadius, int count)
		{
			List<SwarmCritter> critters = GetRandomElipseCritters(
				new int[] { NPCID.Goldfish, NPCID.Piranha, NPCID.AnglerFish }, majorRadius, nFrames: 6, count: count);
			critters.ForEach(c => { 
				var ec = (ElipseFlyingCritter)c;
				ec.faceVelocity = true;
				ec.baseRotation = MathHelper.Pi;
				ec.scale = 0.75f;
			});
			return critters;
		}

		public static List<SwarmCritter> GetJellyfish(int majorRadius, int count)
		{
			// the number of npcs for which minframe = 0, maxframe = 4 is impressive
			List<SwarmCritter> critters = GetRandomElipseCritters(
				new int[] { NPCID.BlueJellyfish, NPCID.PinkJellyfish, NPCID.GreenJellyfish },
				majorRadius, nFrames: 7, count: count);
			critters.ForEach(c => {
				var ec = (ElipseFlyingCritter)c;
				ec.faceVelocity = true;
				ec.baseRotation = MathHelper.PiOver2;
				ec.frameSpeed = 10;
				ec.scale = 0.75f;
			});
			return critters;
		}

		public static List<SwarmCritter> GetBirdCritters(int majorRadius, int count)
		{
			return GetRandomElipseCritters(new int[] { NPCID.Bird, NPCID.BirdBlue, NPCID.BirdRed }, majorRadius, count: count);
		}

		public static List<SwarmCritter> GetButterflyCritters(int majorRadius, int count)
		{
			Main.instance.LoadNPC(NPCID.Butterfly);
			Texture2D texture = TextureAssets.Npc[NPCID.Butterfly].Value;
			List<SwarmCritter> critters = new List<SwarmCritter>();
			int butterflyTypes = 8;
			for(int i = 0; i < count; i++)
			{
				int critterType = Main.rand.Next(butterflyTypes);
				critters.Add(new ElipseFlyingCritter(texture, i, 24, 
					minFrame: 3 * critterType, maxFrame: 3 * critterType + 3,
					majorRadius: majorRadius, swarmSize: count, animationSpeed: 90));
			}
			return critters;
		}
	}

	internal abstract class SwarmCritter
	{
		internal Texture2D texture;
		internal SpriteEffects effects;
		internal float rotation;
		internal int idx;
		internal float scale = 1;
		internal Vector2 offset;

		internal SwarmCritter(Texture2D texture, int idx)
		{
			this.texture = texture;
			this.idx = idx;
		}

		internal abstract void Update(int frame);
		internal abstract Rectangle GetBounds(int frame);

		internal void Draw(Color lightColor, int frame, Vector2 center)
		{
			Rectangle bounds = GetBounds(frame);
			Vector2 pos = center + offset;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition, bounds, lightColor, rotation, bounds.GetOrigin(), scale, effects, 0);
		}
	}

	internal class ElipseFlyingCritter : SwarmCritter
	{
		private int nFrames;
		private int majorRadius;
		internal int swarmSize;
		private int minFrame;
		private int maxFrame;
		private Vector2 velocity;
		private float eccentricity;

		internal int frameSpeed = 5;
		internal float baseRotation;
		internal bool lToR;
		internal bool faceVelocity;
		internal int animationSpeed;

		internal ElipseFlyingCritter(Texture2D texture, int idx, int nFrames, 
			int majorRadius = 96, int swarmSize = 5, 
			int minFrame = 0, int maxFrame = 4, int animationSpeed = 60) : base(texture, idx)
		{
			this.nFrames = nFrames;
			this.majorRadius = majorRadius;
			this.swarmSize = swarmSize;
			this.minFrame = minFrame;
			this.maxFrame = maxFrame;
			this.animationSpeed = animationSpeed;
			eccentricity = 0.1f;
		}

		internal override Rectangle GetBounds(int frame)
		{
			int curFrame = (frame / frameSpeed) % (maxFrame - minFrame) + minFrame;
			int frameHeight = texture.Height / nFrames;
			return new Rectangle(0, curFrame * frameHeight, texture.Width, frameHeight);
		}

		internal override void Update(int frame)
		{
			float baseAngle = MathHelper.TwoPi * idx / swarmSize;
			float angle = baseAngle + MathHelper.TwoPi * frame / animationSpeed;
			Vector2 nextOffset = angle.ToRotationVector2() * majorRadius;
			nextOffset.Y *= eccentricity;
			// this may or may not work
			nextOffset = nextOffset.RotatedBy(baseAngle);
			velocity = nextOffset - offset;
			rotation = faceVelocity ? velocity.ToRotation() + baseRotation : velocity.X * 0.05f;
			effects = velocity.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			offset = nextOffset;
		}
	}
}
