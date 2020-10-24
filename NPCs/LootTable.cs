using AmuletOfManyMinions.Projectiles.Minions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Squires.Squeyere;
using AmuletOfManyMinions.Projectiles.Squires.BoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire;
using AmuletOfManyMinions.Projectiles.Squires.AncientCobaltSquire;
using System.Linq;

namespace AmuletOfManyMinions.NPCs
{
    class LootTable : GlobalNPC
    {
        public override void NPCLoot(NPC npc)
        {
            base.NPCLoot(npc);
            float spawnChance = Main.rand.NextFloat();
            if(spawnChance < 0.10f && npc.type == NPCID.Eyezor)
            {
                Item.NewItem(npc.getRect(), ItemType<SqueyereMinionItem>(), 1);
            }

            if(spawnChance < 0.01f && (npc.type == NPCID.AngryBones  || 
               npc.type == NPCID.AngryBonesBig  || 
               npc.type == NPCID.AngryBonesBigHelmet || 
               npc.type == NPCID.AngryBonesBigMuscle))
            {
                Item.NewItem(npc.getRect(), ItemType<BoneSquireMinionItem>(), 1);
            }

            if(spawnChance < 0.03f && (npc.type == NPCID.BlueArmoredBones || 
               npc.type == NPCID.BlueArmoredBonesMace || 
               npc.type == NPCID.BlueArmoredBonesNoPants || 
               npc.type == NPCID.BlueArmoredBonesSword))
            {
                Item.NewItem(npc.getRect(), ItemType<ArmoredBoneSquireMinionItem>(), 1);
            }

            if(spawnChance < 0.08f && npc.type == NPCID.ManEater)
            {
                Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1);
            }
            if(spawnChance < 0.03f && npc.TypeName == "Hornet" )
            {
                Item.NewItem(npc.getRect(), ItemType<AncientCobaltSquireMinionItem>(), 1);
            }

        }
    }
}
